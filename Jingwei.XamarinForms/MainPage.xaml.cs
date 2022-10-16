using Jingwei.XamarinForms.Models;
using Jingwei.XamarinForms.ViewModels;
using Ktos.GoogleGlass;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Jingwei.XamarinForms
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel vm;

        public MainPage(BrokerConfiguration configuration)
        {
            InitializeComponent();

            vm = new MainPageViewModel(configuration);
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GlassDpadHandler.KeyUp += GlassDpadHandler_KeyUp;
            DeviceDisplay.KeepScreenOn = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            GlassDpadHandler.KeyUp -= GlassDpadHandler_KeyUp;
            DeviceDisplay.KeepScreenOn = false;
        }

        private void GlassDpadHandler_KeyUp(object sender, GlassDpadEventArgs e)
        {
            if (e.KeyCode == KeyCode.DpadCenter || e.KeyCode == KeyCode.Enter)
            {
                if (!vm.IsConnected)
                {
                    vm.Connect.Execute(null);
                }

                e.Handled = true;
            }
            else if (e.KeyCode == KeyCode.DpadDown || e.KeyCode == KeyCode.Back)
            {
                if (vm.IsConnected)
                {
                    vm.Disconnect.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}