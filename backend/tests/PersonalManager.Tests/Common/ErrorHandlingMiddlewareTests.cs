using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PersonalManager.Api.Common;
using PersonalManager.Api.Middleware;

namespace PersonalManager.Tests.Common;

public class ErrorHandlingMiddlewareTests
{
    private static async Task<(int Status, JsonElement Body)> Run(Exception toThrow)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ErrorHandlingMiddleware(_ => throw toThrow, NullLogger<ErrorHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await JsonDocument.ParseAsync(context.Response.Body);
        return (context.Response.StatusCode, body.RootElement.Clone());
    }

    [Theory]
    [InlineData(typeof(NotFoundException), 404)]
    [InlineData(typeof(ForbiddenException), 403)]
    [InlineData(typeof(UnauthenticatedException), 401)]
    [InlineData(typeof(ConflictException), 409)]
    public async Task AppExceptions_MapToTheirStatusCodeAndMessage(Type exceptionType, int expectedStatus)
    {
        var exception = (AppException)Activator.CreateInstance(exceptionType, "自訂訊息")!;

        var (status, body) = await Run(exception);

        Assert.Equal(expectedStatus, status);
        Assert.Equal("自訂訊息", body.GetProperty("message").GetString());
        Assert.False(body.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task ValidationException_ReturnsBadRequestWithFieldErrors()
    {
        var (status, body) = await Run(new DomainValidationException("資料有誤", ["標題不可空白"]));

        Assert.Equal(400, status);
        Assert.Equal("標題不可空白", body.GetProperty("errors")[0].GetString());
    }

    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(KeyNotFoundException))]
    [InlineData(typeof(InvalidOperationException))]
    public async Task UnexpectedExceptions_ReturnGenericMessageWithoutInternalDetails(Type exceptionType)
    {
        var secret = "SELECT * FROM Users WHERE PasswordHash = 'x'";
        var exception = (Exception)Activator.CreateInstance(exceptionType, secret)!;

        var (status, body) = await Run(exception);

        Assert.Equal(500, status);
        Assert.DoesNotContain(secret, body.ToString());
    }
}
