using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class SobreNosView : ContentPage
{
	public SobreNosView(SobreNosViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}