using System.Net;
using System.Text.Json;
using PersonalManager.Tests.Infrastructure;

namespace PersonalManager.Tests.Features;

public class ContactMethodsApiTests(ApiFactory factory) : OwnedCollectionContract(factory)
{
    protected override string Resource => "contact-methods";

    protected override object NewItem(string label, bool isPublic = true) =>
        new { type = "Instagram", label, value = "@yingching.design", isPublic };

    protected override string LabelOf(JsonElement item) => item.GetProperty("label").GetString()!;

    [Theory]
    [InlineData("Behance", "behance.net/yingching")]
    [InlineData("Dribbble", "https://dribbble.com/yingching")]
    [InlineData("Website", "https://yingching.example")]
    [InlineData("Email", "hello@yingching.example")]
    [InlineData("Phone", "+886 912-345-678")]
    public async Task Create_AcceptsCommonPlatforms(string type, string value)
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl, new { type, value });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("Website", "javascript:alert(document.cookie)")]
    [InlineData("GitHub", "JavaScript:alert(1)")]
    [InlineData("Other", "data:text/html,<script>alert(1)</script>")]
    public async Task Create_WithScriptOrDataUrl_IsBadRequest(string type, string value)
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl, new { type, value });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("Email", "not-an-email")]
    [InlineData("Phone", "call me maybe")]
    public async Task Create_WithValueNotMatchingType_IsBadRequest(string type, string value)
    {
        var me = await Factory.CreateUserAsync();

        var response = await me.Client().PostJsonAsync(MeUrl, new { type, value });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
