using MatutosApp.ViewsModels;
namespace MatutosApp.Views;

public partial class UsuarioTelefoneConsultarView : ContentPage
{
	public UsuarioTelefoneConsultarView(UsuarioTelefoneConsultarViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}