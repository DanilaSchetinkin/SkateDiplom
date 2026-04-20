using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchetinkinDemo.Models;
using System;

namespace SchetinkinDemo
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Настройка ServiceProvider
            var services = new ServiceCollection();

            // Здесь должна быть ваша строка подключения, как в appsettings.json
            // Или можно прочитать ее из конфигурации:
            // var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            // var connectionString = configuration.GetConnectionString("DefaultConnection");
            var connectionString = "Host=localhost;Port=5432;Database=skateshop_db;Username=skateuser;Password=1234321"; // ЗАМЕНИТЕ на вашу!

            void ConfigureDb(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder options) =>
                options.UseNpgsql(connectionString);

            services.AddDbContext<SkateshopDbContext>(ConfigureDb);
            // Отдельные экземпляры контекста для чата (корневой DI иначе даёт один контекст — гонки и падения EF).
            services.AddDbContextFactory<SkateshopDbContext>(ConfigureDb);

            ServiceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}