using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PersonalManager.Api.Common;

namespace PersonalManager.Tests.Common;

public class CurrentUserTests
{
    private static ICurrentUser For(params Claim[] claims)
    {
        var identity = claims.Length == 0 ? new ClaimsIdentity() : new ClaimsIdentity(claims, "Test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        return new HttpCurrentUser(new FixedContextAccessor(context));
    }

    /// <summary>
    /// HttpContextAccessor 以 static AsyncLocal 儲存 context，同一個測試裡建立兩個會互相覆蓋，
    /// 所以測試用每個實例各自持有 context 的版本。
    /// </summary>
    private sealed class FixedContextAccessor(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    [Fact]
    public void SignedInUser_ExposesIdFromNameIdentifierClaim()
    {
        var user = For(new Claim(ClaimTypes.NameIdentifier, "42"));

        Assert.Equal(42, user.UserId);
        Assert.Equal(42, user.RequireUserId());
    }

    [Fact]
    public void Anonymous_HasNoIdAndRequireUserIdThrowsUnauthenticated()
    {
        var user = For();

        Assert.Null(user.UserId);
        Assert.Throws<UnauthenticatedException>(() => user.RequireUserId());
    }

    [Fact]
    public void AdminRoleClaim_MakesUserAdmin()
    {
        var admin = For(new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Admin"));
        var member = For(new Claim(ClaimTypes.NameIdentifier, "2"), new Claim(ClaimTypes.Role, "User"));

        Assert.True(admin.IsAdmin);
        Assert.False(member.IsAdmin);
    }
}
