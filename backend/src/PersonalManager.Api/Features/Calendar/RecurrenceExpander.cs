using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Calendar;

/// <summary>把事件（含重複規則）展開成落在查詢區間內的每一次實際發生時間。</summary>
public static class RecurrenceExpander
{
    /// <summary>單一事件在一次查詢中最多展開的次數，避免異常資料造成大量運算。</summary>
    public const int MaxOccurrencesPerEvent = 500;

    /// <summary>
    /// 回傳與 [<paramref name="from"/>, <paramref name="to"/>) 重疊的發生時間。
    /// 第 n 次一律由原始開始時間推算（例如每月 31 日：1/31 → 2/28 → 3/31），不會逐次累積偏移。
    /// </summary>
    public static IEnumerable<(DateTime Start, DateTime End)> Expand(
        DateTime start, DateTime end, Recurrence recurrence, DateOnly? until, DateTime from, DateTime to)
    {
        var duration = end - start;

        if (recurrence == Recurrence.None)
        {
            if (Overlaps(start, end, from, to))
                yield return (start, end);
            yield break;
        }

        var produced = 0;
        for (var n = FirstCandidateIndex(start, duration, recurrence, from); produced < MaxOccurrencesPerEvent; n++)
        {
            var occurrenceStart = NthStart(start, recurrence, n);
            if (occurrenceStart >= to || (until is not null && DateOnly.FromDateTime(occurrenceStart) > until))
                yield break;

            var occurrenceEnd = occurrenceStart + duration;
            if (Overlaps(occurrenceStart, occurrenceEnd, from, to))
            {
                produced++;
                yield return (occurrenceStart, occurrenceEnd);
            }
        }
    }

    private static bool Overlaps(DateTime start, DateTime end, DateTime from, DateTime to) => start < to && end >= from;

    private static DateTime NthStart(DateTime start, Recurrence recurrence, int n) => recurrence switch
    {
        Recurrence.Daily => start.AddDays(n),
        Recurrence.Weekly => start.AddDays(7 * n),
        Recurrence.Monthly => start.AddMonths(n),
        Recurrence.Yearly => start.AddYears(n),
        _ => throw new ArgumentOutOfRangeException(nameof(recurrence))
    };

    /// <summary>固定週期的規則直接跳到查詢區間附近，不必從多年前逐次計算。</summary>
    private static int FirstCandidateIndex(DateTime start, TimeSpan duration, Recurrence recurrence, DateTime from)
    {
        var periodDays = recurrence switch { Recurrence.Daily => 1, Recurrence.Weekly => 7, _ => 0 };
        var earliestRelevantStart = from - duration;
        if (periodDays == 0 || earliestRelevantStart <= start)
            return 0;
        return Math.Max(0, (int)((earliestRelevantStart - start).TotalDays / periodDays) - 1);
    }
}
