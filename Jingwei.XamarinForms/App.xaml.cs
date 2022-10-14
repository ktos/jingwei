using Jingwei.XamarinForms.Models;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Xamarin.Forms;

namespace Jingwei.XamarinForms
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            string data = "";

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;

            using (Stream fs = assembly.GetManifestResourceStream("Jingwei.XamarinForms.config.json"))
            using (StreamReader sr = new StreamReader(fs))
            {
                data = sr.ReadToEnd();
            }

            var config = JsonSerializer.Deserialize<BrokerConfiguration>(data);
            MainPage = new MainPage(config);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}