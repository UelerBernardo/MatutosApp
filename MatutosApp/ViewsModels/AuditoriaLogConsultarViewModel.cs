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
    public partial class AuditoriaLogConsultarViewModel : BaseViewModel
    {
        private readonly AuditoriaLogService _auditoriaLogService;

        [ObservableProperty] private string? entidade;
        [ObservableProperty] private string? usuarioLog;

        // Propriedade que guarda a opção que o usuário selecionou no Picker
        [ObservableProperty] private AcaoFiltro acaoSelecionada;

        [ObservableProperty] private DateTime dataInicio = DateTime.Today;
        [ObservableProperty] private DateTime dataFim = DateTime.Today;
        [ObservableProperty] ObservableCollection<AuditoriaLog> listaLog;

        // Lista que vai aparecer na tela (no Picker)
        public ObservableCollection<AcaoFiltro> ListaAcoes { get; } = new();

        public AuditoriaLogConsultarViewModel(AuditoriaLogService auditoriaLogService)
        {
            _auditoriaLogService = auditoriaLogService;
            ListaLog = new ObservableCollection<AuditoriaLog>();

            CarregarOpcoesAcao();
        }

        private void CarregarOpcoesAcao()
        {
            ListaAcoes.Clear();

            // A primeira opção sempre é "Todas", enviando NULL para a API
            ListaAcoes.Add(new AcaoFiltro { Descricao = "Todas", Valor = null });

            // Adicionando as opções baseadas no seu Enum
            ListaAcoes.Add(new AcaoFiltro { Descricao = "Inserção", Valor = TipoAcaoEnum.Inserção });
            ListaAcoes.Add(new AcaoFiltro { Descricao = "Alteração", Valor = TipoAcaoEnum.Alteração });
            ListaAcoes.Add(new AcaoFiltro { Descricao = "Exclusão", Valor = TipoAcaoEnum.Exclusão });

            // Define "Todas" como o valor pré-selecionado ao abrir a tela
            AcaoSelecionada = ListaAcoes.First();
        }

        [RelayCommand]
        public async Task AbrirTelaDetalhes(AuditoriaLog log)
        {
            if(log == null) 
                return;
            var parametros = new Dictionary<string, object>
            {
                {"Log", log.Codigo_Log  }
            };

            await Shell.Current.GoToAsync(nameof(AuditoriaLogDetalhesView), parametros);
        }

        [RelayCommand]
        public async Task ConsultarLog()
        {
            try
            {
                string? token = await SecureStorage.Default.GetAsync("jwt_token");

                // Joga a data final para o final do dia (23:59) para garantir que pegará os logs do dia inteiro
                DateTime fim = DataFim.Date.AddHours(23).AddMinutes(59);

                var parametros = new FiltroLogDTO
                {
                    Entidade = Entidade,
                    UsuarioLog = UsuarioLog,

                    // Extrai apenas o Enum (ou o null) da opção escolhida na tela
                    Acao = AcaoSelecionada?.Valor,

                    Inicio = DataInicio,
                    Fim = fim,
                };

                var resultado = await _auditoriaLogService.LogConsultarLista(token, parametros);

                if (resultado.Sucesso && resultado.Dados != null)
                {
                    ListaLog.Clear();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var log in resultado.Dados)
                        {
                            ListaLog.Add(log);
                        };
                    });
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao carregar os logs: {ex.Message}", "Ok");
            }
        }
    }

    // Classe auxiliar para construir a lista suspensa (Picker) mesclando Texto e o Enum
    public class AcaoFiltro
    {
        public string Descricao { get; set; }

        // Aqui usamos o TipoAcaoEnum original da sua classe MatutosDomain
        public TipoAcaoEnum? Valor { get; set; }

        // O MAUI chama o ToString() automaticamente para mostrar o texto pro usuário na tela
        public override string ToString()
        {
            return Descricao;
        }
    }
}