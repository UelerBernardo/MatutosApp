using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class ClientePerfilConsultarViewModel : BaseViewModel
    {
        private readonly ClienteService _clienteService;
        [ObservableProperty] private Cliente clientePerfil;
        [ObservableProperty] private bool isPopupSenhaVisivel;
        [ObservableProperty] private string senhaAtual;
        [ObservableProperty] private string novaSenha;
        [ObservableProperty] private string confirmarNovaSenha;

        public ClientePerfilConsultarViewModel(ClienteService clienteService)
        {
            _clienteService = clienteService;
            _ = ConsultarClientePerfil();
        }

        [RelayCommand]
        public async Task ConsultarTelefone()
        {
            await Shell.Current.GoToAsync(nameof(UsuarioTelefoneConsultarView));
        }

        public async Task ConsultarClientePerfil()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("jwt_token");

                var resultadoCliente = await _clienteService.ClienteConsultarPefil(token);

                if (resultadoCliente != null)
                {
                    ClientePerfil = resultadoCliente;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao carregar perfil: {ex.Message}", "Ok");
            }
        }
        [RelayCommand]
        public void AbrirPopupSenha()
        {
            IsPopupSenhaVisivel = true;
        }

        [RelayCommand]
        public void FecharPopupSenha()
        {
            IsPopupSenhaVisivel = false;
            SenhaAtual = string.Empty;
            NovaSenha = string.Empty;
            ConfirmarNovaSenha = string.Empty;
        }

        [RelayCommand]
        public async Task SalvarNovaSenha()
        {
            if (string.IsNullOrWhiteSpace(SenhaAtual) || string.IsNullOrWhiteSpace(NovaSenha) || string.IsNullOrWhiteSpace(ConfirmarNovaSenha))
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Preencha todos os campos.", "Ok");
                return;
            }

            if (NovaSenha != ConfirmarNovaSenha)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "As senhas novas não coincidem.", "Ok");
                return;
            }

            var token = await SecureStorage.Default.GetAsync("jwt_token");

            var resultado = await _clienteService.AlterarSenhaCliente(token, SenhaAtual, NovaSenha);

            if(resultado.Sucesso)
            {
                await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");
                FecharPopupSenha();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
            }

        }
    }
}
