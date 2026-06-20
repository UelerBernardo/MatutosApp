using Azure;
using MatutosDomain;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public UsuarioService()
        {
            string baseURL = "https://localhost:7110/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseURL)

            };
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

        public async Task<(bool Sucesso, string Mensagem, Usuario Dados)> UsuarioLogin(UsuarioLogin login)
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

                        return (true, string.Empty, dadosLogin.Usuario);
                    }

                    return (false, "Falha ao ler o token de acesso.", null);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);
                    string mensagemDaApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar requisição.";
                    return (false, mensagemDaApi, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro de conexão no login: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", null);
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
    }
}
