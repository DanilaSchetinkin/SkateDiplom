using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo;

/// <summary>
/// Общая логика чатов поддержки: клиент → админ и сотрудник → техподдержка (админы).
/// </summary>
public static class SupportChatHelper
{
    /// <summary>Клиентский чат с поддержкой (магазин).</summary>
    public const string CustomerSupportConversationType = "user_admin";

    /// <summary>Внутренний чат сотрудника с техподдержкой.</summary>
    public const string StaffSupportConversationType = "staff_support";

    public static async Task<int> GetPrimaryAdminUserIdAsync(SkateshopDbContext db) =>
        await db.Users
            .AsNoTracking()
            .Where(u => u.RoleId != null && u.Role!.Name.ToLower() == "admin")
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Находит или создаёт беседу поддержки для пользователя (одна беседа на пользователя и тип).
    /// </summary>
    public static async Task<int> GetOrCreateSupportConversationAsync(
        SkateshopDbContext db,
        int userId,
        string conversationType,
        string titleWhenCreated)
    {
        var adminId = await GetPrimaryAdminUserIdAsync(db);

        var existing = await db.Conversations
            .Include(c => c.ConversationParticipants)
            .FirstOrDefaultAsync(c =>
                c.Type == conversationType &&
                c.ConversationParticipants.Any(p => p.UserId == userId));

        if (existing != null)
        {
            if (adminId != 0 && adminId != userId &&
                existing.ConversationParticipants.All(p => p.UserId != adminId))
            {
                db.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationId = existing.Id,
                    UserId = adminId
                });
                await db.SaveChangesAsync();
            }

            return existing.Id;
        }

        var conversation = new Conversation
        {
            Type = conversationType,
            Title = titleWhenCreated
        };
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync();

        db.ConversationParticipants.Add(new ConversationParticipant
        {
            ConversationId = conversation.Id,
            UserId = userId
        });

        if (adminId != 0 && adminId != userId)
        {
            db.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = adminId
            });
        }

        await db.SaveChangesAsync();
        return conversation.Id;
    }

    public static Task<int> GetOrCreateCustomerSupportAsync(SkateshopDbContext db, int userId, string userFio) =>
        GetOrCreateSupportConversationAsync(
            db,
            userId,
            CustomerSupportConversationType,
            $"Поддержка: клиент {userFio}");

    public static Task<int> GetOrCreateStaffSupportAsync(SkateshopDbContext db, int userId, string userFio) =>
        GetOrCreateSupportConversationAsync(
            db,
            userId,
            StaffSupportConversationType,
            $"Техподдержка: сотрудник {userFio}");
}
