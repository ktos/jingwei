using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace Jingwei.PowerPointAddIn
{
    internal class Config
    {
        public string Server { get; set; }
        public string ClientId { get; set; }
        public bool UseMTls { get; set; }
        public bool IsDebug { get; set; }
    }

    public partial class ThisAddIn
    {
        private IMqttClient mqttClient;

        private string configFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "jingwei.json"
        );

        private Config config = null;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            if (File.Exists(configFile))
            {
                config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile));
                if (string.IsNullOrEmpty(config.ClientId))
                    config.ClientId = Environment.MachineName.ToLower();

                Application.SlideShowBegin += OnSlideShowBegin;
                Application.SlideShowNextSlide += OnNextSlide;
            }
            else
            {
                Debug.WriteLine("Configuration file not found, aborting.");
            }
        }

        private async void OnSlideShowBegin(PowerPoint.SlideShowWindow Wn)
        {
            await ConnectMqtt();

            // send the first slide info when the slideshow starts
            OnNextSlide(Wn);
        }

        private async Task ConnectMqtt()
        {
            var mqttFactory = new MqttFactory();

            mqttClient = mqttFactory.CreateMqttClient();

            if (config.UseMTls)
            {
                var clientCert = new X509Certificate2(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "jingwei.pfx"
                    )
                );

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(config.Server)
                    .WithClientId(config.ClientId)
                    .WithTlsOptions(options =>
                        options
                            .UseTls()
                            // ignore server certificate validation
                            // it should be possible to validate the server certificate with a custom CA, see:
                            // https://github.com/dotnet/MQTTnet/wiki/Client#using-a-custom-ca-with-tls
                            .WithIgnoreCertificateChainErrors()
                            .WithAllowUntrustedCertificates()
                            .WithClientCertificates(new List<X509Certificate2>() { clientCert })
                            .WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12)
                    )
                    .Build();

                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    if (config.IsDebug)
                        Debug.WriteLine("Connected to MQTT broker with mTLS.");
                }
                catch (MqttCommunicationException ex)
                {
                    Debug.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
                }
            }
            else
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(config.Server)
                    .WithClientId(config.ClientId)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            }
        }

        private async void OnNextSlide(PowerPoint.SlideShowWindow Wn)
        {
            PowerPoint.Slide slide = Wn.View.Slide;

            string message = GetCurrentSlideInfo(slide);
            bool success = await SendMqttMessageAsync(message);

            if (config.IsDebug)
            {
                if (success)
                {
                    Debug.WriteLine($"Sent MQTT message: {message}");
                }
                else
                {
                    Debug.WriteLine("Failed to send MQTT message.");
                }
            }
        }

        private async Task<bool> SendMqttMessageAsync(string message)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("powerpoint")
                .WithPayload(message)
                .Build();

            if (mqttClient.IsConnected)
            {
                var result = await mqttClient.PublishAsync(
                    applicationMessage,
                    CancellationToken.None
                );
                if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetCurrentSlideInfo(PowerPoint.Slide slide)
        {
            string notes = null;
            if (slide.HasNotesPage == MsoTriState.msoTrue)
            {
                notes = GetSlideNotes(slide);
            }

            var message = $"Slide #{slide.SlideIndex}";
            if (!string.IsNullOrEmpty(notes))
                message += $": {notes}";

            return message;
        }

        private string GetSlideNotes(PowerPoint.Slide slide)
        {
            PowerPoint.SlideRange notesPages = slide.NotesPage;
            foreach (PowerPoint.Shape shape in notesPages.Shapes)
            {
                if (
                    shape.Type == MsoShapeType.msoPlaceholder
                    && shape.PlaceholderFormat.Type
                        == PowerPoint.PpPlaceholderType.ppPlaceholderBody
                )
                {
                    return shape.TextFrame.TextRange.Text;
                }
            }

            return "";
        }

        private async void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            if (config != null)
            {
                await mqttClient.DisconnectAsync();

                Application.SlideShowBegin -= OnSlideShowBegin;
                Application.SlideShowNextSlide -= OnNextSlide;
            }
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion VSTO generated code
    }
}
