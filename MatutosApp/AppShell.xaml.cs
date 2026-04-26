using MatutosApp.Views;
using MatutosApp.ViewsModels;

namespace MatutosApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(UsuarioCadastroView), typeof(UsuarioCadastroView));
            //Routing.RegisterRoute(nameof(pgLoginView), typeof(pgLoginView));
        }
    }
}
