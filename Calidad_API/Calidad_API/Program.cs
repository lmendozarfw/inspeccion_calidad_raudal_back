using Calidad_API.Data;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Calidad_API.Services;
using Calidad_API.Services.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Inyección de Dependencias
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<Calidad_API.Models.Usuario>, PasswordHasher<Calidad_API.Models.Usuario>>();
builder.Services.AddScoped<IAreaService, OperacionService>();
builder.Services.AddScoped<IDefectoService, DefectoService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IInspeccionService, InspeccionService>();
builder.Services.AddScoped<ICriticidadService, CriticidadService>();
builder.Services.AddScoped<IValeService, ValeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ValePdfService>();
builder.Services.AddScoped<IUnidadNegocioService, UnidadNegocioService>();
builder.Services.AddScoped<IDepartamentoService, DepartamentoService>();
builder.Services.AddScoped<ITipoInspeccionService, TipoInspeccionService>();
builder.Services.AddScoped<IPiezaService, PiezaService>();
builder.Services.AddScoped<IModeloService, ModeloService>();

// Configuración JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddOpenApi(options =>
{
    // Para que Scalar muestre el candado de Bearer
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //// Solo para generar el hash una vez
    //var hasher = new PasswordHasher<Usuario>();
    //var hash = hasher.HashPassword(new Usuario(), "Password123*");
    //Console.WriteLine(hash);
    app.MapOpenApi();                 // expone /openapi/v1.json
    app.MapScalarApiReference();      // expone /scalar (UI interactiva)
                                      // Solo para generar el hash una vez

}

app.UseCors("Angular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();