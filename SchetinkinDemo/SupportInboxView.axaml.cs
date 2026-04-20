using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchetinkinDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo;

public partial class SupportInboxView : UserControl
{
    private string _conversationType = "";
    private Action<int> _onOpenChat = _ => { };
    private Action _onBack = () => { };

    /// <summary>Нужен для компилятора Avalonia (avares).</summary>
    public SupportInboxView()
    {
        InitializeComponent();
    }

    public SupportInboxView(string conversationType, string header, Action<int> onOpenChat, Action onBack)
        : this()
    {
        _conversationType = conversationType;
        _onOpenChat = onOpenChat;
        _onBack = onBack;
        HeaderTextBlock.Text = header;
        Loaded += async (_, _) => await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (string.IsNullOrEmpty(_conversationType))
            return;

        var db = App.ServiceProvider!.GetRequiredService<SkateshopDbContext>();
        var raw = await db.Conversations
            .AsNoTracking()
            .Where(c => c.Type == _conversationType)
            .OrderByDescending(c => c.Messages.Any() ? c.Messages.Max(m => m.SentAt) : c.CreatedAt)
            .Take(100)
            .Select(c => new
            {
                c.Id,
                c.Title,
                LastActivity = c.Messages.Any() ? c.Messages.Max(m => m.SentAt) : c.CreatedAt
            })
            .ToListAsync();

        var rows = raw.Select(r => new InboxRow
        {
            ConversationId = r.Id,
            Title = string.IsNullOrWhiteSpace(r.Title) ? $"Беседа #{r.Id}" : r.Title!,
            LastActivity = r.LastActivity
        }).ToList();

        ConversationsListBox.ItemsSource = rows;
        if (rows.Count > 0)
            ConversationsListBox.SelectedIndex = 0;
    }

    private void OpenChatButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ConversationsListBox.SelectedItem is not InboxRow row)
            return;

        _onOpenChat(row.ConversationId);
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e) => _onBack();

    private sealed class InboxRow
    {
        public int ConversationId { get; init; }
        public string Title { get; init; } = "";
        public DateTime LastActivity { get; init; }

        public override string ToString() =>
            $"{Title}  —  {LastActivity:dd.MM.yyyy HH:mm}";
    }
}
