using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatutosApi.Services;


namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("usuario")]
    public class UsuarioController : ControllerBase
    {
        private MatutosDbContext _dbcontext;

        public UsuarioController(MatutosDbContext dbcontext)
        {
            _dbcontext = dbcontext ?? throw new ArgumentNullException(nameof(dbcontext));
        }

        [HttpPost("cadastrar")]
        [AllowAnonymous]
        public async Task<IActionResult> CriarUsuario([FromBody] UsuarioCadastro request)
        {
            var emailExiste = await _dbcontext.Usuarios.AnyAsync(u => u.Email == request.Email);
            if (emailExiste)
            {
                return BadRequest(new { Message = "Este e-mail já está em uso." });
            }

            Usuario novoUsuario;

            switch (request.TipoSelecionado)
            {
                case UsuarioTipo.Cliente:
                    novoUsuario = new Cliente();
                    break;
                case UsuarioTipo.Barbeiro:
                    novoUsuario = new Barbeiro();
                    break;
                case UsuarioTipo.Administrador:
                    novoUsuario = new Administrador();
                    break;
                default:
                    return BadRequest(new { Message = "Perfil inválido selecionado." });
            }

            novoUsuario.Nome = request.Nome;
            novoUsuario.Email = request.Email;
            novoUsuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha);
            novoUsuario.Ativo = true;

            _dbcontext.Usuarios.Add(novoUsuario);
            await _dbcontext.SaveChangesAsync();

            var token = TokenService.GenerateToken(novoUsuario);

            return Ok(new
            {
                novoUsuario.Codigo_Usuario,
                novoUsuario.Nome,
                novoUsuario.Email,
                Perfil = request.TipoSelecionado.ToString(),
                Token = token
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UsuarioLogin usuario)
        {
            var login = await _dbcontext.Usuarios.FirstOrDefaultAsync(u => u.Email == usuario.Email);

            if (login == null || !BCrypt.Net.BCrypt.Verify(usuario.Senha, login.Senha))
            {
                return Unauthorized(new { Mensagem = "Login ou senha inválidos!" });
            }

            var token = TokenService.GenerateToken(login);

            return Ok(new
            {
                login.Codigo_Usuario,
                Nome = login.Nome,
                login.Email,
                Token = token
            });
        }

       

    }
}
