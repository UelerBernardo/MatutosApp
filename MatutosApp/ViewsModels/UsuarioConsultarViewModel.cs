using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using MatutosApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class UsuarioConsultarViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioServico;

        [ObservableProperty] private UsuarioTipo? usuarioSelecionado = UsuarioTipo.Cliente;
        [ObservableProperty] private string? nome;
        [ObservableProperty] private bool? ativo;
        [ObservableProperty] private ObservableCollection<Usuario> listaUsuarios = new();
        public ObservableCollection<UsuarioTipo> UsuarioTiposDisponiveis { get; set; }

        public List<string> OpcoesStatus { get; } = new List<string> { "Todos", "Ativos", "Inativos" };
        [ObservableProperty] private string statusSelecionado = "Todos";

        public UsuarioConsultarViewModel(UsuarioService usuarioServico)
        {
            UsuarioTiposDisponiveis = new ObservableCollection<UsuarioTipo>(Enum.GetValues(typeof(UsuarioTipo)).Cast<UsuarioTipo>());
            _usuarioServico = usuarioServico;
        }



        //[RelayCommand]
        //public async Task AbrirUsuarioCadastro()
        //{
        //    var parametros = new Dictionary<string, object>
        //            {
        //                { "CadastroDeUsuario", true } 
        //            };

        //    await Shell.Current.GoToAsync(nameof(UsuarioCadastroView), parametros);
        //}

        [RelayCommand]
        public async Task AbrirCadastroUsuario()
        {
            var parametros = new Dictionary<string, object>
            {
                {"CadastroDeUsuario", true}
            };

            await Shell.Current.GoToAsync(nameof(UsuarioCadastroView), parametros);
        }


        [RelayCommand]
        public async Task ConsultarUsuarios()
        {
            try
            {
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                if (UsuarioSelecionado == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "O campo Tipo de Usuário é obrigatório!", "Ok");
                    return;
                }
                if (StatusSelecionado == "Ativos")
                    Ativo = true;
                else if (StatusSelecionado == "Inativos")
                    Ativo = false;
                else
                    Ativo = null;


                var resultado = await _usuarioServico.ConsultarListaUsuario(token, UsuarioSelecionado.Value, Nome, Ativo);

                if (resultado.Sucesso)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Agora isso vai funcionar perfeitamente!
                        ListaUsuarios.Clear();

                        if (resultado.Dados != null)
                        {
                            foreach (var item in resultado.Dados)
                            {
                                ListaUsuarios.Add(item);
                            }
                        }
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(resultado.Mensagem) && resultado.Mensagem.Contains("Não foi encontrado"))
                    {
                        MainThread.BeginInvokeOnMainThread(() => ListaUsuarios.Clear());
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os usuários. Erro: {ex.Message}", "OK");
            }
        }
    }
}