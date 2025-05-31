using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartSupport;
using SmartSupport.Repositories;
using SmartSupport.Repositories.Interfaces;
using SmartSupport.Services;
using SmartSupport.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// DB context
builder.Services.AddDbContext<SmartSupportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with API endpoints
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<SmartSupportDbContext>();

// Services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddHttpClient<IAiService, AiService>();

// Repositories
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "SmartSupport API", Version = "v1" });

    // Додаємо схему авторизації
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введи JWT token, наприклад: Bearer {your token}"
    });

    // Вказуємо, що всі запити можуть використовувати Bearer авторизацію
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseHttpsRedirection();
app.UseAuthentication(); // Required for Identity
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<IdentityUser>(); // Map Identity endpoints

app.Run();
