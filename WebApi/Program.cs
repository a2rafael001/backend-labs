using Dapper;
using Migrations;
using Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using WebApi.Dal;
using WebApi.Services;


var builder = WebApplication.CreateBuilder(args);

// DbSettings из конфигурации (appsettings.Development.json)
builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));

// Контроллеры + Swagger (UI)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();


builder.Services.AddScoped<WebApi.Validators.ValidatorFactory>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Dapper: snake_case → C# свойства
DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

// Swagger UI (включаем всегда, чтобы не путаться)
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// (опционально) уберите редирект на https, чтобы не было предупреждения
// app.UseHttpsRedirection();

app.MapControllers();

// ⚠️ запуск миграций при старте API
try
{
    Console.WriteLine("Running DB migrations...");
    var exitCode = Migrations.Program.Main(Array.Empty<string>()); // либо добавьте: using Migrations;
    if (exitCode != 0)
        throw new Exception("Migration runner failed");
    Console.WriteLine("DB migrations completed.");
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;
}

app.Run();
