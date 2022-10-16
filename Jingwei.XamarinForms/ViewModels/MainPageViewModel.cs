using Jingwei.XamarinForms.Models;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Jingwei.XamarinForms.ViewModels
{
    internal class MqttMessage
    {
        public string Topic { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }

    internal class MainPageViewModel : INotifyPropertyChanged
    {
        private IMqttClient client;
        private AutoResetEvent state;
        private Timer timer;
        private MqttMessage lastMessage;

        public MqttMessage LastMessage
        {
            get { return lastMessage; }
            private set
            {
                if (this.lastMessage != value)
                {
                    lastMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly BrokerConfiguration brokerConfiguration;

        public DateTime LocalTime =>
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.Now.ToUniversalTime(),
                TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw")
            );

        public bool IsConnected
        {
            get { return client.IsConnected; }
            private set { OnPropertyChanged(); }
        }

        public int VisibleRows => 5;

        public ICommand Connect { get; set; }

        public ICommand Disconnect { get; set; }

        public MainPageViewModel(BrokerConfiguration configuration)
        {
            Connect = new Command(InternalConnect);
            Disconnect = new Command(async _ => await client.DisconnectAsync());

            brokerConfiguration = configuration;

            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            state = new AutoResetEvent(false);
            timer = new Timer(
                _ => OnPropertyChanged(nameof(LocalTime)),
                state,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(30)
            );

            IsConnected = false;
        }

        private async void InternalConnect()
        {
            try
            {
                var options = new MqttClientOptionsBuilder()
                    .WithClientId(brokerConfiguration.ClientId)
                    .WithTcpServer(brokerConfiguration.Host)
                    .WithCleanSession()
                    .Build();

                client.ConnectedAsync += async (_) => IsConnected = client.IsConnected;
                client.DisconnectedAsync += async (_) => IsConnected = client.IsConnected;

                await client.ConnectAsync(options, CancellationToken.None);

                foreach (var item in brokerConfiguration.Topics)
                {
                    await client.SubscribeAsync(item);
                }
            }
            catch (MQTTnet.Exceptions.MqttCommunicationException e)
            {
                LastMessage = new MqttMessage { Message = "EXCEPTION: " + e.Message };
            }

            client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;
        }

        private async Task Client_ApplicationMessageReceivedAsync(
            MqttApplicationMessageReceivedEventArgs e
        )
        {
            LastMessage = new MqttMessage
            {
                Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload),
                Topic = e.ApplicationMessage.Topic,
                Time = DateTime.Now
            };
        }

        /// <summary>
        /// Even run when databound property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handles when property is changed raising <see cref="PropertyChanged"/>
        /// event.
        ///
        /// Part of <see cref="INotifyPropertyChanged"/> implementation.
        /// </summary>
        /// <param name="name">Name of a changed property</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}