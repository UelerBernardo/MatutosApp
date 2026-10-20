using MatutosDomain;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MatutosApp.Services
{
    public class RelatorioService
    {
        private readonly HttpClient _httpClient;

        public RelatorioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Sucesso, string Mensagem, List<RelatorioFaturamentoResponseDTO>? Dados)> GerarRelatorioFaturamento(string token, FiltroFaturamentoDTO filtros)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resposta = await _httpClient.PostAsJsonAsync("relatorio/faturamento", filtros);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<RelatorioFaturamentoResponseDTO>>(options);
                    return (true, string.Empty, dados);
                }
                else
                {
                    // Trata erros que voltarem da API
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);
                    return (false, erroResposta?.Mensagem ?? "Erro ao consultar o relatório.", null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção no relatório: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, List<RelatorioClientesNaoFidelizadosResponseDTO>? Dados)> GerarRelatorioClientesNaoFidelizados(string token, int? qtdDias)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                // Monta a URL. Se o usuário digitou algum dia, adiciona o ?qtdDias=X no final
                string url = "relatorio/clientes-nao-fidelizados";
                if (qtdDias.HasValue)
                {
                    url += $"?qtdDias={qtdDias.Value}";
                }

                // Voltamos a usar o GetAsync, enviando a URL que montamos acima
                var resposta = await _httpClient.GetAsync(url);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<RelatorioClientesNaoFidelizadosResponseDTO>>(options);
                    return (true, string.Empty, dados);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);
                    return (false, erroResposta?.Mensagem ?? "Erro ao consultar o relatório.", null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao consultar clientes: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, List<RelatorioServicosPorBarbeiroResponseDTO>? Dados)> GerarRelatorioServicosPorBarbeiro(string token, FiltroServicosPorBarbeiroDTO parametros)
        {
            try
            {
                // Limpa o token caso a string venha com o prefixo "Bearer "
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                // DICA: Ajuste o prefixo "relatorio/" conforme o nome da sua Controller na API!
                var resposta = await _httpClient.PostAsJsonAsync("relatorio/servicos-barbeiro", parametros);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<RelatorioServicosPorBarbeiroResponseDTO>>(options);

                    return (true, string.Empty, dados);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao consultar o relatório de barbeiros.";
                    System.Diagnostics.Debug.WriteLine($"Falha na API (Relatório Barbeiros): {mensagemApi}");

                    return (false, mensagemApi, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao consultar relatório de barbeiros: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão.", null);
            }
        }
    }
}
