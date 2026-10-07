using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Contacts;

public sealed class ContactMethodService(ApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<List<PublicContactMethodDto>> GetPublicAsync(string username)
    {
        var userId = await db.RequirePublicUserIdAsync(username);
        return await db.ContactMethods.AsNoTracking().OwnedBy(userId)
            .Where(c => c.IsPublic)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => new PublicContactMethodDto(c.Id, c.Type, c.Label, c.Value))
            .ToListAsync();
    }

    public Task<List<ContactMethodDto>> GetMineAsync() =>
        db.ContactMethods.AsNoTracking().OwnedBy(currentUser.RequireUserId())
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => ToDto(c))
            .ToListAsync();

    public async Task<ContactMethodDto> CreateAsync(SaveContactMethodRequest request)
    {
        var userId = currentUser.RequireUserId();
        var contact = new ContactMethod { UserId = userId, SortOrder = await db.ContactMethods.OwnedBy(userId).NextPositionAsync() };
        Apply(contact, request);
        db.ContactMethods.Add(contact);
        await db.SaveChangesAsync();
        return ToDto(contact);
    }

    public async Task<ContactMethodDto> UpdateAsync(int id, SaveContactMethodRequest request)
    {
        var contact = await FindMineAsync(id);
        Apply(contact, request);
        contact.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        return ToDto(contact);
    }

    public async Task DeleteAsync(int id)
    {
        db.ContactMethods.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderRequest request)
    {
        await db.ContactMethods.OwnedBy(currentUser.RequireUserId()).ApplyOrderAsync(request);
        await db.SaveChangesAsync();
    }

    private async Task<ContactMethod> FindMineAsync(int id) =>
        await db.ContactMethods.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(c => c.Id == id)
        ?? throw new NotFoundException("找不到這個聯絡方式");

    private static void Apply(ContactMethod c, SaveContactMethodRequest r)
    {
        c.Type = r.Type;
        c.Label = r.Label?.Trim() ?? "";
        c.Value = r.Value.Trim();
        c.IsPublic = r.IsPublic;
    }

    private static ContactMethodDto ToDto(ContactMethod c) => new(c.Id, c.Type, c.Label, c.Value, c.IsPublic, c.SortOrder);
}
