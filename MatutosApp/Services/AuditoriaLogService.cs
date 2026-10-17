
using MatutosDomain;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class AuditoriaLogService
    {
        private readonly HttpClient _httpClient;

        public AuditoriaLogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public  async Task<(bool Sucesso, string? Mensagem, AuditoriaLogDetalhesDTO Dados)> LogConsultarDetalhes(string token, int id)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resultado = await _httpClient.GetAsync($"auditoria-log/consultar-detalhes/{id}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resultado.IsSuccessStatusCode)
                {
                    var dados = await resultado.Content.ReadFromJsonAsync<AuditoriaLogDetalhesDTO>(options);

                    return (true, string.Empty, dados);
                }
                else
                {
                    var mensagemApi = await resultado.Content.ReadFromJsonAsync<ApiErroResposta>(options);
                    return (false, mensagemApi.Mensagem, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao consultar os logs: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, List<AuditoriaLog>? Dados)> LogConsultarLista(string token, FiltroLogDTO parametros)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resultado = await _httpClient.PostAsJsonAsync("auditoria-log/consulta-lista", parametros);

                // 👉 ADICIONADO AQUI: O conversor de Enum de Texto para o C#
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter()); // Essa é a linha mágica!

                if (resultado.IsSuccessStatusCode)
                {
                    // Agora o ReadFromJson vai usar a nossa opção configurada e lerá o Enum perfeitamente
                    var dados = await resultado.Content.ReadFromJsonAsync<List<AuditoriaLog>>(options);

                    return (true, string.Empty, dados);
                }
                else
                {
                    var mensagemApi = await resultado.Content.ReadFromJsonAsync<ApiResposta>(options);

                    return (false, mensagemApi?.Mensagem ?? "Erro ao consultar logs.", null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao consultar os logs: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão.", null);
            }
        }
    }
}