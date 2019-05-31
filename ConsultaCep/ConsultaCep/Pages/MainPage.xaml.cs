using ConsultaCep.ViewModels;
using Xamarin.Forms;

namespace ConsultaCep.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            this.BindingContext = new MainViewModel();
        }
    }
}
