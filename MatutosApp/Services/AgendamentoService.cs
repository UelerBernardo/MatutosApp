using MatutosDomain;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class AgendamentoService
    {
        private readonly HttpClient _httpClient;

        public AgendamentoService(HttpClient httpClient)
        {
            //string baseURL = DeviceInfo.Platform == DevicePlatform.Android
            //  ? "https://10.0.2.2:7110/" // 👉 Emulador acessando a máquina (HTTPS)
            //  : "https://localhost:7110/";
            //_httpClient = new HttpClient(ObterManipuladorInseguro())
            //{
            //    BaseAddress = new Uri(baseURL)
            _httpClient = httpClient;
        }

        public async Task<(bool Sucesso, string Mensagem, List<string>? Dados)> ConsultarHorariosLivres(string token, int codigoBarbeiro, DateTime dataAgendamento)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string dataFormatada = dataAgendamento.ToString("yyyy-MM-dd");

                string urlCompleta = $"agendamento/consultar/horarios-livres?codigoBarbeiro={codigoBarbeiro}&dataAgendamento={dataFormatada}";

                var resposta = await _httpClient.GetAsync(urlCompleta);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<string>>(options);
                    return (true, "Horários carregados com sucesso.", dados);
                }
                else
                {
                    // Tenta ler a mensagem de erro que o BadRequest ou StatusCode(500) enviou
                    var erro = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);
                    return (false, erro?.Mensagem ?? "Não foi possível carregar os horários livres.", null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao consultar horários livres: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, List<MeusServicos>? Dados)> ConsultarMeusServicos(string token, DateTime dataInicial, DateTime dataFinal, string? nomeUsuario, AgendamentoSituacao? situacao, bool isAdmin)
        {
            try
            {

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var queryParams = new List<string>
                    {
                        $"dataInicial={dataInicial:yyyy-MM-ddTHH:mm:ss}",
                        $"dataFinal={dataFinal:yyyy-MM-ddTHH:mm:ss}"
                    };

                queryParams.Add($"isAdmin={isAdmin}");

                if (!string.IsNullOrWhiteSpace(nomeUsuario))
                {
                    queryParams.Add($"nomeUsuario={Uri.EscapeDataString(nomeUsuario)}");
                }


                if (situacao.HasValue)
                {
                    queryParams.Add($"situacao={(int)situacao.Value}");
                }

                // 3. Juntamos tudo com '&' (ex: dataIncial=...&nomeCliente=...)
                string queryString = string.Join("&", queryParams);

                // Ajuste a rota base se o seu Controller exigir (ex: "agendamento/meus-servicos?")
                string urlCompleta = $"agendamento/meus-servicos?{queryString}";

                // 4. Executamos a requisição GET
                var resposta = await _httpClient.GetAsync(urlCompleta);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (resposta.IsSuccessStatusCode)
                {
                    var dados = await resposta.Content.ReadFromJsonAsync<List<MeusServicos>>(options);
                    return (true, "Consulta realizada com sucesso.", dados);
                }
                else
                {
                    var erro = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);
                    return (false, erro?.Mensagem ?? "Não foi possível carregar os serviços.", null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao consultar meus serviços: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão com a internet.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, int IdAgendamento)> AgendamentoServicoCadastrar(List<Agendamento_Servico> agendamento_Servico, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.PostAsJsonAsync("agendamento/cadastrar/agendamentoServico", agendamento_Servico);

                if(resposta.IsSuccessStatusCode)
                {
                    var agendamentoNovo = await resposta.Content.ReadFromJsonAsync<Agendamento>();
                    return (true, string.Empty, agendamentoNovo.Codigo_Agendamento);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();
                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha: {mensagemApi}");

                    return (false, mensagemApi,0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.",0);
            }

        }

        public async Task<(bool Sucesso, string Mensagem, int NovoId)> AgendamentoCadastrar(Agendamento agendamento, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.PostAsJsonAsync("agendamento/cadastrar", agendamento);

                if(resposta.IsSuccessStatusCode)
                {
                    var agendamentoNovo = await resposta.Content.ReadFromJsonAsync<Agendamento>();
                    return (true, string.Empty, agendamentoNovo?.Codigo_Agendamento ?? 0);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();
                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha: {mensagemApi}");

                    return (false, mensagemApi, 0);
                }
            }

            catch(Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", 0);
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> AgendamentoExcluir(int codigoAgendamento, string token)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var conteudoVazio = new StringContent("{}", Encoding.UTF8, "application/json");

                var resposta = await _httpClient.PatchAsync($"agendamento/inativarAgendamento/{codigoAgendamento}", conteudoVazio);

                if (resposta.IsSuccessStatusCode)
                {
                    return (true, "Agendamento cancelado com sucesso.");
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha na API: {mensagemApi}");

                    return (false, mensagemApi);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao inativar agendamento: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }

       public async Task<(bool Sucesso, string Mensagem, Agendamento? Dados)> AgendamentoConsultarDetalhes(int codigoAgendamento, string token)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var resposta = await _httpClient.GetAsync($"agendamento/consultar/detalhes/{codigoAgendamento}");

                if (resposta.IsSuccessStatusCode)
                {

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = await resposta.Content.ReadFromJsonAsync<Agendamento>(options);

                    return (true, string.Empty, dados);
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha na API: {mensagemApi}");

                    return (false, mensagemApi, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar detalhes: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem, List<MeusAgendamentos>? Dados)> AgendamentoConsultar( string token, string? nomeBarbeiro, AgendamentoSituacao? situacao, DateTime dataInicial, DateTime dataFinal)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var query = new List<string>
                {
                    $"dataInicial={dataInicial: yyyy-MM-ddTHH:mm:ss}",
                    $"dataFinal={dataFinal: yyyy-MM-ddTHH:mm:ss}"
                };

                if(!string.IsNullOrEmpty(nomeBarbeiro))
                {
                    query.Add($"nomeBarbeiro={Uri.EscapeDataString(nomeBarbeiro)}");
                }

                if(situacao.HasValue)
                {
                    query.Add($"situacao={(int)situacao.Value}");
                }

                string queryFinal = string.Join("&", query);

                string urlCompleta = $"agendamento/meus-agendamentos?{queryFinal}";

                var resposta = await _httpClient.GetAsync(urlCompleta);

                if (resposta.IsSuccessStatusCode)
                {

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = await resposta.Content.ReadFromJsonAsync<List<MeusAgendamentos>>(options);

                    return (true, string.Empty, dados);
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha na API: {mensagemApi}");

                    return (false, mensagemApi, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar detalhes: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> AgendamentoAlterarSituacao(int codigoAgendamento, string token, AgendamentoSituacao agendamentoSituacao)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var jsonConteudo = JsonSerializer.Serialize(agendamentoSituacao);
                var conteudo = new StringContent(jsonConteudo, Encoding.UTF8, "application/json");

                var resposta = await _httpClient.PatchAsync($"agendamento/alterarSituacao/{codigoAgendamento}", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    return (true, "Situação alterada com Sucesso.");
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha na API: {mensagemApi}");

                    return (false, mensagemApi);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao inativar agendamento: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }
    }
}
