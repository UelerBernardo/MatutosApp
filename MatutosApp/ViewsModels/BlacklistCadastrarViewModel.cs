using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(BlacklistAlteracao), "BlacklistAlteracao")]
    public partial class BlacklistCadastrarViewModel : BaseViewModel
    {
        private readonly BlacklistService _blacklistService;
        private readonly BarbeiroService _barbeiroService;
       
        [ObservableProperty] private Blacklist blacklistAlteracao;
        [ObservableProperty] private DateTime inicio_Bloqueio = DateTime.Now;
        [ObservableProperty] private DateTime fim_Bloqueio = DateTime.Now.AddHours(1);
        [ObservableProperty] private bool ativo = true;
        [ObservableProperty] private string detalhes;

      
        [ObservableProperty] private int? codigo_Agendamento;

        [ObservableProperty] private TimeSpan horaInicio_Bloqueio;
        [ObservableProperty] private TimeSpan horaFim_Bloqueio;

        [ObservableProperty]
        private AcaoTela acaoTela = AcaoTela.Cadastro;

        [ObservableProperty] private ObservableCollection<BarbeiroExibicao> listaBarbeiro = new();

        public BlacklistCadastrarViewModel(BlacklistService blacklistService, BarbeiroService barbeiroService)
        {
            _blacklistService = blacklistService;
            _barbeiroService = barbeiroService;

            _ = ConsultarBarbeiro();
        }

        partial void OnBlacklistAlteracaoChanged(Blacklist value)
        {
            if (value != null)
            {
                AcaoTela = AcaoTela.Alteração;
            }

            DefinirModoTela();
        }

        private void DefinirModoTela()
        {
            if (AcaoTela == AcaoTela.Cadastro)
            {
                Detalhes = string.Empty;
                Fim_Bloqueio = DateTime.Today;
                Inicio_Bloqueio = DateTime.Today;
                HoraFim_Bloqueio = TimeSpan.Zero;
                HoraInicio_Bloqueio = TimeSpan.Zero;
            }
            else
            {
                Detalhes = BlacklistAlteracao.Detalhes;
                Fim_Bloqueio = BlacklistAlteracao.Fim_Bloqueio.Date;
                Inicio_Bloqueio = BlacklistAlteracao.Inicio_Bloqueio.Date;

                // 👉 CORREÇÃO 2: Forma correta de extrair o horário de um DateTime no C#
                HoraInicio_Bloqueio = BlacklistAlteracao.Inicio_Bloqueio.TimeOfDay;
                HoraFim_Bloqueio = BlacklistAlteracao.Fim_Bloqueio.TimeOfDay;

                Codigo_Agendamento = BlacklistAlteracao.Codigo_Agendamento;
                Ativo = BlacklistAlteracao.Ativo;
            }
        }

        [RelayCommand]
        public async Task SalvarBlacklist()
        {
            // 👉 CORREÇÃO 5: Usar 'await' no lugar de '_ =' para garantir que o loading (IsBusy) e os alertas funcionem no tempo certo
            if (AcaoTela == AcaoTela.Cadastro)
            {
                await CadastrarBlacklist();
            }
            else
            {
                await AlterarBlacklist();
            }
        }

        public async Task AlterarBlacklist()
        {
            try
            {
                if (BlacklistAlteracao.Codigo_Agendamento != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Não é possível alterar bloqueios oriundos de agendamentos feitos por clientes", "Ok");
                    return;
                }

                var inicio = Inicio_Bloqueio.Date + HoraInicio_Bloqueio;
                var fim = Fim_Bloqueio.Date + HoraFim_Bloqueio;

                IsBusy = true;
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                var alteracaoBlacklist = new Blacklist
                {
                    // 👉 CORREÇÃO 3: Passando o ID para a API saber quem ela vai alterar!
                    Codigo_BlackList = BlacklistAlteracao.Codigo_BlackList,

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

                var resultado = await _blacklistService.AlterarBlacklist(alteracaoBlacklist, token);
                if (resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");
                    await VoltarTelaAsync(); // Adicionei o VoltarTela para sair da edição após o sucesso!
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao alterar bloqueio: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ConsultarBarbeiro()
        {
            try
            {
                var usuarioLogado = UsuarioSessaoService.UsuarioLogado;

                IsBusy = true;
                var resposta = await _barbeiroService.BarbeiroConsultar();

                if (resposta != null)
                {
                    ListaBarbeiro.Clear();

                    // 1. Tiramos o IF de dentro do foreach!
                    if (usuarioLogado.TipoSelecionado != UsuarioTipo.Administrador)
                    {
                        // Se não for admin, busca apenas ele mesmo e adiciona UMA única vez
                        var barbeiroLogado = resposta.FirstOrDefault(b => b.Codigo_Usuario == usuarioLogado.Codigo_Usuario);

                        if (barbeiroLogado != null)
                        {
                            ListaBarbeiro.Add(new BarbeiroExibicao
                            {
                                Codigo_Usuario = barbeiroLogado.Codigo_Usuario,
                                Nome = barbeiroLogado.Nome,
                                IsSelecionado = false // Lógica extra pode ser adicionada aqui se precisar
                            });
                        }
                    }
                    else
                    {
                        // Se FOR admin, aí sim fazemos o foreach para adicionar todos da barbearia
                        foreach (var barbeiros in resposta)
                        {
                            ListaBarbeiro.Add(new BarbeiroExibicao
                            {
                                Codigo_Usuario = barbeiros.Codigo_Usuario,
                                Nome = barbeiros.Nome,
                                IsSelecionado = false
                            });
                        }
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

        public async Task CadastrarBlacklist()
        {
            try
            {
                var inicio = Inicio_Bloqueio.Date + HoraInicio_Bloqueio;
                var fim = Fim_Bloqueio.Date + HoraFim_Bloqueio;

                IsBusy = true;
                string token = await SecureStorage.Default.GetAsync("jwt_token");

                if (string.IsNullOrWhiteSpace(Detalhes))
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha os detalhes para bloquear a agenda.", "Ok");
                    return;
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

            [ObservableProperty]
            private bool isSelecionado;
        }
    }
}