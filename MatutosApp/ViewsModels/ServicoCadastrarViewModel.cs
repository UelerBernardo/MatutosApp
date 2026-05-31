using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosDomain;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class ServicoCadastrarViewModel : BaseViewModel
    {
        ServicoService _servicoService;

        [ObservableProperty] private string descricao;
        [ObservableProperty] private int tempo_servico;
        [ObservableProperty] private bool ativo;
        [ObservableProperty] private decimal preco;
        [ObservableProperty] private string duracao;

        public ServicoCadastrarViewModel(ServicoService servicoService)
        {
            _servicoService = servicoService;
        }

        [RelayCommand]
        public async Task CadastrarServico()
        {
            if(Descricao.IsNullOrEmpty() || Preco <= 0 || Duracao.IsNullOrEmpty() || Tempo_servico <=0)
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos", "Ok");
            }

            try
            {
                var token = await SecureStorage.Default.GetAsync("jwt_token");

                var servicoNovo = new Servico
                {
                    Descricao = Descricao,
                    Ativo = Ativo,
                    Duracao = Duracao,
                    Preco = Preco,
                    Tempo_Servico = Tempo_servico
                };

                var resultado = await _servicoService.ServicoCadastrar(servicoNovo, token);

                if(resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");

                    await Shell.Current.GoToAsync("..");
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


    }
}
