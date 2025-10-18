using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgoraHora.Api.Data;
using AgoraHora.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    public UsuariosController(AppDbContext db, IConfiguration cfg) { _db = db; _cfg = cfg; }

    // DTOs
    public record RegisterOwnerDto(string EstabelecimentoNome, string Email, string Nome, string Senha);
    public record RegisterUserDto(int EstabelecimentoId, string Email, string Nome, string Senha, string Papel = "staff");
    public record LoginDto(string Email, string Senha);

    // 1) Cria Estabelecimento + Usuário dono
    [HttpPost("register-owner")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return BadRequest("Email e senha obrigatórios.");

        if (await _db.TB_USUARIO.AnyAsync(u => u.Email == dto.Email))
            return Conflict("E-mail já cadastrado.");

        using var tx = await _db.Database.BeginTransactionAsync();

        var estab = new Estabelecimento { Nome = dto.EstabelecimentoNome, Ativo = true };
        _db.Estabelecimentos.Add(estab);
        await _db.SaveChangesAsync();

        var user = new Usuario
        {
            Email = dto.Email.Trim(),
            Nome = dto.Nome?.Trim() ?? dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Ativo = true
        };
        _db.TB_USUARIO.Add(user);
        await _db.SaveChangesAsync();

        _db.TB_USUARIO_ESTABELECIMENTO.Add(new UsuarioEstabelecimento
        {
            UsuarioId = user.Id,
            EstabelecimentoId = estab.Id,
            Papel = "owner"
        });
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
        return CreatedAtAction(nameof(GetMeEstabelecimentos), new { id = user.Id }, new { estabId = estab.Id, userId = user.Id });
    }

    // 2) Adiciona usuário a um Estabelecimento existente
    [HttpPost("register")]
    [Authorize] // opcional: somente donos/adm podem convidar
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto)
    {
        if (!await _db.Estabelecimentos.AnyAsync(e => e.Id == dto.EstabelecimentoId))
            return NotFound("Estabelecimento não encontrado.");

        if (await _db.TB_USUARIO.AnyAsync(u => u.Email == dto.Email))
            return Conflict("E-mail já cadastrado.");

        using var tx = await _db.Database.BeginTransactionAsync();

        var user = new Usuario
        {
            Email = dto.Email.Trim(),
            Nome = dto.Nome?.Trim() ?? dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Ativo = true
        };
        _db.TB_USUARIO.Add(user);
        await _db.SaveChangesAsync();

        _db.TB_USUARIO_ESTABELECIMENTO.Add(new UsuarioEstabelecimento
        {
            UsuarioId = user.Id,
            EstabelecimentoId = dto.EstabelecimentoId,
            Papel = dto.Papel
        });
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
        return Created($"/api/usuarios/{user.Id}", new { userId = user.Id });
    }

    // 3) Login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var u = await _db.TB_USUARIO
            .Include(x => x.Estabelecimentos)
            .FirstOrDefaultAsync(x => x.Email == dto.Email && x.Ativo);
        if (u is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, u.SenhaHash))
            return Unauthorized(new { message = "Usuário ou senha inválidos." });

        var estabs = string.Join(",", u.Estabelecimentos.Select(e => e.EstabelecimentoId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new Claim("estabs", estabs)
        };
        var jwt = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(jwt) });
    }


// 4) Meus estabelecimentos
[HttpGet("me/estabelecimentos")]
    [Authorize]
    public async Task<IActionResult> GetMeEstabelecimentos()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId)) return Unauthorized();

        var estabs = await _db.TB_USUARIO_ESTABELECIMENTO
            .Where(x => x.UsuarioId == userId)
            .Select(x => new { x.EstabelecimentoId, x.Papel })
            .ToListAsync();

        return Ok(estabs);
    }
}
