using AgoraHora.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS (pode manter, mas não é necessário se front vier da mesma origem)
const string Cors = "CorsDev";
builder.Services.AddCors(o => o.AddPolicy(Cors, p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
));

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AgoraHora API", Version = "1.0.0" });
});

var app = builder.Build();

// Dev
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json?v=1", "AgoraHora.Api v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors(Cors);
app.UseStaticFiles();         
app.MapControllers().RequireCors(Cors);

// Health
app.MapGet("/health", () => Results.Ok(new { ok = true }));

// API
app.MapControllers().RequireCors(Cors);

app.Run();
