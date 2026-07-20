
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    //[QueryProperty(nameof(AdministradorCadastro), "CadastroDeUsuario")]
    public partial class UsuarioViewModel : BaseViewModel, IQueryAttributable
    {
        public readonly UsuarioService? _usuarioService;

        [ObservableProperty] private bool administradorCadastro;

        //Propriedade associadas ao usuário
        [ObservableProperty] private string nome;
        [ObservableProperty] private string email;
        [ObservableProperty] private string senha;
        [ObservableProperty] private UsuarioTipo usuarioTipoSelecionado;

        [ObservableProperty] private bool isModoCadastro;
        [ObservableProperty] private bool isModoAlteracao;

        [ObservableProperty] private UsuarioTipo usuarioTipoLogado;

        [ObservableProperty]
        private bool podeEditarTipoUsuario = true;

        [ObservableProperty]
        private bool podeEditarSenha = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NomeBotaoAcao))]
        [NotifyPropertyChangedFor(nameof(Mensagem))]
        private AcaoTela _acaoTela;

        public string NomeBotaoAcao => AcaoTela == AcaoTela.Cadastro ? "Criar Conta" : "Alterar Perfil";

        public string Mensagem => AcaoTela == AcaoTela.Cadastro ? "Crie a sua Conta!" : "Editar Dados";

        public ObservableCollection<UsuarioTipo> usuarioTipoDisponivel { get; }

        public UsuarioViewModel(UsuarioService api)
        {
            _usuarioService = api;
            usuarioTipoDisponivel = new ObservableCollection<UsuarioTipo>(Enum.GetValues(typeof(UsuarioTipo)).Cast<UsuarioTipo>());

            EcontrarTipoUsuarioLogado();
            DefinirModoDaTela();
        }

        private void EcontrarTipoUsuarioLogado()
        {
            var usuarioLogado = UsuarioSessaoService.UsuarioLogado;

            if (usuarioLogado == null)
            {
                UsuarioTipoLogado = 0;
                return;
            }

            if (usuarioLogado.TipoSelecionado == UsuarioTipo.Cliente)
            {
                UsuarioTipoLogado = UsuarioTipo.Cliente;
            }
            else if (usuarioLogado.TipoSelecionado == UsuarioTipo.Barbeiro)
            {
                UsuarioTipoLogado = UsuarioTipo.Barbeiro;
            }
            else
            {
                UsuarioTipoLogado = UsuarioTipo.Administrador;
            }
        }


        private void DefinirModoDaTela()
        {
            var usuario = UsuarioSessaoService.UsuarioLogado;

            if(AdministradorCadastro == true)
            {
                if (UsuarioTipoLogado == UsuarioTipo.Administrador)
                {
                    _acaoTela = AcaoTela.Cadastro;

                    IsModoAlteracao = false;
                    IsModoCadastro = true;

                    Nome = string.Empty;
                    Email = string.Empty;
                    Senha = string.Empty;
                    UsuarioTipoSelecionado = UsuarioTipo.Cliente;
                    PodeEditarSenha = true;
                    PodeEditarTipoUsuario = true;
                }
            }
            else
            { 

                if (usuario != null)
                {
                    ModoAlteracao(usuario);
                }
                else
                {
                    ModoCadastro();
                }
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
            Senha = "**********"; 
            PodeEditarSenha = false;
            PodeEditarTipoUsuario = false;
        }

        private void ModoCadastro()
        {
            if(UsuarioTipoLogado == 0)
            { 
                _acaoTela = AcaoTela.Cadastro;

                // Avisa o XAML que é um cadastro novo
                IsModoAlteracao = false;
                IsModoCadastro = true;

                Nome = string.Empty;
                Email = string.Empty;
                Senha = string.Empty;
                UsuarioTipoSelecionado = UsuarioTipo.Cliente;
                PodeEditarTipoUsuario = false;
                PodeEditarSenha = true;
            }
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
                await AlterarUsuario();
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

                var resultado = await _usuarioService.UsuarioCadastrar(usuarioNovo);

                if (resultado.Sucesso)
                {
                    if(UsuarioSessaoService.UsuarioLogado == null)
                    {
                        UsuarioSessaoService.IniciarSessao(resultado.Dados);
                    }

                    var confirmar = await Application.Current.MainPage.DisplayAlert("Quase lá!", "Cadastro concluído. Deseja realizar o cadastro de telefone?", "Sim", "Não");

                    if(!confirmar)
                    {
                        await Shell.Current.GoToAsync("///PrincipalView");
                    }
                    else
                    {

                        await Shell.Current.GoToAsync("TelefoneCadastroView");
                    }

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

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Verifica se a chave que você enviou existe no pacote
            if (query.TryGetValue("CadastroDeUsuario", out var valorEnviado))
            {
                if (valorEnviado is bool valorBooleano)
                {
                    AdministradorCadastro = valorBooleano;
                }
                else if (valorEnviado is string valorString && bool.TryParse(valorString, out bool convertido))
                {
                    AdministradorCadastro = convertido;
                }

                DefinirModoDaTela();
            }
        }

        [RelayCommand]
        public async Task Cancelar()
        {
            if(usuarioTipoLogado != null)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            { 
                Application.Current.MainPage = new AppShell();
            }
        }
    }
}
