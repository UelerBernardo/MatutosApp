using MatutosApp.Views;
using MatutosApp.ViewsModels;
using Microsoft.Maui.Controls;

namespace MatutosApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(UsuarioCadastroView), typeof(UsuarioCadastroView));
            Routing.RegisterRoute(nameof(TelefoneCadastroView), typeof(TelefoneCadastroView));
            //Routing.RegisterRoute(nameof(LoginView), typeof(LoginView));
            //Routing.RegisterRoute(nameof(PrincipalView), typeof(PrincipalView));
            Routing.RegisterRoute(nameof(AgendamentoCadastroView), typeof(AgendamentoCadastroView));
            Routing.RegisterRoute(nameof(AgendamentoServicoView), typeof(AgendamentoServicoView));
            Routing.RegisterRoute(nameof(AgendamentoDetalhesView), typeof(AgendamentoDetalhesView));
            Routing.RegisterRoute(nameof(AgendamentoConsultarView), typeof(AgendamentoConsultarView));
            Routing.RegisterRoute(nameof(ClientePerfilConsultarView), typeof(ClientePerfilConsultarView));
            Routing.RegisterRoute(nameof(UsuarioTelefoneConsultarView), typeof(UsuarioTelefoneConsultarView));
            Routing.RegisterRoute(nameof(UsuarioImagemView), typeof(UsuarioImagemView));
            Routing.RegisterRoute(nameof(ServicoConsultarView), typeof(ServicoConsultarView));
            Routing.RegisterRoute(nameof(ServicoCadastroView), typeof(ServicoCadastroView));
            Routing.RegisterRoute(nameof(BlacklistConsultarView), typeof(BlacklistConsultarView));
            Routing.RegisterRoute(nameof(BlacklistCadastrarView), typeof(BlacklistCadastrarView));
            Routing.RegisterRoute(nameof(UsuarioSolicitarCodigoView), typeof(UsuarioSolicitarCodigoView));
            Routing.RegisterRoute(nameof(UsuarioRedefinirSenhaView), typeof(UsuarioRedefinirSenhaView));
            Routing.RegisterRoute(nameof(ConfiguraNotificacaoCadastrarView), typeof(ConfiguraNotificacaoCadastrarView));
            Routing.RegisterRoute(nameof(ConfiguraNotificacaoConsultarView), typeof(ConfiguraNotificacaoConsultarView));

        }
    }
}
