using System.Net;
using System.Text.Json;

namespace PersonalManager.Tests.Infrastructure;

/// <summary>
/// 「使用者自己的清單」類資源在後台（/api/me）共同必須遵守的行為。
/// 每個資源繼承這個類別並提供路由與範例資料，就會自動跑完整組測試；
/// 資源特有的規則（驗證、欄位）寫在繼承的類別裡。有公開頁面的資源改繼承 <see cref="PublicCollectionContract"/>。
/// </summary>
public abstract class OwnedCollectionContract(ApiFactory factory) : IClassFixture<ApiFactory>
{
    protected ApiFactory Factory { get; } = factory;

    /// <summary>路由片段，例如 <c>skills</c> → <c>/api/me/skills</c>、<c>/api/public/users/{u}/skills</c>。</summary>
    protected abstract string Resource { get; }

    /// <summary>
    /// 建立一筆合法的資料；<paramref name="label"/> 必須出現在 <see cref="LabelOf"/> 讀得到的欄位。
    /// 沒有公開頁面的資源可忽略 <paramref name="isPublic"/>。
    /// </summary>
    protected abstract object NewItem(string label, bool isPublic = true);

    protected abstract string LabelOf(JsonElement item);

    protected string MeUrl => $"/api/me/{Resource}";
    protected string PublicUrl(string username) => $"/api/public/users/{username}/{Resource}";

    protected Task<JsonElement> CreateAsync(TestUser user, object body) => user.PostCreatedAsync(MeUrl, body);

    protected async Task<List<string>> MyLabelsAsync(TestUser user) =>
        (await (await user.Client().GetAsync(MeUrl)).ReadDataAsync<List<JsonElement>>()).Select(LabelOf).ToList();

    [Fact]
    public async Task Me_Anonymous_IsUnauthorized()
    {
        var response = await Factory.CreateClient().GetAsync(MeUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_List_ReturnsOnlyOwnItemsIncludingPrivate()
    {
        var me = await Factory.CreateUserAsync();
        var other = await Factory.CreateUserAsync();
        await CreateAsync(me, NewItem("公開的"));
        await CreateAsync(me, NewItem("私人的", isPublic: false));
        await CreateAsync(other, NewItem("別人的"));

        Assert.Equal(["公開的", "私人的"], await MyLabelsAsync(me));
    }

    [Fact]
    public async Task Me_UpdateOrDeleteSomeoneElsesItem_IsNotFoundAndLeavesItUnchanged()
    {
        var owner = await Factory.CreateUserAsync();
        var intruder = await Factory.CreateUserAsync();
        var item = await CreateAsync(owner, NewItem("原始內容"));

        var update = await intruder.Client().PutJsonAsync($"{MeUrl}/{item.Id()}", NewItem("被竄改"));
        var delete = await intruder.Client().DeleteAsync($"{MeUrl}/{item.Id()}");

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal(["原始內容"], await MyLabelsAsync(owner));
    }

    [Fact]
    public async Task Me_Update_ChangesOwnItem()
    {
        var me = await Factory.CreateUserAsync();
        var item = await CreateAsync(me, NewItem("舊的"));

        var response = await me.Client().PutJsonAsync($"{MeUrl}/{item.Id()}", NewItem("新的"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["新的"], await MyLabelsAsync(me));
    }

    [Fact]
    public async Task Me_Delete_RemovesOwnItem()
    {
        var me = await Factory.CreateUserAsync();
        var item = await CreateAsync(me, NewItem("要刪除的"));

        var response = await me.Client().DeleteAsync($"{MeUrl}/{item.Id()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await MyLabelsAsync(me));
    }

    [Fact]
    public async Task Me_Reorder_AppliesGivenOrder()
    {
        var me = await Factory.CreateUserAsync();
        var a = await CreateAsync(me, NewItem("A"));
        var b = await CreateAsync(me, NewItem("B"));
        var c = await CreateAsync(me, NewItem("C"));

        var response = await me.Client().PutJsonAsync($"{MeUrl}/order", new { ids = new[] { c.Id(), a.Id(), b.Id() } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["C", "A", "B"], await MyLabelsAsync(me));
    }

    [Fact]
    public async Task Me_Reorder_WithSomeoneElsesItem_IsBadRequest()
    {
        var me = await Factory.CreateUserAsync();
        var other = await Factory.CreateUserAsync();
        var mine = await CreateAsync(me, NewItem("我的"));
        var theirs = await CreateAsync(other, NewItem("別人的"));

        var response = await me.Client().PutJsonAsync($"{MeUrl}/order", new { ids = new[] { theirs.Id(), mine.Id() } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

/// <summary>同時有公開頁面（/api/public/users/{username}/...）的清單型資源：額外驗證公開資料的過濾。</summary>
public abstract class PublicCollectionContract(ApiFactory factory) : OwnedCollectionContract(factory)
{
    [Fact]
    public async Task Public_ReturnsOnlyPublicItemsInDisplayOrder()
    {
        var owner = await Factory.CreateUserAsync();
        await CreateAsync(owner, NewItem("第一"));
        await CreateAsync(owner, NewItem("隱藏", isPublic: false));
        await CreateAsync(owner, NewItem("第二"));

        var items = await (await Factory.CreateClient().GetAsync(PublicUrl(owner.Username)))
            .ReadDataAsync<List<JsonElement>>();

        Assert.Equal(["第一", "第二"], items.Select(LabelOf));
    }

    [Fact]
    public async Task Public_UnknownOrInactiveUser_IsNotFound()
    {
        var inactive = await Factory.CreateUserAsync(isActive: false);

        var unknown = await Factory.CreateClient().GetAsync(PublicUrl("nobody-here"));
        var hidden = await Factory.CreateClient().GetAsync(PublicUrl(inactive.Username));

        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }
}
