using CommunityToolkit.Mvvm.ComponentModel;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class TelefoneViewModel : BaseViewModel
    {
        //Instanciar a classe service pois será usada para conectar com o banco de dados
        public readonly TelefoneService _telefoneService;

        //Definição dos campos que serão usudos para cadastro do Telefone
        [ObservableProperty] private string numero_telefone;
        [ObservableProperty] private string ddd;
        [ObservableProperty] private bool principal;


        public TelefoneViewModel(TelefoneService telefone)
        {
            _telefoneService = telefone;
        }

        private async Task CadastrarTelefone()
        {
            try
            {
                var telefoneNovo = new Telefone
                {
                    DDD = ddd,
                    Numero_Telefone = numero_telefone,
                    Principal = principal
                };

                bool sucesso = await _telefoneService.TelefoneCadastrar(telefoneNovo, );
                if (sucesso)
                {
                    bool confimar = await Shell.Current.DisplayAlert("Sucesso", "Telefone Cadastrado com sucesso! Deseja cadastrar um novo telefone?", "Sim", "Não");
                    if (!confimar)
                    {
                        return;
                    }
                }     
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "OK");
            }
        }

    }
}
