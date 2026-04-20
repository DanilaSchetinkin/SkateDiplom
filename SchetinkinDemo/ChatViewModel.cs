using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using SchetinkinDemo.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace SchetinkinDemo.ViewModels
{
    public class MessageViewModel : ReactiveObject
    {
        public string SenderName { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
        public Brush BackgroundColor { get; set; }
        public Brush SenderColor { get; set; }
        public Avalonia.Layout.HorizontalAlignment HorizontalAlignment { get; set; }
    }

    public class ChatViewModel : ReactiveObject
    {
        private readonly IDbContextFactory<SkateshopDbContext> _dbFactory;
        private readonly int _currentUserId;
        private readonly int _conversationId;

        public ObservableCollection<MessageViewModel> Messages { get; } = new ObservableCollection<MessageViewModel>();

        private string _newMessageText = string.Empty;
        public string NewMessageText
        {
            get => _newMessageText;
            set => this.RaiseAndSetIfChanged(ref _newMessageText, value);
        }

        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }

        public ChatViewModel(IDbContextFactory<SkateshopDbContext> dbFactory, int currentUserId, int conversationId)
        {
            _dbFactory = dbFactory;
            _currentUserId = currentUserId;
            _conversationId = conversationId;

            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage);

            SendMessageCommand.ThrownExceptions.Subscribe(ex =>
                System.Diagnostics.Debug.WriteLine($"SendMessage: {ex}"));

            LoadMessages();
        }

        public async void LoadMessages()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var messages = await db.Messages
                    .Where(m => m.ConversationId == _conversationId)
                    .Include(m => m.Sender)
                    .OrderBy(m => m.SentAt)
                    .AsNoTracking()
                    .ToListAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Messages.Clear();
                    foreach (var msg in messages)
                        Messages.Add(MapToMessageViewModel(msg));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadMessages: {ex}");
            }
        }

        private MessageViewModel MapToMessageViewModel(Message msg)
        {
            var isCurrentUser = msg.SenderId == _currentUserId;
            var senderName = msg.Sender is null
                ? "Пользователь"
                : $"{msg.Sender.FirstName} {msg.Sender.LastName}".Trim();
            if (string.IsNullOrEmpty(senderName))
                senderName = "Пользователь";

            return new MessageViewModel
            {
                SenderName = senderName,
                MessageText = msg.MessageText,
                SentAt = msg.SentAt,
                HorizontalAlignment = isCurrentUser ? Avalonia.Layout.HorizontalAlignment.Right : Avalonia.Layout.HorizontalAlignment.Left,
                BackgroundColor = isCurrentUser ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightBlue),
                SenderColor = isCurrentUser ? new SolidColorBrush(Colors.DarkGreen) : new SolidColorBrush(Colors.DarkBlue)
            };
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(NewMessageText))
                return;

            var text = NewMessageText.Trim();

            // Сбрасываем поле ввода на UI-потоке
            await Dispatcher.UIThread.InvokeAsync(() => NewMessageText = string.Empty);

            try
            {
                using var db = _dbFactory.CreateDbContext();
                var newMessage = new Message
                {
                    ConversationId = _conversationId,
                    SenderId = _currentUserId,
                    MessageText = text,
                    SentAt = DateTime.UtcNow
                };

                db.Messages.Add(newMessage);
                await db.SaveChangesAsync();
                await db.Entry(newMessage).Reference(m => m.Sender).LoadAsync();

                var vm = MapToMessageViewModel(newMessage);

                // Добавляем сообщение в коллекцию на UI-потоке
                await Dispatcher.UIThread.InvokeAsync(() => Messages.Add(vm));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendMessage: {ex}");
                // Возвращаем текст обратно на UI-потоке при ошибке
                await Dispatcher.UIThread.InvokeAsync(() => NewMessageText = text);
            }
        }
    }
}
