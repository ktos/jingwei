using Jingwei.XamarinForms.Models;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
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

        private MqttMessage lastMessage;

        public MqttMessage LastMessage
        {
            get
            {
                return lastMessage;
            }

            private set
            {
                if (this.lastMessage != value)
                {
                    lastMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string broker;

        public string Broker
        {
            get
            {
                return broker;
            }

            private set
            {
                if (this.broker != value)
                {
                    broker = value;
                    OnPropertyChanged();
                }
            }
        }

        private string clientId;

        public string ClientId
        {
            get
            {
                return clientId;
            }

            private set
            {
                if (this.clientId != value)
                {
                    clientId = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<string> topics;

        public List<string> Topics
        {
            get
            {
                return topics;
            }

            private set
            {
                if (this.topics != value)
                {
                    topics = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                return client.IsConnected;
            }

            private set
            {
                OnPropertyChanged();
            }
        }

        public int VisibleRows => 5;

        public ICommand Connect { get; set; }

        public ICommand Disconnect { get; set; }

        public MainPageViewModel(BrokerConfiguration configuration)
        {
            Connect = new Command(InternalConnect);
            Disconnect = new Command(async _ => await client.DisconnectAsync());

            ClientId = configuration.ClientId;
            Broker = configuration.Host;
            Topics = configuration.Topics;

            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            IsConnected = false;
        }

        private async void InternalConnect()
        {
            try
            {
                var options = new MqttClientOptionsBuilder()
                    .WithClientId(ClientId)
                    .WithTcpServer(Broker)
                    .WithCleanSession()
                    .Build();

                client.ConnectedAsync += async (_) => IsConnected = client.IsConnected;
                client.DisconnectedAsync += async (_) => IsConnected = client.IsConnected;

                await client.ConnectAsync(options, CancellationToken.None);

                foreach (var item in Topics)
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

        private async Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
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