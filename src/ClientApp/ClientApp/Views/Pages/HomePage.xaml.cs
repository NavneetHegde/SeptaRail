using SeptaRail.ClientApp.Services;
using SeptaRail.ClientApp.ViewModel;

namespace SeptaRail.ClientApp.Views.Pages;

public partial class HomePage : ContentPage
{
	public HomePage(INextTrainFunction nextTrainFunction)
	{
		InitializeComponent();
        BindingContext = new HomePageViewModel(nextTrainFunction);
    }
}