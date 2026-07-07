using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class BlacklistCadastrarViewModel : BaseViewModel
    {
        private readonly BlacklistService _blacklistService;
        private readonly BarbeiroService _barbeiroService;

        [ObservableProperty] private DateTime inicio_Bloqueio = DateTime.Now;
        [ObservableProperty] private DateTime fim_Bloqueio = DateTime.Now.AddHours(1);
        [ObservableProperty] bool ativo = true;
        [ObservableProperty] string detalhes;
        [ObservableProperty] int codigo_Agendamento;
        [ObservableProperty] private TimeSpan horaInicio_Bloqueio;
        [ObservableProperty] private TimeSpan horaFim_Bloqueio;

        [ObservableProperty] private ObservableCollection<BarbeiroExibicao> listaBarbeiro = new();

        public BlacklistCadastrarViewModel(BlacklistService blacklistService, BarbeiroService barbeiroService)
        {
            _blacklistService = blacklistService;
            _barbeiroService = barbeiroService;

            _ = ConsultarBarbeiro();
        }

        [RelayCommand]
        public async Task ConsultarBarbeiro()
        {
            try
            {
                IsBusy = true;
                var resposta = await _barbeiroService.BarbeiroConsultar();

                if(resposta != null)
                {
                    ListaBarbeiro.Clear();
                    foreach(var barbeiros in resposta)
                    {
                        ListaBarbeiro.Add(new BarbeiroExibicao
                        {
                            Codigo_Usuario = barbeiros.Codigo_Usuario,
                            Nome = barbeiros.Nome,
                            IsSelecionado = false
                        });
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Não foi possível carregar a lista de barbeiros.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao carregar barbeiros: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task CadastrarBlacklist()
        {
            try
            {
                var inicio = Inicio_Bloqueio.Date + HoraInicio_Bloqueio;
                var fim = Fim_Bloqueio.Date + HoraFim_Bloqueio;

                IsBusy = true;
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                if(Detalhes == null)
                {
                    Detalhes = "Bloqueio sem motivo";
                }

                var novaBlacklist = new Blacklist
                {
                    Inicio_Bloqueio = inicio,
                    Fim_Bloqueio = fim,

                    Ativo = Ativo,
                    Detalhes = Detalhes,
                    Codigo_Agendamento = null,

                    UsuariosBloqueados = ListaBarbeiro
                        .Where(b => b.IsSelecionado)
                        .Select(ub => new Usuario_Blacklist
                        {
                            Codigo_Usuario = ub.Codigo_Usuario
                        }).ToList()
                };

                var resultado = await _blacklistService.CadastrarBlacklist(novaBlacklist, token);

                if (resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "OK");
                    await VoltarTelaAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                }

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "Ok");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public partial class BarbeiroExibicao : ObservableObject
        {
            public int Codigo_Usuario { get; set; }
            public string Nome { get; set; }

            // 👉 O CheckBox da tela vai alterar essa variável
            [ObservableProperty]
            private bool isSelecionado;
        }
    }
}
