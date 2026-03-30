using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace SchetinkinDemo
{
    public class MessageBox : Window
    {
        public MessageBox(string message)
        {
            Title = "Сообщение";
            Width = 300;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { RowDefinitions = new RowDefinitions("*,Auto") };
            var textBlock = new TextBlock
            {
                Text = message,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            okButton.Click += (s, e) => Close();
            grid.Children.Add(textBlock);
            grid.Children.Add(okButton);
            Content = grid;
        }
    }
}