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
        //private readonly ClienteService _clienteService;

        private readonly UsuarioService _usuarioService;

        [ObservableProperty] private Usuario usuarioPerfil;

        [ObservableProperty] private bool isPopupSenhaVisivel;
        [ObservableProperty] private string senhaAtual;
        [ObservableProperty] private string novaSenha;
        [ObservableProperty] private string confirmarNovaSenha;


        public ClientePerfilConsultarViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _ = ConsultarPerfil();
        }

        public void AtualizarFotoTela()
        {
            // Como a sua classe herda de BaseViewModel, você tem acesso ao OnPropertyChanged.
            // Isso avisa o XAML: "Ei, releia a propriedade FotoPerfilSource agora!"
            OnPropertyChanged(nameof(FotoPerfilSource));
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
        public async Task ConsultarTelefone()
        {
            await Shell.Current.GoToAsync(nameof(UsuarioTelefoneConsultarView));
        }

        [RelayCommand]
        public async Task AbrirMeusDados()
        {
            await Shell.Current.GoToAsync(nameof(UsuarioCadastroView));
        }
        [RelayCommand]
        public async Task AbrirAgendamentos()
        {
            await Shell.Current.GoToAsync(nameof(AgendamentoConsultarView));
        }

        [RelayCommand]
        public async Task AbrirImagemUsuario()
        {
            Usuario usuarioLogado = UsuarioSessaoService.UsuarioLogado;

            // 2. Blindagem: Se por algum motivo a sessão estiver vazia, não faz nada (evita crash)
            if (usuarioLogado == null)
                return;

            var parametros = new Dictionary<string, object>
                {
                    { "FotoAtual", usuarioLogado.Imagem_Usuario }
                };
            await Shell.Current.GoToAsync(nameof(UsuarioImagemView), parametros);
        }

        public async Task ConsultarPerfil()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("jwt_token");

                var resultadoUsuario = await _usuarioService.ConsultarPefil(token);

                if (resultadoUsuario != null)
                {
                    UsuarioPerfil = resultadoUsuario;
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

            if(NovaSenha == SenhaAtual)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "A senha nova não pode ser igual a senha atual!", "Ok");
                return;
            }

            var token = await SecureStorage.Default.GetAsync("jwt_token");

            var resultado = await _usuarioService.AlterarSenha(token, SenhaAtual, NovaSenha);

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
