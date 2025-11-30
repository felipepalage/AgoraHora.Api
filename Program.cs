using System.Text;
using AgoraHora.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== DB
builder.Services.AddDbContext<AppDbContext>(o =>
	o.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));

// ===== Tenant context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();

// ===== Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== CORS
const string Cors = "CorsDev";
builder.Services.AddCors(o => o.AddPolicy(Cors, p =>
	p.AllowAnyOrigin()
	 .AllowAnyHeader()
	 .AllowAnyMethod()
));

// ===== Auth (JWT)
var jwtKey = builder.Configuration["Jwt:Key"]
			 ?? throw new InvalidOperationException("Jwt:Key ausente");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(opt =>
	{
		opt.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = signingKey,
			ClockSkew = TimeSpan.FromMinutes(2)
		};
	});

builder.Services.AddAuthorization();

// ===== Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "AgoraHora API",
		Version = "1.0.0"
	});

	var scheme = new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Bearer {token}"
	};

	c.AddSecurityDefinition("Bearer", scheme);
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{ scheme, Array.Empty<string>() }
	});
});

// ===== Upload: limites razoáveis
builder.Services.Configure<FormOptions>(o =>
{
	o.MultipartBodyLengthLimit = 5_000_000;   // 5 MB
	o.ValueLengthLimit = int.MaxValue;
	o.MemoryBufferThreshold = 1024 * 64;
});

var app = builder.Build();

// ===== Dev tools
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

// Swagger em todos os ambientes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgoraHora API v1");
	c.RoutePrefix = "swagger";
});

// garante wwwroot/uploads/…
var webRoot = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
app.Environment.WebRootPath = webRoot;
var uploadsDir = Path.Combine(webRoot, "uploads");
Directory.CreateDirectory(uploadsDir);

// Static files com cache leve
app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=604800"; // 7 dias
	}
});

app.UseCors(Cors);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireCors(Cors);

// Health
app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.Run();


namespace AgoraHora.Api.Data
{
	public sealed class CurrentTenant : ICurrentTenant
	{
		private readonly IHttpContextAccessor _http;
		public CurrentTenant(IHttpContextAccessor http) => _http = http;

		public int? EstabelecimentoId
		{
			get
			{
				var h = _http.HttpContext?.Request?.Headers["X-Estabelecimento-Id"].FirstOrDefault();
				return int.TryParse(h, out var id) ? id : null;
			}
		}
	}
}
