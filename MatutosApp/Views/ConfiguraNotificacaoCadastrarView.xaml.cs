using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class ConfiguraNotificacaoCadastrarView : ContentPage
{
	public ConfiguraNotificacaoCadastrarView(ConfiguraNotificacaoConsultarViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}