using Azure;
using MatutosDomain;
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
            string baseURL = "https://localhost:7110/"; ;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseURL)

            };
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

        public async Task<bool> UsuarioCadastrar(UsuarioCadastro usuario)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("usuario/cadastrar", usuario);

                if (response.IsSuccessStatusCode)
                {
                    var dadosAutenticacao = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    if (dadosAutenticacao != null && !string.IsNullOrEmpty(dadosAutenticacao.Token))
                    {
                        await SecureStorage.Default.SetAsync("jwt_token", dadosAutenticacao.Token);
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Cadastro realizado, mas o Token não foi recebido.");
                        return false; 
                    }
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao cadastrar pessoa. Status: {response.StatusCode}, Erro: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar pessoa: {ex.Message}");
                return false;
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
