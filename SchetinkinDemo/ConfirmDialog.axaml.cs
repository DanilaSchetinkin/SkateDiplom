using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SchetinkinDemo;

public partial class ConfirmDialog : Window
{
    
        // Конструктор по умолчанию для дизайнера
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        // НАШ ГЛАВНЫЙ КОНСТРУКТОР
        // Он будет принимать текст вопроса
        public ConfirmDialog(string message)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
        }

        // Когда пользователь нажимает "Да"
        private void YesButton_Click(object? sender, RoutedEventArgs e)
        {
            // Мы закрываем диалог и ВОЗВРАЩАЕМ true
            this.Close(true);
        }

        // Когда пользователь нажимает "Нет"
        private void NoButton_Click(object? sender, RoutedEventArgs e)
        {
            // Мы закрываем диалог и ВОЗВРАЩАЕМ false
            this.Close(false);
        }
    }