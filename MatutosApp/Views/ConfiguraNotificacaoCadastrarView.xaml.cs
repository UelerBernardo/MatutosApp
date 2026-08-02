using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class ConfiguraNotificacaoCadastrarView : ContentPage
{
	public ConfiguraNotificacaoCadastrarView(ConfiguraNotificacaoCadastrarViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}