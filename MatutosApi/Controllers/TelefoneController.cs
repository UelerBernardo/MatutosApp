using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("telefone")]
    public class TelefoneController : ControllerBase
    {
        private MatutosDbContext _dbcontext;

        public TelefoneController(MatutosDbContext dbcontext)
        {
            _dbcontext = dbcontext ?? throw new ArgumentNullException(nameof(dbcontext));
        }
        [HttpPost("cadastrar")]
        [Authorize]
        public async Task<IActionResult> CriarTelefone([FromBody] Telefone telefone)
        {
            UsuarioTelefone usuarioTelefone = new UsuarioTelefone();

            var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(usuario))
            {
                return BadRequest(new { Mensagem = "Usuário não identificado no token." });
            }

            int codigoUsuarioLogado = int.Parse(usuario);

            var telefoneExiste = await _dbcontext.Telefones
                .AnyAsync(t => t.DDD == telefone.DDD && t.Numero_Telefone == telefone.Numero_Telefone);

            if (telefoneExiste)
            {
                return BadRequest(new { Mensagem = "Este telefone já está cadastrado no sistema." });
            }

            if (telefone.Principal)
            {
                //Validação se o usuário já possui um telefone como principal
                var jaPossuiPrincipal = await _dbcontext.UsuarioTelefones
                    .AnyAsync(ut => ut.Codigo_Usuario == codigoUsuarioLogado && ut.Telefone.Principal == true);

                if (jaPossuiPrincipal)
                {
                    return BadRequest(new { Mensagem = "Você já possui um telefone principal cadastrado. Desmarque a opção para adicionar este número." });
                }
            }

            var novoTelefone = new Telefone
            {
                Numero_Telefone = telefone.Numero_Telefone,
                DDD = telefone.DDD,
                Principal = telefone.Principal
            };

            _dbcontext.Telefones.Add(novoTelefone);

            await _dbcontext.SaveChangesAsync();


            var vinculo = new UsuarioTelefone
            {
                Codigo_Usuario = codigoUsuarioLogado,
                Codigo_Telefone = novoTelefone.Codigo_Telefone
            };

            _dbcontext.UsuarioTelefones.Add(vinculo);
            await _dbcontext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Telefone cadastrado e vinculado com sucesso!",
                novoTelefone.Codigo_Telefone,
                novoTelefone.Numero_Telefone,
                novoTelefone.DDD
            });
        }
    }
}
