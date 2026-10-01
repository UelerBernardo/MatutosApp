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
            //Usuario encontrado
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
                Mensagem = "Telefone cadastrado e vinculado com sucesso!",
                novoTelefone.Codigo_Telefone,
                novoTelefone.Numero_Telefone,
                novoTelefone.DDD
            });
        }

        [HttpGet("consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarTelefone()
        {
            try
            {
                var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                   ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuario) || !int.TryParse(usuario, out int codigoCliente))
                {
                    return Unauthorized(new { Mensagem = "Usuário não autenticado ou token inválido." });
                }

                var listaTelefone = await _dbcontext.UsuarioTelefones
                    .Where(ut => ut.Codigo_Usuario == codigoCliente)
                   .Select(ut => new
                   {
                       Codigo_Usuario = ut.Codigo_Usuario,
                       Codigo_Telefone = ut.Codigo_Telefone,

                       Usuario = new
                       {
                           Nome = ut.Usuario.Nome
                       },
                       Telefone = new
                       {
                           Codigo_Telefone = ut.Codigo_Telefone,
                           DDD = ut.Telefone.DDD,
                           Numero_Telefone = ut.Telefone.Numero_Telefone,
                           Principal = ut.Telefone.Principal
                       }
                   }).ToListAsync();

                if (!listaTelefone.Any())
                {
                    return NotFound(new { Mensagem = "Você não possui telefone cadastrado." });
                }

                return Ok(listaTelefone);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar telefones: {ex.Message}" });
            }
        }

        [HttpDelete("excluir/{codigoTelefone}")]
        [Authorize]
        public async Task<IActionResult> ExcluirTelefone(int codigoTelefone)
        {
            try
            {
                var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuario) || !int.TryParse(usuario, out int codigoCliente))
                {
                    return Unauthorized(new { Mensagem = "Usuário não autenticado ou token inválido." });
                }

                // 1. Traz as entidades para a memória
                var telefoneUsuarioExcluir = await _dbcontext.UsuarioTelefones
                    .FirstOrDefaultAsync(a => a.Codigo_Telefone == codigoTelefone && a.Codigo_Usuario == codigoCliente);

                if (telefoneUsuarioExcluir == null)
                {
                    return BadRequest(new { Mensagem = "Vínculo de telefone não encontrado." });
                }

                var telefoneExcluir = await _dbcontext.Telefones
                    .FirstOrDefaultAsync(a => a.Codigo_Telefone == codigoTelefone);

                // 2. Remove as entidades do contexto
                _dbcontext.UsuarioTelefones.Remove(telefoneUsuarioExcluir);

                if (telefoneExcluir != null)
                {
                    _dbcontext.Telefones.Remove(telefoneExcluir);
                }

                // 3. Efetiva no banco e gera os logs de DELETE automaticamente
                await _dbcontext.SaveChangesAsync();

                return Ok(new { Mensagem = "Telefone excluído com sucesso." });
            }
            catch (Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { Mensagem = $"Crash na API: {erroReal}" });
            }
        }

        [HttpPut("alterar/{codigoTelefone}")]
        [Authorize]
        public async Task<IActionResult> AlterarTelefone(int codigoTelefone, [FromBody] Telefone telefone)
        {
            try
            {
                if (codigoTelefone != telefone.Codigo_Telefone)
                {
                    return BadRequest(new { Mensagem = "O código do telefone na URL não corresponde ao objeto enviado." });
                }

                var telefoneExiste = await _dbcontext.Telefones
                    .AnyAsync(t => t.DDD == telefone.DDD && t.Numero_Telefone == telefone.Numero_Telefone && t.Codigo_Telefone != codigoTelefone);

                if (telefoneExiste)
                {
                    return BadRequest(new { Mensagem = "Este telefone já está cadastrado no sistema." });
                }

                // 1. Carrega o telefone para a memória
                var telefoneBanco = await _dbcontext.Telefones
                    .FirstOrDefaultAsync(a => a.Codigo_Telefone == codigoTelefone);

                if (telefoneBanco == null)
                {
                    return NotFound(new { Mensagem = "Telefone não encontrado para alteração." });
                }

                // 2. Altera as propriedades (ChangeTracker anota as mudanças)
                telefoneBanco.DDD = telefone.DDD;
                telefoneBanco.Numero_Telefone = telefone.Numero_Telefone;
                telefoneBanco.Principal = telefone.Principal;

                // 3. Salva gerando o log de UPDATE
                await _dbcontext.SaveChangesAsync();

                return Ok(new { Mensagem = "Telefone alterado com sucesso!" });
            }
            catch (Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { Mensagem = $"Crash na API: {erroReal}" });
            }
        }
    }
}
