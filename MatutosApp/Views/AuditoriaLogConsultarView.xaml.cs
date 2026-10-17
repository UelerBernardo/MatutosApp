using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class AuditoriaLogConsultarView : ContentPage
{
	public AuditoriaLogConsultarView(AuditoriaLogConsultarViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}