using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Mappings;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Services;

// ===== Portfolio Service =====
public interface IPortfolioService : ICrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>
{
    Task<List<PortfolioResponse>> GetByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId);
    Task<List<PortfolioResponse>> GetFeaturedAsync(int userId);
}

public class PortfolioService : CrudService<Portfolio, CreatePortfolioDto, UpdatePortfolioDto, PortfolioResponse>, IPortfolioService
{
    public PortfolioService(IRepository<Portfolio> repo) : base(repo) { }
    protected override Portfolio MapToEntity(CreatePortfolioDto dto) => dto.ToEntity();
    protected override PortfolioResponse MapToResponse(Portfolio entity) => entity.ToResponse();
    protected override void ApplyUpdate(Portfolio entity, UpdatePortfolioDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioResponse>> GetByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetPublicByUserIdAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }

    public async Task<List<PortfolioResponse>> GetFeaturedAsync(int userId)
    {
        var items = await Repository.FindAsync(p => p.UserId == userId && p.IsFeatured && p.IsPublic);
        return items.OrderBy(p => p.SortOrder).Select(MapToResponse).ToList();
    }
}

// ===== PortfolioAttachment Service =====
public interface IPortfolioAttachmentService : ICrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>
{
    Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId);
}

public class PortfolioAttachmentService : CrudService<PortfolioAttachment, CreatePortfolioAttachmentDto, UpdatePortfolioAttachmentDto, PortfolioAttachmentResponse>, IPortfolioAttachmentService
{
    public PortfolioAttachmentService(IRepository<PortfolioAttachment> repo) : base(repo) { }
    protected override PortfolioAttachment MapToEntity(CreatePortfolioAttachmentDto dto) => dto.ToEntity();
    protected override PortfolioAttachmentResponse MapToResponse(PortfolioAttachment entity) => entity.ToResponse();
    protected override void ApplyUpdate(PortfolioAttachment entity, UpdatePortfolioAttachmentDto dto) => entity.ApplyUpdate(dto);

    public async Task<List<PortfolioAttachmentResponse>> GetByPortfolioIdAsync(int portfolioId)
    {
        var items = await Repository.FindAsync(a => a.PortfolioId == portfolioId);
        return items.OrderBy(a => a.SortOrder).Select(MapToResponse).ToList();
    }
}
