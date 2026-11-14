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

        public async Task<(bool Sucesso, string Mensagem, byte[]? ArquivoPdf)> GerarRelatorioFaturamentoPdf(string token, FiltroFaturamentoDTO parametros)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                // Aponta para a nova rota que devolve o PDF
                var resposta = await _httpClient.PostAsJsonAsync("relatorio/faturamento/pdf", parametros);

                if (resposta.IsSuccessStatusCode)
                {
                    var pdfBytes = await resposta.Content.ReadAsByteArrayAsync();
                    return (true, string.Empty, pdfBytes);
                }
                else
                {
                    // Trata erros que vêm em JSON
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);
                    return (false, erroResposta?.Mensagem ?? "Erro ao consultar relatório.", null);
                }
            }
            catch (Exception ex)
            {
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, byte[]? ArquivoPdf)> GerarRelatorioServicosPorBarbeiroPdf(string token, FiltroServicosPorBarbeiroDTO parametros)
            {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var resposta = await _httpClient.PostAsJsonAsync("relatorio/servicos-barbeiro/pdf", parametros);

                if (resposta.IsSuccessStatusCode)
                {
                    var pdfBytes = await resposta.Content.ReadAsByteArrayAsync();
                    return (true, string.Empty, pdfBytes);
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);
                    return (false, erroResposta?.Mensagem ?? "Erro ao gerar PDF.", null);
                }
            }
            catch (Exception)
            {
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, byte[]? ArquivoPdf)> GerarRelatorioClientesNaoFidelizadosPdf(string token, int? qtdDias)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                string url = "relatorio/clientes-nao-fidelizados/pdf";
                if (qtdDias.HasValue) url += $"?qtdDias={qtdDias.Value}";

                var resposta = await _httpClient.GetAsync(url);

                if (resposta.IsSuccessStatusCode)
                {
                    var pdfBytes = await resposta.Content.ReadAsByteArrayAsync();
                    return (true, string.Empty, pdfBytes);
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiResposta>(options);
                    return (false, erroResposta?.Mensagem ?? "Erro ao gerar PDF.", null);
                }
            }
            catch (Exception)
            {
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }
    }
}
