using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Common;
using PersonalManager.Api.Data;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Features.Todos;

public sealed record SaveTodoRequest(
    [Required(ErrorMessage = "請輸入待辦事項"), StringLength(200)] string Title,
    [StringLength(5000)] string? Description,
    TodoPriority Priority,
    TodoStatus Status,
    DateTime? DueDate);

public sealed record TodoDto(
    int Id, string Title, string Description, TodoPriority Priority, TodoStatus Status, DateTime? DueDate,
    DateTime? CompletedAt, DateTime CreatedAt);

public sealed class TodoService(ApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    /// <summary>未完成的排前面，再依到期日（沒有到期日的排最後）。</summary>
    public Task<List<TodoDto>> GetMineAsync(TodoStatus? status)
    {
        var query = db.TodoItems.AsNoTracking().OwnedBy(currentUser.RequireUserId());
        if (status is not null)
            query = query.Where(t => t.Status == status);
        return query
            .OrderBy(t => t.Status == TodoStatus.Completed)
            .ThenBy(t => t.DueDate == null).ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.Id)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TodoDto> CreateAsync(SaveTodoRequest request)
    {
        var todo = new TodoItem { UserId = currentUser.RequireUserId() };
        Apply(todo, request);
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();
        return ToDto(todo);
    }

    public async Task<TodoDto> UpdateAsync(int id, SaveTodoRequest request)
    {
        var todo = await FindMineAsync(id);
        Apply(todo, request);
        todo.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        return ToDto(todo);
    }

    public async Task DeleteAsync(int id)
    {
        db.TodoItems.Remove(await FindMineAsync(id));
        await db.SaveChangesAsync();
    }

    private async Task<TodoItem> FindMineAsync(int id) =>
        await db.TodoItems.OwnedBy(currentUser.RequireUserId()).FirstOrDefaultAsync(t => t.Id == id)
        ?? throw new NotFoundException("找不到這個待辦事項");

    /// <summary>標記完成時記下完成時間；重新打開時清除。</summary>
    private void Apply(TodoItem todo, SaveTodoRequest request)
    {
        todo.Title = request.Title.Trim();
        todo.Description = request.Description ?? "";
        todo.Priority = request.Priority;
        todo.DueDate = request.DueDate?.ToUniversalTime();
        todo.CompletedAt = request.Status == TodoStatus.Completed
            ? todo.CompletedAt ?? clock.GetUtcNow().UtcDateTime
            : null;
        todo.Status = request.Status;
    }

    private static TodoDto ToDto(TodoItem t) =>
        new(t.Id, t.Title, t.Description, t.Priority, t.Status, t.DueDate, t.CompletedAt, t.CreatedAt);
}

[ApiController]
[Authorize]
[Route("api/me/todos")]
public sealed class MyTodosController(TodoService todos) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<List<TodoDto>>> List([FromQuery] TodoStatus? status) =>
        ApiResponse<List<TodoDto>>.Ok(await todos.GetMineAsync(status));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TodoDto>>> Create(SaveTodoRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<TodoDto>.Ok(await todos.CreateAsync(request), "已新增待辦"));

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<TodoDto>> Update(int id, SaveTodoRequest request) =>
        ApiResponse<TodoDto>.Ok(await todos.UpdateAsync(id, request), "已更新待辦");

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        await todos.DeleteAsync(id);
        return ApiResponse.Ok("已刪除待辦");
    }
}
