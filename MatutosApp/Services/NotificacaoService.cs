using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

namespace MatutosApp.Services
{
    public class NotificacaoService
    {
        private readonly HttpClient? _httpClient;

        public NotificacaoService(HttpClient? httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Sucesso, string Mensagem)> NotificacaoVisualizarTodas(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Dispara a requisição sem enviar dados no corpo
                var resultado = await _httpClient.PutAsync("notificacao/visualizar-todas", null);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var mensagem = await resultado.Content.ReadFromJsonAsync<ApiResposta>(options);

                if (resultado.IsSuccessStatusCode)
                {
                    return (true, mensagem?.Mensagem ?? "Sucesso");
                }
                else
                {
                    return (false, mensagem?.Mensagem ?? "Erro desconhecido");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao alterar as notificações: {ex.Message}");
                return (false, string.Empty);
            }
        }
        public async Task<(bool Sucesso, NotificacaoDetalhes Dados, string Mensagem)> ConsultarNotificacaoDetalhes(string token, int idHistorico)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resposta = await _httpClient.GetAsync($"notificacao/notificacao-detalhe/{idHistorico}");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true}; 

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<NotificacaoDetalhes>(options);

                    return (true, dados, string.Empty);
                }
                else
                {
                    var mensagem = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);
                    return (false, null, mensagem.Mensagem);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar a notificação: {ex.Message}");
                return (false, null, string.Empty);
            }

        }

        public async Task<(bool Sucesso, List<NotificacaoResponseDTO>? Dados)> ConsultarNotificacao(string token, DateTime dataInicial, DateTime dataFinal)
        {
            try
            {

                var query = new List<string>
                {
                    $"dataInicial={dataInicial:yyyy-MM-ddTHH:mm:ss}",
                    $"dataFinal={dataFinal:yyyy-MM-ddTHH:mm:ss}"
                };

                string queryString = string.Join("&", query);

                string urlCompleta = $"notificacao/notificacao-consultar?{queryString}";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resposta = await _httpClient.GetAsync(urlCompleta);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<NotificacaoResponseDTO>>(options);
                    return (true, dados ?? new List<NotificacaoResponseDTO>());
                }
                else
                {
                    var errorMessage = await resposta.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao consultar a notificação. Status {resposta.StatusCode}, Erro: {errorMessage}");
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar a notificação: {ex.Message}");
                return (false, null);
            }
        }

     
        public async Task<(bool Sucesso, List<Configura_Notificacao>? Dados)> ConsultarRegraNotificacao(string token, DateTime inicial, DateTime final, bool? ativo)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var query = new List<string>
                 {
                     $"inicial={inicial:yyyy-MM-ddTHH:mm:ss}",
                     $"final={final:yyyy-MM-ddTHH:mm:ss}"
                 };

                if (ativo.HasValue)
                {
                    query.Add($"ativo={ativo.Value}");
                }

                var urlCompleta = $"notificacao/regra-consultar?{string.Join("&", query)}";

                var resposta = await _httpClient.GetAsync(urlCompleta);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if(resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<Configura_Notificacao>>(options);
                    return (true, dados ?? new List<Configura_Notificacao>());
                }
                else
                {
                    var errorMessage = await resposta.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao consultar regra de notificação. Status {resposta.StatusCode}, Erro: {errorMessage}");
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar regra de notificação: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<(bool Sucesso, List<Tipo_Evento>? Dados)> ConsultarTipoEvento(string token)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var resposta = await _httpClient.GetAsync("notificacao/tipo-evento/consultar");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<Tipo_Evento>>(options);
                    return (true, dados ?? new List<Tipo_Evento>());
                }
                else
                {
                    var errorMessage = await resposta.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao consultar tipo de evento. Status: {resposta.StatusCode}, Erro: {errorMessage}");
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar tipo de evento: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> AlterarRegraNotificacao(string token, Configura_Notificacao configura_Notificacao)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                var resultado = await _httpClient.PutAsJsonAsync("notificacao/regra-alterar", configura_Notificacao);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var mensagem = await resultado.Content.ReadFromJsonAsync<ApiResposta>(options);
                if (resultado.IsSuccessStatusCode)
                {
                    string mensagemSucesso = mensagem?.Mensagem;

                    return (true, mensagemSucesso);
                }
                else
                {
                    string mensagemFalha = mensagem?.Mensagem;
                    return (false, mensagemFalha);
                }
            }
            catch (Exception ex)
            {
                // Log ajustado para a ação correta
                System.Diagnostics.Debug.WriteLine($"Exceção ao alterar notificação: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> CadastrarNotificacoes(string token, Configura_Notificacao configura_Notificacao)
        {
            try
            {
                // Limpa o token para evitar duplicação do prefixo "Bearer "
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var resposta = await _httpClient.PostAsJsonAsync("notificacao/regra-cadastrar", configura_Notificacao);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var mensagemResposta = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);

                if (resposta.IsSuccessStatusCode)
                {
                    string mensagemApi = mensagemResposta?.Mensagem ?? "Regra cadastrada com sucesso!";
                    return (true, mensagemApi);
                }
                else
                {
                    string mensagemApi = mensagemResposta?.Mensagem ?? "Erro desconhecido ao processar requisição.";
                    return (false, mensagemApi);
                }
            }
            catch (Exception ex)
            {
                // Log ajustado para a ação correta
                System.Diagnostics.Debug.WriteLine($"Exceção ao cadastrar notificação: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }
    }
}
