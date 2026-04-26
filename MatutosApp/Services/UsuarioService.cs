using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class UsuarioService
    {
        private readonly HttpClient? _httpClient;

        public UsuarioService()
        {
            string baseUrl = "http://localhost:5028";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)

            };
        }


        public async Task<bool> UsuarioCadastrar(UsuarioCadastro usuario)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("usuario/cadastrar", usuario);

                if (response.IsSuccessStatusCode)
                {
                    var pessoaNova = await response.Content.ReadFromJsonAsync<Usuario>();
                    return true;
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

    }
}
