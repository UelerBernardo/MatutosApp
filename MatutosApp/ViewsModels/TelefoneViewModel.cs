using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Maui.Controls.Platform.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(TelefoneEditar), "TelefoneParaEditar")]
    public partial class TelefoneViewModel : BaseViewModel
    {
        private readonly TelefoneService _telefoneService;

        [ObservableProperty] private Telefone? telefoneEditar;

        // O Toolkit vai gerar: Numero_telefone, Ddd e Principal
        [ObservableProperty] private string numero_telefone;
        [ObservableProperty] private string ddd;
        [ObservableProperty] private bool principal;
        [ObservableProperty]
        private bool podeEditarPrincipal = true;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TituloPagina))]
        [NotifyPropertyChangedFor(nameof(NomeBotaoAcao))]
        private AcaoTela _acaoTela;

        //propriedade para receber se é alteração ou cadastro para mudar o nome da tela
        public string TituloPagina => AcaoTela == AcaoTela.Cadastro ? "Novo Cliente" : "Editar Perfil";
        public string NomeBotaoAcao => AcaoTela == AcaoTela.Cadastro ? "Cadastrar" : "Alterar";
        

        public TelefoneViewModel(TelefoneService telefone)
        {
            _telefoneService = telefone;
        }

        private void ModoCadastro()
        {
            AcaoTela = AcaoTela.Cadastro;
            Numero_telefone = string.Empty;
            Ddd = string.Empty;
            Principal = false;
            PodeEditarPrincipal = true;
        }


        partial void OnTelefoneEditarChanged(Telefone? value)
        {
            if(value != null)
            {
                AcaoTela = AcaoTela.Alteração;

                Numero_telefone = value.Numero_Telefone;
                Ddd = value.DDD;
                Principal = value.Principal;
                PodeEditarPrincipal = false;
            }
            else
            {
                ModoCadastro();
            }
        }

        private async Task AlterarTelefone()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");
                if (string.IsNullOrEmpty(token))
                {
                    await App.Current.MainPage.DisplayAlert("Atenção", "Sessão expirada. Por favor, faça login novamente.", "OK");
                    return;
                }

                var telefoneAlterar = new Telefone
                {
                    Codigo_Telefone = TelefoneEditar.Codigo_Telefone,
                    DDD = Ddd,
                    Numero_Telefone = Numero_telefone,
                    Principal = Principal,
                };

                var resultado = await _telefoneService.TelefoneAlterar(telefoneAlterar, token);

                if(resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");

                    await Shell.Current.GoToAsync(nameof(UsuarioTelefoneConsultarView));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "OK");
            }
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
        public async Task CadastrarOuAlterar() 
        {
            if (string.IsNullOrEmpty(Numero_telefone) || string.IsNullOrEmpty(Ddd))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos.", "OK");
                return;
            }

            if(AcaoTela == AcaoTela.Alteração)
            {
                AlterarTelefone();
            }
            else
            { 
                await CadastrarTelefone();
            }
        }

        public void LimparDados()
        {
            Numero_telefone = string.Empty;
            Ddd = string.Empty;
            Principal = false;
        }
    }
}
