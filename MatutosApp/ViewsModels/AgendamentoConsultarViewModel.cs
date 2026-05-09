using MatutosApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class AgendamentoConsultarViewModel : BaseViewModel
    {
        private readonly AgendamentoService _agendamentoService;

        public AgendamentoConsultarViewModel(AgendamentoService service) 
        {

            _agendamentoService = service;

        }


    }
}
