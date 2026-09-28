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
    public partial class BlacklistConsultarViewModel : BaseViewModel
    {
        private readonly BlacklistService _blacklistService;

        [ObservableProperty] private DateTime dataInicial = DateTime.Now.Date.AddDays(-1);
        [ObservableProperty] private DateTime dataFinal = DateTime.Now.Date;

        [ObservableProperty]
        private ObservableCollection<Blacklist> listaBlacklist = new();


        public BlacklistConsultarViewModel(BlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
            _ = ConsultarBlacklist();
        }

        [RelayCommand]
        public async Task AbrirAlteracaoBlacklist(Blacklist blacklist)
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert("Atenação", "Deseja realizar alteração do registro selecionado?", "Sim", "Não");

            if(!confirmar)
            {
                return;
            }

            var query = new Dictionary<string, object>
            {
                { "BlacklistAlteracao", blacklist}
            };

            await Shell.Current.GoToAsync(nameof(BlacklistCadastrarView), query);
        }

        [RelayCommand]
        public async Task ConsultarBlacklist()
        {
            try
            {
               string token = await SecureStorage.Default.GetAsync("jwt_token");

                DateTime inicial = DataInicial;

                DateTime final = DataFinal.AddHours(23).AddMinutes(59);

               var resultado = await _blacklistService.ConsultarBlacklist(token, inicial, final);

                if(resultado.Sucesso)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ListaBlacklist.Clear();
                        foreach (var blacklist in resultado.Dados)
                        {

                            // Transforma o 'Servico' que veio da API no 'ServicoExibicao' que a tela entende!
                            ListaBlacklist.Add(new Blacklist
                            {
                                Codigo_BlackList = blacklist.Codigo_BlackList,
                                Inicio_Bloqueio = blacklist.Inicio_Bloqueio,
                                Fim_Bloqueio = blacklist.Fim_Bloqueio,
                                Ativo = blacklist.Ativo,
                                Detalhes = blacklist.Detalhes,
                                Codigo_Agendamento = blacklist.Codigo_Agendamento
                            });
                        }
                    });
                }

                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                }

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os serviços. Erro: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task AbrirBlacklistCadastro()
        {
            await Shell.Current.GoToAsync(nameof(BlacklistCadastrarView));
        }
    }
}
