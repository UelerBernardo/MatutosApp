using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("servico")]
    public class ServicoController : ControllerBase
    {
        private readonly MatutosDbContext _dbContext;

        public ServicoController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost ("cadastrar")]
        [Authorize]
        public async  Task<IActionResult> CadastrarServico([FromBody] Servico servico)
        {

            try
            {
                if (servico.Imagens != null && servico.Imagens.Count > 3)
                {
                    return BadRequest(new { Message = "Um serviço não pode ser cadastrado com mais de 3 imagens." });
                }

                var servicoNovo = new Servico
                {
                    Descricao = servico.Descricao,
                    Duracao = servico.Duracao,
                    Preco = servico.Preco,
                    Tempo_Servico = servico.Tempo_Servico,
                    Ativo = servico.Ativo,

                    Imagens = servico.Imagens?.Select(img => new Servico_Imagem
                    {
                        Imagem = img.Imagem
                    }).ToList() ?? new List<Servico_Imagem>()

                };  

                _dbContext.Servicos.Add(servicoNovo);

                await _dbContext.SaveChangesAsync();

                return Ok(new { Mensagem = "Serviço e imagens cadastrados com sucesso!" });
            }

            catch(Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { Message = $"Erro interno: {erroReal}" });
            }
        }


        [HttpGet ("consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarServico()
        {
            try
            {

                var servicos = await _dbContext.Servicos
                    .Where(a => a.Ativo == true)
                    .Select(a => new
                    {
                        a.Codigo_Servico,
                        a.Descricao,
                        a.Preco,
                        a.Duracao,
                        a.Tempo_Servico,
                        a.Ativo
                    })
                    .ToListAsync();
                return Ok(servicos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro interno ao buscar barbeiros: {ex.Message}" });
            }
        }

        [HttpPost("cadastrar/imagens/{codigoServico}")]
        [Authorize] 
        public async Task<IActionResult> AdicionarImagem(int codigoServico, [FromBody] AdicionarImagemServicoRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImagemBase64))
                {
                    return BadRequest(new { Mensagem = "Nenhuma imagem foi enviada." });
                }

                var servico = await _dbContext.Servicos
                    .Include(s => s.Imagens)
                    .FirstOrDefaultAsync(s => s.Codigo_Servico == codigoServico);

                if (servico == null)
                {
                    return NotFound(new { Mensagem = "Serviço não encontrado." });
                }

                
                if (servico.Imagens.Count >= 3)
                {
                    return BadRequest(new { Mensagem = "Este serviço já atingiu o limite máximo de 3 imagens." });
                }

                var novaImagem = new Servico_Imagem
                {
                    Codigo_Servico = codigoServico,
                    Imagem = request.ImagemBase64
                };

                _dbContext.Servico_Imagens.Add(novaImagem);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Mensagem = "Imagem cadastrada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao salvar a imagem: {ex.Message}" });
            }
        }


        [HttpGet("consultar/imagens/{codigoServico}")]
        public async Task<IActionResult> ConsultarImagensDoServico(int codigoServico)
        {
            try
            {
                var imagens = await _dbContext.Servico_Imagens
                    .Where(img => img.Codigo_Servico == codigoServico)
                    .Select(img => new
                    {
                        img.Codigo_Imagem,
                        img.Imagem, 
                        img.Codigo_Servico
                    })
                    .ToListAsync();

                if (!imagens.Any())
                {
                    return NotFound(new { Mensagem = "Nenhuma imagem encontrada para este serviço." });
                }

                return Ok(imagens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao buscar as imagens: {ex.Message}" });
            }
        }
    }
}
