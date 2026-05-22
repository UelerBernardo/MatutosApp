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

        public ObservableCollection<UsuarioTipo> usuarioTipoDisponivel { get; }

        public UsuarioViewModel(UsuarioService api)
        {
            _usuarioService = api;
            usuarioTipoDisponivel = new ObservableCollection<UsuarioTipo>(Enum.GetValues(typeof(UsuarioTipo)).Cast<UsuarioTipo>());

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
        private async Task Cadastrar()
        {
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos.", "OK");
                return;
            }
            await CadastrarUsuario();

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
