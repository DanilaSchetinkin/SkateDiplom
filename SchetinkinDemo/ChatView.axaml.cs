using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchetinkinDemo.Models;
using SchetinkinDemo.ViewModels;

namespace SchetinkinDemo
{
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
        }

        public ChatView(int currentUserId, int conversationId)
        {
            InitializeComponent();
            var factory = App.ServiceProvider!.GetRequiredService<IDbContextFactory<SkateshopDbContext>>();
            DataContext = new ChatViewModel(factory, currentUserId, conversationId);
        }
    }
}