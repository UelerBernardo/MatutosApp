using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class AgendamentoViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;
        private readonly BarbeiroService _barbeiroService;
        private readonly ServicoService _servicoService;

        public ObservableCollection<DataAgendamento> ListaDatas { get; } = new();
        public ObservableCollection<HorarioAgendamento> ListaHorarios { get; } = new();
        public ObservableCollection<Barbeiro> ListaBarbeiro { get; set; } = new();
        public ObservableCollection<Servico> ListaServico { get; set; } = new();

        [ObservableProperty] private DataAgendamento? data_Selecionada;
        [ObservableProperty] private HorarioAgendamento hora_Selecionada;

        [ObservableProperty] private DateTime data_Fim_Agendamento;
        [ObservableProperty] private Barbeiro barbeiroSelecionado;
        [ObservableProperty] private Servico servicoSelecionado;

        public DateTime DataMinima => DateTime.Today;

        public AgendamentoViewModel(AgendamentoService agendamentoService, BarbeiroService barbeiroService)
        {
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;

            _ = ConsultarBarbeiro();
            CarregarDatasDisponiveis();
        }

        partial void OnBarbeiroSelecionadoChanged(Barbeiro value) => _ = CarregarHorariosAsync();
        partial void OnData_SelecionadaChanged(DataAgendamento value) => _ = CarregarHorariosAsync();

        private void CarregarDatasDisponiveis()
        {
            ListaDatas.Clear();
            DateTime dataAtual = DateTime.Today;

            for (int i = 0; i < 15; i++)
            {
                if (dataAtual.DayOfWeek != DayOfWeek.Sunday)
                {
                    ListaDatas.Add(new DataAgendamento { DataReal = dataAtual });
                }
                dataAtual = dataAtual.AddDays(1);
            }
        }

        public async Task CarregarHorariosAsync()
        {
            if (BarbeiroSelecionado == null || Data_Selecionada == null)
                return;

            try
            {
                IsBusy = true;
                ListaHorarios.Clear();

                string token = await SecureStorage.Default.GetAsync("jwt_token");

                // 👉 CORREÇÃO 2: Acessando a propriedade DataReal para enviar um DateTime para a API!
                var resultado = await _agendamentoService.ConsultarHorariosLivres(
                    token,
                    BarbeiroSelecionado.Codigo_Usuario,
                    Data_Selecionada.DataReal
                );

                if (resultado.Sucesso && resultado.Dados != null)
                {
                    foreach (var horarioTexto in resultado.Dados)
                    {
                        if (TimeSpan.TryParse(horarioTexto, out TimeSpan horaConvertida))
                        {
                            ListaHorarios.Add(new HorarioAgendamento
                            {
                                HoraReal = horaConvertida
                            });
                        }
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Aviso", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao carregar os horários: {ex.Message}", "Ok");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CadastrarAgendamento()
        {
            try
            {
                if (BarbeiroSelecionado == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor, selecione um profissional.", "OK");
                    return;
                }

                if (Data_Selecionada == null || Hora_Selecionada == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor, selecione a data e o horário desejados.", "OK");
                    return;
                }

                string token = await SecureStorage.Default.GetAsync("jwt_token");

                if (string.IsNullOrEmpty(token))
                {
                    await App.Current.MainPage.DisplayAlert("Atenção", "Sessão expirada. Por favor, faça login novamente.", "OK");
                    return;
                }

                DateTime dataCompleta = Data_Selecionada.DataReal.Date + Hora_Selecionada.HoraReal;

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
        public async Task Cadastrar() => await CadastrarAgendamento();

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

        public class DataAgendamento
        {
            public DateTime DataReal { get; set; }
            public string DiaSemana => DataReal.ToString("ddd", new System.Globalization.CultureInfo("pt-BR")).ToUpper();
            public string DiaMes => DataReal.ToString("dd");
        }

        public class HorarioAgendamento
        {
            public TimeSpan HoraReal { get; set; }
            public string HoraFormatada => HoraReal.ToString(@"hh\:mm");
        }
    }
}