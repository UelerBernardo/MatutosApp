using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class TelefoneViewModel : BaseViewModel
    {
        private readonly TelefoneService _telefoneService;

        // O Toolkit vai gerar: Numero_telefone, Ddd e Principal
        [ObservableProperty] private string numero_telefone;
        [ObservableProperty] private string ddd;
        [ObservableProperty] private bool principal;

        public TelefoneViewModel(TelefoneService telefone)
        {
            _telefoneService = telefone;
        }

        private async Task CadastrarTelefone()
        {
            try
            {
                string tokenJwt = await SecureStorage.Default.GetAsync("jwt_token");

                if (string.IsNullOrEmpty(tokenJwt))
                {
                    await App.Current.MainPage.DisplayAlert("Atenção", "Sessão expirada. Por favor, faça login novamente.", "OK");
                    return;
                }

                var telefoneNovo = new Telefone
                {
                    DDD = Ddd,
                    Numero_Telefone = Numero_telefone,
                    Principal = Principal
                };

                var resultado = await _telefoneService.TelefoneCadastrar(telefoneNovo, tokenJwt);

                if (resultado.Sucesso)
                {
                    bool confirmar = await Shell.Current.DisplayAlert("Sucesso", "Telefone Cadastrado com sucesso! Deseja cadastrar um novo telefone?", "Sim", "Não");

                    if (!confirmar)
                    {
                        await Shell.Current.GoToAsync(nameof(PrincipalView)); 
                    }
                    else
                    {
                        LimparDados();
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Cadastrar() // Adicionado sufixo Async
        {
            if (string.IsNullOrEmpty(Numero_telefone) || string.IsNullOrEmpty(Ddd))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos.", "OK");
                return;
            }

            // CORRIGIDO: Agora com o await para o fluxo aguardar o processamento
            await CadastrarTelefone();
        }

        public void LimparDados()
        {
            Numero_telefone = string.Empty;
            Ddd = string.Empty;
            Principal = false;
        }
    }
}
