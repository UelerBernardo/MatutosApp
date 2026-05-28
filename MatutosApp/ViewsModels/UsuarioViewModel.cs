
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class UsuarioViewModel : BaseViewModel 
    {
        public readonly UsuarioService? _usuarioService;

        //Propriedade associadas ao usuário
        [ObservableProperty] private string nome;
        [ObservableProperty] private string email;
        [ObservableProperty] private string senha;
        [ObservableProperty] private UsuarioTipo usuarioTipoSelecionado;

        [ObservableProperty] private bool isModoCadastro;
        [ObservableProperty] private bool isModoAlteracao;

        private AcaoTela _acaoTela;

        public ObservableCollection<UsuarioTipo> usuarioTipoDisponivel { get; }

        public UsuarioViewModel(UsuarioService api)
        {
            _usuarioService = api;
            usuarioTipoDisponivel = new ObservableCollection<UsuarioTipo>(Enum.GetValues(typeof(UsuarioTipo)).Cast<UsuarioTipo>());

            DefinirModoDaTela();

        }


        private void DefinirModoDaTela()
        {
            var usuario = UsuarioSessaoService.UsuarioLogado;

            if (usuario != null)
            {
                ModoAlteracao(usuario);
            }
            else
            {
                ModoCadastro();
            }
        }

        private void ModoAlteracao(Usuario usuario)
        {
            _acaoTela = AcaoTela.Alteração;

            // Avisa o XAML que estamos alterando
            IsModoAlteracao = true;
            IsModoCadastro = false;

            Nome = usuario.Nome;
            Email = usuario.Email;
            UsuarioTipoSelecionado = usuario.TipoSelecionado;
            Senha = "**********"; // Apenas máscara visual para a tela
        }

        private void ModoCadastro()
        {
            _acaoTela = AcaoTela.Cadastro;

            // Avisa o XAML que é um cadastro novo
            IsModoAlteracao = false;
            IsModoCadastro = true;

            Nome = string.Empty;
            Email = string.Empty;
            Senha = string.Empty;
            UsuarioTipoSelecionado = UsuarioTipo.Cliente;
        }


        [RelayCommand]
        private async Task Logar()
        {
            if ( string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos.", "OK");
                return;
            }
            await LoginUsuario();

        }

        [RelayCommand]
        private async Task CadastrarOuAlterar()
        {
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos.", "OK");
                return;
            }

            if(_acaoTela == AcaoTela.Alteração)
            {
                AlterarUsuario();
            }
            else
            {
                await CadastrarUsuario();
            }


        }

        private async Task AlterarUsuario()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");
                if (string.IsNullOrEmpty(token))
                {
                    await App.Current.MainPage.DisplayAlert("Atenção", "Sessão expirada. Por favor, faça login novamente.", "OK");
                    return;
                }

                var usuarioAlteracao = new Usuario
                {
                    Nome = Nome,
                    Email = Email,
                    Senha = Senha,
                    TipoSelecionado = UsuarioTipoSelecionado
                };

                var resultado = await _usuarioService.UsuarioAlterar(usuarioAlteracao, token); 

                if (resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Continuar");

                    await Shell.Current.GoToAsync(nameof(ClientePerfilConsultarView));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "Ok");
            }
        }

        private async Task CadastrarUsuario()
        {
            try
            {
                var usuarioNovo = new UsuarioCadastro
                {
                    Nome = Nome,
                    Email = Email,
                    Senha = Senha,
                    TipoSelecionado = UsuarioTipoSelecionado
                };

                bool sucesso = await _usuarioService.UsuarioCadastrar(usuarioNovo);
                if (sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Quase lá!", "Cadastro inicial concluído. Agora, informe seu telefone para contato.", "Continuar");

                    await Shell.Current.GoToAsync("TelefoneCadastroView");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Não foi possível realizar o cadastro.", "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "Ok");
            }
        }

        private async Task LoginUsuario()
        {
            try
            {
                var login = new UsuarioLogin
                {
                    Email = Email, 
                    Senha = Senha
                };
                var resultado = await _usuarioService.UsuarioLogin(login);

                if (resultado.Sucesso)
                {
                    UsuarioSessaoService.IniciarSessao(resultado.Dados);

                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Seja Bem-vindo!", "Ok");

                    await Shell.Current.GoToAsync(nameof(PrincipalView));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "OK");
            }
        }
    }
}
