
using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class BlacklistConsultarView : ContentPage
{
	public BlacklistConsultarView(BlacklistConsultarViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

		if(BindingContext is BlacklistConsultarViewModel viewModel)
		{
			viewModel.ConsultarBlacklist();

        }
    }
}