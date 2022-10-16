using Microsoft.Office.Core;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace Jingwei.PowerPointAddIn
{
    internal class Config
    {
        public string Server { get; set; }
        public string ClientId { get; set; }
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
            var mqttFactory = new MqttFactory();

            mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(config.Server)
                .WithClientId(config.ClientId)
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        }

        private async void OnNextSlide(PowerPoint.SlideShowWindow Wn)
        {
            PowerPoint.Slide slide = Wn.View.Slide;
            if (slide.HasNotesPage == MsoTriState.msoTrue)
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
                        var notes = $"Slide {slide.SlideIndex}: {shape.TextFrame.TextRange.Text}";
                        Debug.WriteLine(notes);

                        var applicationMessage = new MqttApplicationMessageBuilder()
                            .WithTopic("powerpoint")
                            .WithPayload(notes)
                            .Build();

                        if (mqttClient.IsConnected)
                            await mqttClient.PublishAsync(
                                applicationMessage,
                                CancellationToken.None
                            );
                    }
                }
            }
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