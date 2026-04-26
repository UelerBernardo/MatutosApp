using Azure;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class TelefoneService
    {
        private readonly HttpClient _httpClient;

        public TelefoneService()
        {
            string baseURL = "http://localhost:5015";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseURL)
            };
        }

        public async Task<bool> TelefoneCadastrar(Telefone telefone, string tokenJwt)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJwt);
                var response = await _httpClient.PostAsJsonAsync("telefone/cadastrar", telefone);

                if (response.IsSuccessStatusCode)
                {
                    var telefoneNovo = await response.Content.ReadFromJsonAsync<Telefone>();
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao cadastrar telefone. Status: {response.StatusCode}, Erro: {errorMessage}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar pessoa: {ex.Message}");
                return false;
            }
            
        }

    }
}
