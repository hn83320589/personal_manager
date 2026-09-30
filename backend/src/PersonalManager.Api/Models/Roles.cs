namespace PersonalManager.Api.Models;

public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { User, Admin };
}
