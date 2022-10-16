using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jingwei.XamarinForms.Models
{
    public class BrokerConfiguration : INotifyPropertyChanged
    {
        private string mqttBroker;

        public string Host
        {
            get { return mqttBroker; }
            set
            {
                if (mqttBroker != value)
                {
                    mqttBroker = value;
                    OnPropertyChanged();
                }
            }
        }

        private string clientId;

        public string ClientId
        {
            get { return clientId; }
            set
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
            get { return topics; }
            set
            {
                if (this.topics != value)
                {
                    topics = value;
                    OnPropertyChanged();
                }
            }
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}