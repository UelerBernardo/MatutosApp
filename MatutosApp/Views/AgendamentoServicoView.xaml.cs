using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class AgendamentoServicoView : ContentPage
{
	public AgendamentoServicoView(AgendamentoServicoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}