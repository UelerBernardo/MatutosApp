using Azure;
using MatutosDomain;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Plugin.Firebase.CloudMessaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class UsuarioService
    {
        private readonly HttpClient? _httpClient;

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 👉 CORREÇÃO 1: A assinatura agora retorna uma List<Usuario>
        public async Task<(bool Sucesso, string Mensagem, List<Usuario>? Dados)> ConsultarListaUsuario(string token, UsuarioTipo usuarioTipo, string? nome, bool? ativo)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 👉 CORREÇÃO 2: Montando os parâmetros na URL (Query String)
                var queryParams = new List<string> { $"usuarioTipo={(int)usuarioTipo}" };

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    queryParams.Add($"nome={nome}");
                }

                if (ativo.HasValue)
                {
                    queryParams.Add($"ativo={ativo.Value}");
                }

                // Junta tudo com '&'. Exemplo: usuario/consultar-lista?usuarioTipo=1&nome=teste&ativo=True
                var url = $"usuario/consultar-lista?{string.Join("&", queryParams)}";

                // Chama o GetAsync apenas com a URL montada
                var resultado = await _httpClient.GetAsync(url);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resultado.IsSuccessStatusCode)
                {
                    var dados = await resultado.Content.ReadFromJsonAsync<List<Usuario>>(options);
                    return (true, string.Empty, dados);
                }
                else
                {
                    var mensagemErro = await resultado.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    // 👉 CORREÇÃO 3: Retornando false e pegando apenas a propriedade Mensagem do erro
                    return (false, mensagemErro?.Mensagem ?? "Erro desconhecido ao consultar.", null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar detalhes: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão com a internet.", null);
            }
        }

        public async Task<Usuario> ConsultarPefil(string token)
        {
            var tokenLimpo = token.Replace("Bearer ", "").Trim();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenLimpo);

            return await _httpClient.GetFromJsonAsync<Usuario>("usuario/consultar");
        }

        public async Task<(bool Sucesso, string Mensagem)> AlterarSenha(string token, string senhaAtualDigitada, string novaSenhaDigitada)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenLimpo);

                var pacote = new
                {
                    SenhaAntiga = senhaAtualDigitada,
                    SenhaNova = novaSenhaDigitada
                };

                var resposta = await _httpClient.PostAsJsonAsync("usuario/alterar/senha", pacote);

                if (resposta.IsSuccessStatusCode)
                {
                    return (true, "Senha alterada com sucesso!");
                }
                else
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    System.Diagnostics.Debug.WriteLine($"Falha: {mensagemApi}");

                    return (false, mensagemApi);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao alterar senha: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão com a internet.");
            }
        }

        public async Task<(bool Sucesso, string Mensagem, string CaminhoImagem)> CadastrarImagemUsuario(FileResult arquivoLocal, string token)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();

                using var streamDaFoto = await arquivoLocal.OpenReadAsync();
                using var conteudoDoArquivo = new StreamContent(streamDaFoto);

                // 👉 BÔNUS: Avisa explicitamente o servidor que isso é um arquivo de imagem (ajuda a evitar bloqueios)
                conteudoDoArquivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                using var formulario = new MultipartFormDataContent();
                formulario.Add(conteudoDoArquivo, "arquivo", arquivoLocal.FileName);

                // 👉 A MÁGICA: Em vez de PostAsync direto, criamos a requisição na mão
                using var request = new HttpRequestMessage(HttpMethod.Post, "usuario/cadastrar/imagem");

                // Colamos o token ESPECIFICAMENTE nesta requisição, blindando contra redirecionamentos
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                // Colocamos o formulário dentro da requisição
                request.Content = formulario;

                // Enviamos o "envelope" completo
                var resposta = await _httpClient.SendAsync(request);

                if (resposta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = await resposta.Content.ReadFromJsonAsync<ApiRespostaImagem>(options);

                    return (true, "Foto atualizada com sucesso!", dados?.Caminho);
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    string conteudoString = await resposta.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(conteudoString))
                    {
                        return (false, $"Acesso negado (Status: {resposta.StatusCode}). O Token foi recusado.", string.Empty);
                    }

                    try
                    {
                        var erroResposta = System.Text.Json.JsonSerializer.Deserialize<ApiErroResposta>(conteudoString, options);
                        return (false, erroResposta?.Mensagem ?? "Erro desconhecido.", string.Empty);
                    }
                    catch
                    {
                        return (false, "Falha de comunicação com a API.", string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao enviar imagem: {ex.Message}");
                return (false, "Falha de comunicação com o servidor ao enviar a foto.", string.Empty);
            }
        }

        public async Task<(bool Sucesso, Usuario Dados)> UsuarioCadastrar(UsuarioCadastro usuario)
        {
            try
            {
                var resposta = await _httpClient.PostAsJsonAsync("usuario/cadastrar", usuario);

                // A configuração para ler o JSON sem frescura com maiúsculas/minúsculas
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    // 👉 CORREÇÃO 1: Passamos o 'options' para dentro do método de leitura
                    var dados = await resposta.Content.ReadFromJsonAsync<AuthResponse>(options);

                    if (dados != null && !string.IsNullOrEmpty(dados.Token))
                    {
                        await SecureStorage.Default.SetAsync("jwt_token", dados.Token);

                        // 👉 CORREÇÃO 2: Montamos a entidade Usuario pura pegando as peças soltas que vieram do AuthResponse
                        var usuarioSalvo = new Usuario
                        {
                            Codigo_Usuario = dados.Usuario.Codigo_Usuario,
                            Nome = dados.Usuario.Nome,
                            Email = dados.Usuario.Email,
                            TipoSelecionado = dados.Usuario.TipoSelecionado
                        };

                        return (true, usuarioSalvo);
                    }
                    else
                    {
                        Debug.WriteLine("Cadastro realizado, mas o Token não foi recebido.");
                        return (false, null);
                    }
                }
                else
                {
                    var errorMessage = await resposta.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao cadastrar pessoa. Status: {resposta.StatusCode}, Erro: {errorMessage}");
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar pessoa: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, Usuario Dados, string TokenFCM)> UsuarioLogin(UsuarioLogin login)
        {
            try
            {
                var resposta = await _httpClient.PostAsJsonAsync("usuario/login", login);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dadosLogin = await resposta.Content.ReadFromJsonAsync<AuthResponse>(options);

                    if (dadosLogin != null && !string.IsNullOrEmpty(dadosLogin.Token))
                    {
                        await SecureStorage.Default.SetAsync("jwt_token", dadosLogin.Token);

                        return (true, string.Empty, dadosLogin.Usuario, dadosLogin.Token);
                    }

                    return (false, "Falha ao ler o token de acesso.", null, null);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);
                    string mensagemDaApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar requisição.";
                    return (false, mensagemDaApi, null, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro de conexão no login: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", null, null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> UsuarioAlterar(Usuario usuario, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.PutAsJsonAsync("usuario/alterar/", usuario);

                if(resposta.IsSuccessStatusCode)
                {
                    return (true, "Usuario alterado com sucesso!");
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();

                    string mensagemDaApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar requisição.";

                    Debug.WriteLine($"Falha: {mensagemDaApi}");
                    return (false, mensagemDaApi);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> SolicitarCodigo(string emailUsuario)
        {
            try
            {
                var resposta = await _httpClient.PostAsJsonAsync("usuario/solicitar-codigo", emailUsuario);

                string conteudoResposta = await resposta.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var mensagemApi = JsonSerializer.Deserialize<ApiResposta>(conteudoResposta, options);

                if (resposta.IsSuccessStatusCode)
                {
                    string mensagemResposta = mensagemApi?.Mensagem ?? "Código enviado com sucesso!";
                    return (true, mensagemResposta);
                }
                else
                {
                    string erroDaApi = mensagemApi?.Mensagem ?? "Erro desconhecido de comunicação.";
                    return(false, erroDaApi);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> RedefinirSenha(string emailUsuario, string senhaNova, string codigoConfirmacao)
        {
            try
            {
                var payload = new
                {
                    Email = emailUsuario,
                    Codigo = codigoConfirmacao,
                    NovaSenha = senhaNova
                };


                var resposta = await _httpClient.PostAsJsonAsync("usuario/redefinir-senha", payload);

                string conteudo = await resposta.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var mensagemApi = JsonSerializer.Deserialize<ApiResposta>(conteudo, options);

                if(resposta.IsSuccessStatusCode)
                {
                    string mensagemResposta = mensagemApi.Mensagem;
                    return (true, mensagemResposta);
                }
                else
                {
                    string erroDaApi = mensagemApi?.Mensagem ?? "Erro desconhecido de comunicação.";
                    return (false, erroDaApi);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }

        public async Task RegistrarTokenFCMAsync(string jwtToken)
        {
            // 👉 A trava mágica: O código abaixo SÓ existe se for Android
#if ANDROID
            try
            {
                // 1. Pede a permissão na tela do celular
                await Plugin.Firebase.CloudMessaging.CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

                // 2. Captura o Token Físico
                var fcmToken = await Plugin.Firebase.CloudMessaging.CrossFirebaseCloudMessaging.Current.GetTokenAsync();

                if (!string.IsNullOrEmpty(fcmToken))
                {
                    // 3. Prepara o envio para a API
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken.Replace("Bearer ", "").Trim());

                    var dto = new { Token = fcmToken };

                    var resposta = await _httpClient.PutAsJsonAsync("usuario/atualizar-token-fcm", dto);

                    if (resposta.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Sucesso: Token do aparelho salvo no banco de dados SMR!");
                    }
                    else
                    {
                        Console.WriteLine("Erro ao salvar token na API.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FCM_DIAG] Erro crítico: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FCM_DIAG_STACK] {ex.StackTrace}");

                // 2. Cospe o erro diretamente no ecrã do emulador para veres na hora!
                if (Application.Current?.MainPage != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Erro de Inicialização FCM",
                            $"Detalhes do erro:\n{ex.Message}\n\nVerifique se o emulador tem a Google Play Store ativa.",
                            "Entendido");
                    });
                }
            }
#else
            // Se rodar no Windows, ele cai aqui, finge que nada aconteceu e segue a vida!
            Console.WriteLine("Registro de Token ignorado: Firebase Push não é suportado no Windows.");
            await Task.CompletedTask;
#endif
        }
    }
}
