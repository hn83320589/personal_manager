using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Contacts;

public sealed partial record SaveContactMethodRequest(
    ContactType Type,
    [StringLength(50)] string? Label,
    [Required(ErrorMessage = "請輸入聯絡資訊"), StringLength(500)] string Value,
    bool IsPublic = true) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var value = Value?.Trim() ?? "";
        var error = Type switch
        {
            ContactType.Email when !new EmailAddressAttribute().IsValid(value) => "Email 格式不正確",
            ContactType.Phone when !PhonePattern().IsMatch(value) => "電話只能包含數字、空白、+、-、( )",
            _ when HasUnsafeScheme(value) => "連結只能是 http 或 https 網址",
            _ => null
        };
        if (error is not null)
            yield return new ValidationResult(error, [nameof(Value)]);
    }

    /// <summary>
    /// 前台會把聯絡方式渲染成連結；帶有 http(s) 以外協定的值（javascript:、data: 等）點擊後會執行程式碼。
    /// 不含協定的帳號名稱（例如 github.com/xxx、@handle）則允許，由前台補上連結。
    /// </summary>
    private static bool HasUnsafeScheme(string value)
    {
        var scheme = SchemePattern().Match(value);
        return scheme.Success && scheme.Groups[1].Value.ToLowerInvariant() is not ("http" or "https");
    }

    [GeneratedRegex(@"^\s*([a-zA-Z][a-zA-Z0-9+.\-]*):")]
    private static partial Regex SchemePattern();

    [GeneratedRegex(@"^\+?[0-9\s\-()]{6,30}$")]
    private static partial Regex PhonePattern();
}

public sealed record ContactMethodDto(int Id, ContactType Type, string Label, string Value, bool IsPublic, int SortOrder);

public sealed record PublicContactMethodDto(int Id, ContactType Type, string Label, string Value);
