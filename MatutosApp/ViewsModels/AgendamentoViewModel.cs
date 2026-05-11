using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class AgendamentoViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;
        private readonly BarbeiroService _barbeiroService;
        private readonly ServicoService _servicoService;

        [ObservableProperty] private DateTime data_Selecionada;
        [ObservableProperty] private TimeSpan hora_Selecionada;
        [ObservableProperty] private DateTime data_Fim_Agendamento;
        [ObservableProperty] private Barbeiro barbeiroSelecionado;
        [ObservableProperty] private Servico servicoSelecionado;

        public ObservableCollection<Barbeiro> ListaBarbeiro { get; set; } = new();
        public ObservableCollection<Servico> ListaServico { get; set; } = new();
        public DateTime DataMinima => DateTime.Today;

        public AgendamentoViewModel(AgendamentoService agendamentoService, BarbeiroService barbeiroService)
        {
            _agendamentoService = agendamentoService;
           _barbeiroService = barbeiroService;
            ListaBarbeiro = new ObservableCollection<Barbeiro>();

           _ = ConsultarBarbeiro();
        }

        public async Task CadastrarAgendamento()
        {
            try
            {
                // CORREÇÃO 3: Trava de segurança para obrigar a escolha do barbeiro
                if (BarbeiroSelecionado == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor, selecione um profissional.", "OK");
                    return;
                }

                string token = await SecureStorage.Default.GetAsync("jwt_token");

                if (string.IsNullOrEmpty(token))
                {
                    await App.Current.MainPage.DisplayAlert("Atenção", "Sessão expirada. Por favor, faça login novamente.", "OK");
                    return;
                }

                DateTime dataCompleta = Data_Selecionada.Date + Hora_Selecionada;

                var agendamentoNovo = new Agendamento
                {
                    Data_Agendamento = dataCompleta,
                    Data_Fim_Agendamento = null,
                    Codigo_Barbeiro = BarbeiroSelecionado.Codigo_Usuario,
                    Codigo_Situacao_Agendamento = AgendamentoSituacao.Aberto,
                    Ativo = true
                };

                var resultado = await _agendamentoService.AgendamentoCadastrar(agendamentoNovo, token);
                if (resultado.Sucesso)
                {
                    await Shell.Current.GoToAsync($"{nameof(AgendamentoServicoView)}?Agendamento={resultado.NovoId}");
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
        public async Task Cadastrar()
        {
            await CadastrarAgendamento();
        }

        [RelayCommand]
        public async Task ConsultarBarbeiro()
        {
            try
            {
                var listaBarbeiro = await _barbeiroService.BarbeiroConsultar();
                if (listaBarbeiro != null)
                {
                    ListaBarbeiro.Clear();
                    foreach (var barbeiros in listaBarbeiro)
                    {
                        ListaBarbeiro.Add(barbeiros);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Não foi possível carregar os barbeiros. Erro: {ex.Message}", "OK");
            }
        }
    }
}
