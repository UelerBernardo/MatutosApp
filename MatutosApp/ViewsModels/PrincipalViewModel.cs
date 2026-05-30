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
    public partial class PrincipalViewModel : BaseViewModel
    {

        private readonly ClienteService _clienteService;

        [ObservableProperty] private Cliente clientePerfil; 

        public PrincipalViewModel(ClienteService clienteService)
        {
            _clienteService = clienteService;
            _ = ConsultarClientePerfil();
        }

        public ImageSource FotoPerfilSource
        {
            get
            {
                var usuarioLogado = UsuarioSessaoService.UsuarioLogado;

                return usuarioLogado == null || string.IsNullOrEmpty(usuarioLogado.Imagem_Usuario)
                    ? "user_placeholder.png"
                    : ImageSource.FromUri(new Uri($"https://localhost:7110{usuarioLogado.Imagem_Usuario}"));
            }
        }


        [RelayCommand]
        public async Task AbrirAgendamento()
        {
            await Shell.Current.GoToAsync(nameof(AgendamentoCadastroView));
        }

        [RelayCommand]
        public async Task MeusAgendamentos()
        {
            await Shell.Current.GoToAsync(nameof(AgendamentoConsultarView));
        }

        [RelayCommand]
        public async Task PerfilAbrir()
        {
            await Shell.Current.GoToAsync(nameof(ClientePerfilConsultarView));
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
    }
}
