using Microsoft.EntityFrameworkCore;
using PurchaseOrderAPI.Infrastructure.Data;
using PurchaseOrderAPI.Infrastructure.Repositories;
using PurchaseOrderAPI.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext, aqui dizemos ao EF Core como ele 
// deve conectar ao banco de dados.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlServerOptions => sqlServerOptions.MigrationsAssembly("PurchaseOrderAPI.Infrastructure")));

// Registrar repositorios
builder.Services.AddScoped<PurchaseOrderRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PurchaseOrderItemRepository>();
builder.Services.AddScoped<PurchaseOrderApprovalRepository>();
builder.Services.AddScoped<AuditLogRepository>();

// Registrar services
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<ApprovalService>();
builder.Services.AddScoped<AuditService>();


// Adiciona controllers
builder.Services.AddControllers();

// Swagger para documentar a API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Aplicar migrations automaticamente na inicialização, isso
// cria o banco e as tabelas automaticamente se não existirem

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configura o HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();