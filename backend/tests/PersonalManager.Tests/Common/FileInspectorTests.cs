using PersonalManager.Api.Features.Files;
using PersonalManager.Api.Models;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Common;

public class FileInspectorTests
{
    public static TheoryData<string, byte[], FileKind> MatchingFiles => new()
    {
        { "logo.png", SampleFiles.Png(10, 10), FileKind.Image },
        { "photo.JPG", SampleFiles.Jpeg(10, 10), FileKind.Image },
        { "photo.jpeg", SampleFiles.Jpeg(10, 10), FileKind.Image },
        { "anim.gif", SampleFiles.Gif(10, 10), FileKind.Image },
        { "shot.webp", SampleFiles.WebPExtended(10, 10), FileKind.Image },
        { "brand-guide.pdf", SampleFiles.Pdf(), FileKind.Pdf },
        { "proposal.docx", SampleFiles.Zip(), FileKind.Word },
        { "old.doc", SampleFiles.OleCompound(), FileKind.Word },
        { "deck.pptx", SampleFiles.Zip(), FileKind.PowerPoint },
        { "budget.xlsx", SampleFiles.Zip(), FileKind.Excel },
        { "assets.zip", SampleFiles.Zip(), FileKind.Archive },
    };

    [Theory]
    [MemberData(nameof(MatchingFiles))]
    public void ContentMatchingExtension_IsAccepted(string fileName, byte[] content, FileKind expected)
    {
        var result = FileInspector.Inspect(fileName, content);

        Assert.True(result.IsAllowed, result.Error);
        Assert.Equal(expected, result.Kind);
    }

    [Theory]
    [InlineData("fake.png")]
    [InlineData("fake.pdf")]
    [InlineData("fake.docx")]
    public void ContentNotMatchingExtension_IsRejected(string fileName)
    {
        var result = FileInspector.Inspect(fileName, SampleFiles.Html());

        Assert.False(result.IsAllowed);
    }

    [Theory]
    [InlineData("icon.svg")]
    [InlineData("page.html")]
    [InlineData("setup.exe")]
    [InlineData("design.psd")]
    [InlineData("design.ai")]
    [InlineData("no-extension")]
    public void UnsupportedExtension_IsRejected(string fileName)
    {
        var result = FileInspector.Inspect(fileName, SampleFiles.Png(1, 1));

        Assert.False(result.IsAllowed);
    }

    [Theory]
    [InlineData("a.png", 1920, 1080)]
    [InlineData("a.jpg", 800, 1200)]
    [InlineData("a.gif", 320, 240)]
    [InlineData("a.webp", 3000, 2000)]
    public void ImageDimensions_AreRead(string fileName, int width, int height)
    {
        var content = Path.GetExtension(fileName) switch
        {
            ".png" => SampleFiles.Png(width, height),
            ".jpg" => SampleFiles.Jpeg(width, height),
            ".gif" => SampleFiles.Gif(width, height),
            _ => SampleFiles.WebPExtended(width, height)
        };

        var result = FileInspector.Inspect(fileName, content);

        Assert.Equal((width, height), (result.Width, result.Height));
    }

    [Fact]
    public void LosslessWebPDimensions_AreRead()
    {
        var result = FileInspector.Inspect("a.webp", SampleFiles.WebPLossless(1234, 567));

        Assert.Equal((1234, 567), (result.Width, result.Height));
    }

    [Fact]
    public void Documents_HaveNoDimensions()
    {
        var result = FileInspector.Inspect("a.pdf", SampleFiles.Pdf());

        Assert.Null(result.Width);
        Assert.Null(result.Height);
    }
}
