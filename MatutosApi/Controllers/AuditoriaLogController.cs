using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Org.BouncyCastle.Asn1.Crmf;
using System.Runtime.CompilerServices;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("auditoria-log")]
    public class AuditoriaLogController : ControllerBase
    {   
        private MatutosDbContext _dbContext;

        public AuditoriaLogController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet("consultar-detalhes/{id}")]
        [Authorize]
        public async Task<IActionResult> LogConsultarDetalhes([FromRoute] int id) // <-- Alterado aqui
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { Mensagem = "Dados inválidos para realizar a consulta" });
                }

                var logDetalhes = await (
                    from log in _dbContext.AuditoriaLogs
                    join usuario in _dbContext.Usuarios on
                        log.Usuario_Acao equals usuario.Codigo_Usuario
                    where (log.Codigo_Log == id)
                    select new AuditoriaLogDetalhesDTO
                    {
                        Codigo_Log = log.Codigo_Log,
                        Usuario_Acao = log.Usuario_Acao,
                        Entidade_Afetada = log.Entidade_Afetada,
                        Registro_ID = log.Registro_ID,
                        Data_Hora_Acao = log.Data_Hora_Acao,
                        Tipo_Acao = log.Tipo_Acao,
                        Valores_Antigos = log.Valores_Antigos,
                        Valores_Novos = log.Valores_Novos,
                        Nome = usuario.Nome
                    }).FirstOrDefaultAsync();

                return Ok(logDetalhes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro interno ao consultar os logs: {ex.Message}" });
            }
        }


        [HttpPost("consulta-lista")]
        [Authorize]
        public async Task<IActionResult> LogConsultarLista([FromBody] FiltroLogDTO filtro)
        {
            try
            {
                // Agora podemos validar corretamente porque são DateTime?
                if (filtro.Inicio == null || filtro.Fim == null)
                {
                    return BadRequest(new { Mensagem = "As datas de início e fim são obrigatórias para a consulta de log." });
                }

                var listaLog = await (
                    from log in _dbContext.AuditoriaLogs

                        // LEFT JOIN: Garante que logs do sistema (sem usuário associado) também apareçam
                    join u in _dbContext.Usuarios on log.Usuario_Acao equals u.Codigo_Usuario into usuarioGroup
                    from usuario in usuarioGroup.DefaultIfEmpty()

                        // Filtros com os parênteses corrigidos!
                    where (log.Data_Hora_Acao >= filtro.Inicio && log.Data_Hora_Acao <= filtro.Fim) &&
                          (string.IsNullOrEmpty(filtro.Entidade) || log.Entidade_Afetada.Contains(filtro.Entidade)) &&
                          (string.IsNullOrEmpty(filtro.UsuarioLog) || (usuario != null && usuario.Nome.Contains(filtro.UsuarioLog))) &&
                          (filtro.Acao == null || log.Tipo_Acao == filtro.Acao)

                    select new
                    {
                        Codigo_Log = log.Codigo_Log,
                        Data_Hora_Acao = log.Data_Hora_Acao,
                        Entidade_Afetada = log.Entidade_Afetada,
                        Registro_ID = log.Registro_ID,
                        Tipo_Acao = log.Tipo_Acao.ToString(), // Retorna a string do Enum (ex: "INSERT")
                        Valores_Antigos = log.Valores_Antigos,
                        Valores_Novos = log.Valores_Novos,
                        Usuario_Acao = log.Usuario_Acao,
                        Nome_Usuario = usuario != null ? usuario.Nome : "Sistema" // Bônus: Tratamento elegante para logs automáticos
                    }).ToListAsync();

                return Ok(listaLog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro interno ao consultar os logs: {ex.Message}" });
            }
        }

    }
}
