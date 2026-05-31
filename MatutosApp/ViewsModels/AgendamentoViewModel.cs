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

        // 1. CORREÇÃO: Os tipos agora refletem exatamente o que está no CollectionView
        [ObservableProperty] private DataAgendamento data_Selecionada;
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
            CarregarDatasDisponiveis(); // Preenche os cartões de dias ao abrir a tela
        }

        // 2. A MÁGICA DA REATIVIDADE: Esses métodos rodam sozinhos quando a variável muda!
        partial void OnBarbeiroSelecionadoChanged(Barbeiro value) => CarregarHorarios();
        partial void OnData_SelecionadaChanged(DataAgendamento value) => CarregarHorarios();

        private void CarregarDatasDisponiveis()
        {
            ListaDatas.Clear();
            DateTime dataAtual = DateTime.Today;

            // Gera os próximos 15 dias para o usuário escolher
            for (int i = 0; i < 15; i++)
            {
                if (dataAtual.DayOfWeek != DayOfWeek.Sunday) // Pula domingo
                {
                    ListaDatas.Add(new DataAgendamento { DataReal = dataAtual });
                }
                dataAtual = dataAtual.AddDays(1);
            }
        }

        private void CarregarHorarios()
        {
            // Só carrega os horários se o Barbeiro E a Data já estiverem selecionados
            if (BarbeiroSelecionado == null || Data_Selecionada == null)
                return;

            ListaHorarios.Clear();

            // AQUI NO FUTURO: Você fará a chamada na API (_agendamentoService.ConsultarHorariosLivres...)
            // Por enquanto, geramos horários fixos para você testar o visual da tela
            TimeSpan horaInicial = new TimeSpan(9, 0, 0);
            TimeSpan horaFinal = new TimeSpan(19, 0, 0);

            while (horaInicial <= horaFinal)
            {
                ListaHorarios.Add(new HorarioAgendamento { HoraReal = horaInicial });
                horaInicial = horaInicial.Add(TimeSpan.FromMinutes(60));
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

                // 3. CORREÇÃO: Valida se ele clicou no cartão de data e de hora
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

                // 4. CORREÇÃO: Extraímos o DateTime/TimeSpan real de dentro dos objetos selecionados
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