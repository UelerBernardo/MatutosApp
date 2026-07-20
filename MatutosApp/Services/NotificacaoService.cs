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

        public async Task<(bool Sucesso, List<Configura_Notificacao>? Dados)> ConsultarRegraNotificacao(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resposta = await _httpClient.GetAsync("notificacao/regra-consultar");

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
                // Limpa o token para evitar duplicação do prefixo "Bearer "
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
