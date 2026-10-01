using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class UsuarioViewModel : BaseViewModel, IQueryAttributable
    {
        public readonly UsuarioService? _usuarioService;

        [ObservableProperty] private bool administradorCadastro;

        // Propriedade associadas ao usuário
        [ObservableProperty] private string nome;
        [ObservableProperty] private string email;

        // Lógica de senha forte
        [ObservableProperty] private string senha;
        [ObservableProperty] private bool temTamanhoMinimo;
        [ObservableProperty] private bool temMaiuscula;
        [ObservableProperty] private bool temNumero;
        [ObservableProperty] private bool temEspecial;

        public bool SenhaForte => TemTamanhoMinimo && TemMaiuscula && TemNumero && TemEspecial;

        [ObservableProperty] private UsuarioTipo usuarioTipoSelecionado;
        [ObservableProperty] private bool emailInvalido;
        [ObservableProperty] private string emailConfirmacao;
        [ObservableProperty] private string senhaConfirmacao;
        [ObservableProperty] private bool isModoCadastro;
        [ObservableProperty] private bool isModoAlteracao;

        [ObservableProperty] private UsuarioTipo usuarioTipoLogado;

        [ObservableProperty] private bool podeEditarTipoUsuario = true;
        [ObservableProperty] private bool podeEditarSenha = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NomeBotaoAcao))]
        [NotifyPropertyChangedFor(nameof(Mensagem))]
        private AcaoTela acaoTela; // IMPORTANTE: O toolkit gera a propriedade com letra maiúscula (AcaoTela)

        public string NomeBotaoAcao => AcaoTela == AcaoTela.Cadastro ? "Criar Conta" : "Alterar Perfil";
        public string Mensagem => AcaoTela == AcaoTela.Cadastro ? "Crie a sua Conta!" : "Editar Dados";

        public ObservableCollection<UsuarioTipo> usuarioTipoDisponivel { get; }

        public UsuarioViewModel(UsuarioService api)
        {
            _usuarioService = api;
            usuarioTipoDisponivel = new ObservableCollection<UsuarioTipo>(Enum.GetValues(typeof(UsuarioTipo)).Cast<UsuarioTipo>());

            EcontrarTipoUsuarioLogado();

            // Chamamos aqui para cobrir os Cenários 1 (Novo usuário) e 2 (Usuário editando o próprio perfil), 
            // que não disparam o ApplyQueryAttributes por não terem parâmetros de rota.
            DefinirModoDaTela();
        }

        partial void OnSenhaChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                TemTamanhoMinimo = false;
                TemMaiuscula = false;
                TemNumero = false;
                TemEspecial = false;
            }
            else
            {
                TemTamanhoMinimo = value.Length >= 8;
                TemMaiuscula = value.Any(char.IsUpper);
                TemNumero = value.Any(char.IsDigit);
                TemEspecial = value.Any(ch => !char.IsLetterOrDigit(ch));
            }

            OnPropertyChanged(nameof(SenhaForte));
        }

        partial void OnEmailChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                EmailInvalido = false;
                return;
            }
            string padraoRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            EmailInvalido = !Regex.IsMatch(value, padraoRegex);
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
                UsuarioTipoLogado = UsuarioTipo.Cliente;
            else if (usuarioLogado.TipoSelecionado == UsuarioTipo.Barbeiro)
                UsuarioTipoLogado = UsuarioTipo.Barbeiro;
            else
                UsuarioTipoLogado = UsuarioTipo.Administrador;
        }

        // Centralizamos os 3 cenários aqui de forma clara e direta
        private void DefinirModoDaTela()
        {
            var usuario = UsuarioSessaoService.UsuarioLogado;

            // CENÁRIO 3: Admin cadastrando um NOVO usuário (Recebeu parâmetro true)
            if (AdministradorCadastro && usuario != null && UsuarioTipoLogado == UsuarioTipo.Administrador)
            {
                AcaoTela = AcaoTela.Cadastro; // Use sempre a propriedade gerada (Capitalizada) para ativar o NotifyPropertyChangedFor
                IsModoAlteracao = false;
                IsModoCadastro = true;

                Nome = string.Empty;
                Email = string.Empty;
                Senha = string.Empty;
                UsuarioTipoSelecionado = UsuarioTipo.Cliente;

                PodeEditarSenha = true;
                PodeEditarTipoUsuario = true; // Admin pode alterar o tipo da pessoa
            }
            // CENÁRIO 2: Usuário já logado chamou a tela (Edição do próprio perfil)
            else if (usuario != null && !AdministradorCadastro)
            {
                AcaoTela = AcaoTela.Alteração;
                IsModoAlteracao = true;
                IsModoCadastro = false;

                Nome = usuario.Nome;
                Email = usuario.Email;
                UsuarioTipoSelecionado = usuario.TipoSelecionado;
                Senha = "**********";

                PodeEditarSenha = false;
                PodeEditarTipoUsuario = false; // Bloqueia o tipo de pessoa
            }
            // CENÁRIO 1: Nenhum usuário logado (Visitante criando a primeira conta)
            else if (usuario == null)
            {
                AcaoTela = AcaoTela.Cadastro;
                IsModoAlteracao = false;
                IsModoCadastro = true;

                Nome = string.Empty;
                Email = string.Empty;
                Senha = string.Empty;
                UsuarioTipoSelecionado = UsuarioTipo.Cliente; // Força como cliente

                PodeEditarSenha = true;
                PodeEditarTipoUsuario = false; // Bloqueia o tipo de pessoa para ficar invisível/desabilitado na UI
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

            if (AcaoTela == AcaoTela.Alteração)
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
                if (Email != EmailConfirmacao)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Os e-mails informados não coincidem.", "Ok");
                    return;
                }

                if (!SenhaForte)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "A senha ainda não atende aos requisitos de segurança.", "OK");
                    return;
                }

                if (Senha != SenhaConfirmacao)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "As senhas informadas não coincidem.", "Ok");
                    return;
                }

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
                    if (UsuarioSessaoService.UsuarioLogado == null)
                    {
                        UsuarioSessaoService.IniciarSessao(resultado.Dados);
                    }

                    var confirmar = await Application.Current.MainPage.DisplayAlert("Quase lá!", "Cadastro concluído. Deseja realizar o cadastro de telefone?", "Sim", "Não");

                    if (!confirmar)
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
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "Ok");
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
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

                // O construtor já havia rodado, mas agora que recebemos o parâmetro via rota, 
                // reavaliamos as regras da tela para aplicar o Cenário 3 (Admin).
                DefinirModoDaTela();
            }
        }

        [RelayCommand]
        public async Task Cancelar()
        {
            if (UsuarioSessaoService.UsuarioLogado != null)
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