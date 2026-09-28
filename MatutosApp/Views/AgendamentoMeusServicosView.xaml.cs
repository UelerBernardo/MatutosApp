using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class AgendamentoMeusServicosView : ContentPage
{
	public AgendamentoMeusServicosView(AgendamentoMeusServicosViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}