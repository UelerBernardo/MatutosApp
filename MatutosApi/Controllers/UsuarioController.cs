using MatutosApi.Infraestrutura;
using MatutosDomain;
using MatutosApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatutosApi.Services;
using System.Runtime.CompilerServices;
using static MatutosApi.Controllers.UsuarioController;
using System.Security.Claims;


namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("usuario")]
    public class UsuarioController : ControllerBase
    {
        private MatutosDbContext _dbcontext;
        private readonly IEmailService _emailService;

        public UsuarioController(MatutosDbContext dbcontext, IEmailService emailService)
        {
            _dbcontext = dbcontext ?? throw new ArgumentNullException(nameof(dbcontext));
            _emailService = emailService;
        }

        [HttpPut("alterar")]
        [Authorize]
        public async Task<IActionResult> AlterarUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var usuarioToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuarioToken) || !int.TryParse(usuarioToken, out int codigoUsuario))
                {
                    return Unauthorized(new { Mensagem = "Usuário não autenticado." });
                }

                var usuarioAlteracao = await _dbcontext.Usuarios
                    .Where(a => a.Codigo_Usuario == codigoUsuario)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(u => u.Nome,  usuario.Nome)
                        .SetProperty(u => u.Email, usuario.Email)
                    );

                if(usuarioAlteracao <= 0)
                {
                    return NotFound(new { Mensagem = "Usuário não encontrado para alteração." });
                }

                return Ok(new { Mensagem = "Usuario alterado com sucesso!" });
            }
            catch (Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { Mensagem = $"Crash na API: {erroReal}" });
            }
        }



        [HttpPost("cadastrar/imagem")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CadastrarImagemUsuario(IFormFile arquivo)
        {
            try
            {
                // 1. Pega o ID do usuário logado pelo Token
                var usuarioToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuarioToken) || !int.TryParse(usuarioToken, out int codigoCliente))
                {
                    return Unauthorized(new { Mensagem = "Usuário não autenticado." });
                }

                // 2. Valida se o arquivo realmente foi enviado e se tem tamanho
                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { Mensagem = "Nenhum arquivo de imagem foi enviado." });
                }

                // 3. Valida a extensão (Para evitar que enviem vírus ou PDF no lugar da foto)
                var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
                var extensao = Path.GetExtension(arquivo.FileName).ToLower();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    return BadRequest(new { Mensagem = "Formato inválido. Envie apenas imagens JPG ou PNG." });
                }

                // 4. Cria a pasta "wwwroot/Uploads/Perfil" no servidor, caso não exista
                string pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                if (!Directory.Exists(pastaUploads))
                {
                    Directory.CreateDirectory(pastaUploads);
                }

                // 5. Cria um nome único para a foto (Ex: perfil_17.jpg)
                // Usar o ID do cliente garante que, se ele trocar de foto, a antiga seja substituída!
                string nomeArquivo = $"perfil_{codigoCliente}{extensao}";
                string caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

                // 6. Salva o arquivo fisicamente na pasta do servidor
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }

                // 7. Atualiza o banco de dados com o caminho da foto
                var usuario = await _dbcontext.Usuarios.FindAsync(codigoCliente);
                if (usuario == null)
                {
                    return NotFound(new { Mensagem = "Usuário não encontrado no banco de dados." });
                }

                // 👉 IMPORTANTE: Crie uma coluna tipo string (VARCHAR) chamada 'Foto_Perfil' na sua tabela Usuario
                usuario.Imagem_Usuario = $"/Uploads/{nomeArquivo}";

                // Salva a alteração no banco
                await _dbcontext.SaveChangesAsync();

                return Ok(new
                {
                    Mensagem = "Imagem de perfil atualizada com sucesso!",
                    Caminho = usuario.Imagem_Usuario
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao salvar imagem: {ex.Message}" });
            }
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

                Token = token,
                Usuario = new
                {
                    Codigo_Usuario = novoUsuario.Codigo_Usuario,
                    Nome = novoUsuario.Nome,
                    Email = novoUsuario.Email,
                    TipoSelecionado = request.TipoSelecionado
                }

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

            UsuarioTipo tipoDescoberto = UsuarioTipo.Cliente;

            if (await _dbcontext.Administradores.AnyAsync(a => a.Codigo_Usuario == login.Codigo_Usuario))
            {
                tipoDescoberto = UsuarioTipo.Administrador;
            }
            else if (await _dbcontext.Barbeiros.AnyAsync(b => b.Codigo_Usuario == login.Codigo_Usuario))
            {
                tipoDescoberto = UsuarioTipo.Barbeiro;
            }
            else if (await _dbcontext.Clientes.AnyAsync(c => c.Codigo_Usuario == login.Codigo_Usuario))
            {
                tipoDescoberto = UsuarioTipo.Cliente;
            }

            var token = TokenService.GenerateToken(login);

            return Ok(new
            {
                Token = token,
                Usuario = new
                {
                    Codigo_Usuario = login.Codigo_Usuario,
                    Nome = login.Nome,
                    Email = login.Email,
                    Imagem_Usuario = login.Imagem_Usuario,
                    TipoSelecionado = tipoDescoberto  
                }
            });

        }


        [HttpGet("consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarPerfil()
        {
            try
            {
                var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuario) || !int.TryParse(usuario, out int codigoUsuario))
                {
                    return Unauthorized(new { Mensagem = "Usuário não autenticado ou token inválido." });
                }

                var clienteconsulta = await _dbcontext.Usuarios
                    .Where(a => a.Codigo_Usuario == codigoUsuario)
                    .Select(a => new
                    {
                        a.Codigo_Usuario,
                        a.Nome,
                        a.Email,
                        a.Ativo,
                        a.Imagem_Usuario
                    }).FirstOrDefaultAsync();

                if (clienteconsulta == null)
                {
                    return NotFound(new { Mensagem = "Perfil de cliente não encontrado." });
                }

                return Ok(clienteconsulta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar usuário: {ex.Message}" });
            }
        }

        [HttpPost("alterar/senha")]
        [Authorize]
        public async Task<IActionResult> AlterarSenhaPerfil([FromBody] AlterarSenhaPerfilRequest dados)
        {
            try
            {
                var usuarioClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuarioClaim) || !int.TryParse(usuarioClaim, out int codigoUsuarioLogado))
                {
                    return Unauthorized(new { Mensagem = "Usuário inválido ou não autenticado." });
                }
                var cliente = await _dbcontext.Usuarios
                    .FirstOrDefaultAsync(a => a.Codigo_Usuario == codigoUsuarioLogado);

                if (cliente == null)
                {
                    return NotFound(new { Mensagem = "Usuário não encontrado." });
                }

                bool senhaValida = BCrypt.Net.BCrypt.Verify(dados.SenhaAntiga, cliente.Senha);

                if (!senhaValida)
                {
                    return BadRequest(new { Mensagem = "Senha atual está incorreta." });
                }

                cliente.Senha = BCrypt.Net.BCrypt.HashPassword(dados.SenhaNova);
                await _dbcontext.SaveChangesAsync();

                return Ok(new { Mensagem = "Senha alterada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao alterar senha: {ex.Message}" });
            }
        }

        [HttpPost("solicitar-codigo")]
        [AllowAnonymous]
        public async Task<IActionResult> SolicitarCodigoRecuperacao([FromBody] string emailUsuario)
        {
            try
            {
                var usuario = await _dbcontext.Usuarios.FirstOrDefaultAsync(u => u.Email == emailUsuario);

                if (usuario == null)
                {
                    return Ok(new { Mensagem = "Se o e-mail estiver cadastrado, você receberá um código em instantes." });
                }

                Random random = new Random();
                string codigoRecuperacao = random.Next(100000, 999999).ToString();

                usuario.Codigo_Recuperacao = codigoRecuperacao;
                usuario.Data_Validade_Codigo = DateTime.Now.AddMinutes(15);

                await _dbcontext.SaveChangesAsync();

                await _emailService.EnviarEmailRecuperacaoAsync(usuario.Email, codigoRecuperacao);

                return Ok(new { Mensagem = "Se o e-mail estiver cadastrado, você receberá um código em instantes." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao processar a solicitação: {ex.Message}" });
            }
        }

        [HttpPost("redefinir-senha")]
        [AllowAnonymous]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest request)
        {
            try
            {
                var usuario = await _dbcontext.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

                // Funil 1: O e-mail existe?
                if (usuario == null)
                {
                    return BadRequest(new { Mensagem = "Dados inválidos." }); // Mensagem genérica de propósito para não vazar e-mails
                }

                // Funil 2: O código está correto?
                if (usuario.Codigo_Recuperacao != request.Codigo)
                {
                    return BadRequest(new { Mensagem = "Código de recuperação inválido ou incorreto." });
                }

                // Funil 3: O código ainda está no prazo de 15 minutos?
                if (usuario.Data_Validade_Codigo < DateTime.Now)
                {
                    return BadRequest(new { Mensagem = "O código de recuperação expirou. Por favor, solicite um novo." });
                }

                // Passou em tudo! Vamos criptografar e salvar a nova senha
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

                // 👉 REGRA DE OURO: Limpar o código para que ele não possa ser reusado por um invasor!
                usuario.Codigo_Recuperacao = null;
                usuario.Data_Validade_Codigo = null;

                await _dbcontext.SaveChangesAsync();

                return Ok(new { Mensagem = "Senha redefinida com sucesso! Você já pode fazer o login." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao redefinir a senha: {ex.Message}" });
            }
        }
        [HttpPut("atualizar-token-fcm")]
        [Authorize]
        public async Task<IActionResult> AtualizarTokenFCM([FromBody] TokenFcmDto dto)
        {
            try
            {
                // Pega o ID do usuário logado direto do token JWT (Segurança)
                var usuarioIdClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(usuarioIdClaim))
                    return Unauthorized(new { Mensagem = "Usuário não autorizado." });

                int codigoUsuario = int.Parse(usuarioIdClaim);

                var usuario = await _dbcontext.Usuarios.FindAsync(codigoUsuario); // Ajuste "_dbContext.Usuarios" se necessário
                if (usuario == null)
                    return NotFound(new { Mensagem = "Usuário não encontrado." });

                // Atualiza o token do aparelho e salva no MariaDB
                usuario.TokenFCM = dto.Token;
                await _dbcontext.SaveChangesAsync();

                return Ok(new { Mensagem = "Token FCM atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao atualizar token: {ex.Message}" });
            }
        }


        // FIM DA CLASSE CONTROLLER
        // --------------------------------------------------------

        // 2. A classe de modelo fica aqui fora (ou em um arquivo separado)
        // 3. Renomeamos para evitar qualquer conflito de Schema no Swagger
        public class AlterarSenhaPerfilRequest
        {
            public string? SenhaAntiga { get; set; }
            public string? SenhaNova { get; set; }
        }

        public class RedefinirSenhaRequest
        {
            public string? Email { get; set; }
            public string? Codigo { get; set; }
            public string? NovaSenha { get; set; }
        }
    }
}
