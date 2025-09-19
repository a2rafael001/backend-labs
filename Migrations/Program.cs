using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Npgsql;

namespace Migrations;

public static class Program
{
    public static int Main(string[] args)
    {
        // Определяем окружение (по умолчанию Development)
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Конфиг: база — папка исполняемого файла (в контейнере это /app), JSON + JSON по окружению + ENV
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()                         // <-- ВАЖНО
            .Build();

        // Берём строку подключения: сначала из ENV (переменные вида DbSettings__MigrationConnectionString / __ConnectionString),
        // если нет — из JSON. Если нет вовсе — падаем.
        var migConn =
            config["DbSettings:MigrationConnectionString"]
            ?? config["DbSettings:ConnectionString"];

        if (string.IsNullOrWhiteSpace(migConn))
        {
            Console.Error.WriteLine("No connection string found (DbSettings:MigrationConnectionString / DbSettings:ConnectionString).");
            return 1;
        }

        var services = new ServiceCollection()
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
                rb.AddPostgres()
                  .WithGlobalConnectionString(migConn)
                  .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
            .BuildServiceProvider(false);

        try
        {
            using var scope = services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();

            Console.WriteLine("Migrations applied successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
