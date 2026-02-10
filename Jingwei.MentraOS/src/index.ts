import { AppServer, AppSession, ViewType } from '@mentra/sdk';


const PACKAGE_NAME = process.env.PACKAGE_NAME ?? (() => { throw new Error('PACKAGE_NAME is not set in .env file'); })();
const MENTRAOS_API_KEY = process.env.MENTRAOS_API_KEY ?? (() => { throw new Error('MENTRAOS_API_KEY is not set in .env file'); })();
const PORT = parseInt(process.env.PORT || '3000');
import mqtt, { MqttClient } from "mqtt";

class ExampleMentraOSApp extends AppServer {
  private client: MqttClient | undefined;

  constructor() {
    super({
      packageName: PACKAGE_NAME,
      apiKey: MENTRAOS_API_KEY,
      port: PORT,
    });
  }

  protected async onSession(session: AppSession, sessionId: string, userId: string): Promise<void> {
    // Show welcome message
    session.layouts.showTextWall("Wait for response...");

    // console.log("fetching data");
    // let response = await fetch("http://localhost:5247/test", { headers: { "X-API-KEY": "test" } });
    // let data = await response.json();
    // session.layouts.showTextWall(data.test);

    this.client = mqtt.connect("mqtt://mqtt.ktos.dev");
    this.client.on("connect", () => {
      this.client?.subscribe("+/+", (err) => {
        if (err) {
          this.logger.error(err, "error subscribing!");
        }
      });
    });

    this.client.on("message", (topic: any, message: Buffer) => {
      // this.logger.info(message.toString());
      // session.layouts.showTextWall(`${topic} ${message.toString()}`);
    });

    session.events.onTranscription((data) => {
      if (!data.isFinal) return;

      const command = data.text.toLowerCase();

      session.layouts.showTextWall(`Unknown command ${command}`);
    });

    //session.events.onButtonPress((btn) => { this.logger.info(btn, "button!"); session.layouts.showTextWall("t"); });

    // session.events.onConnected(async (settings) => {

    // });

    // Handle real-time transcription
    // requires microphone permission to be set in the developer console
    // session.events.onTranscription((data) => {
    //   if (data.isFinal) {
    //     session.layouts.showTextWall("You said: " + data.text, {
    //       view: ViewType.MAIN,
    //       durationMs: 3000
    //     });
    //   }
    // })

    // session.events.onGlassesBattery((data) => {
    //   console.log('Glasses battery:', data);
    // })
  }

  protected async onStop(sessionId: string, userId: string, reason: string): Promise<void> {
    this.client?.end();
  }
}

// Start the server
// DEV CONSOLE URL: https://console.mentra.glass/
// Get your webhook URL from ngrok (or whatever public URL you have)
const app = new ExampleMentraOSApp();

app.start().catch(console.error);