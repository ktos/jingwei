import { AppServer, AppSession, ViewType } from '@mentra/sdk';


const PACKAGE_NAME = process.env.PACKAGE_NAME ?? (() => { throw new Error('PACKAGE_NAME is not set in .env file'); })();
const MENTRAOS_API_KEY = process.env.MENTRAOS_API_KEY ?? (() => { throw new Error('MENTRAOS_API_KEY is not set in .env file'); })();
const PORT = parseInt(process.env.PORT || '3000');
import mqtt, { MqttClient } from "mqtt";

const fs = require('fs')
const path = require('path')
const KEY = fs.readFileSync(process.env.JINGWEI_KEYFILE)
const CERT = fs.readFileSync(process.env.JINGWEI_CERTFILE)
const TRUSTED_CA_LIST = fs.readFileSync(process.env.JINGWEI_CACERTFILE)

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
    session.layouts.showTextWall("Establishing connection...");

    // mqtt.

    this.client = mqtt.connect({
      host: "192.168.8.204",
      port: 8883,
      rejectUnauthorized: false,
      key: KEY,
      cert: CERT,
      ca: TRUSTED_CA_LIST,
      protocol: 'mqtts',
      clientId: "mentra_" + Math.random().toString(16).substring(2, 8)
    });
    console.log(this.client);

    this.client.on("connect", () => {
      session.layouts.showTextWall("Waiting for presentation to start...");
      this.client?.subscribe("powerpoint", (err) => {
        if (err) {
          this.logger.error(err, "error subscribing!");
        }
      });
    });

    this.client.on("message", (topic: any, message: Buffer) => {
      this.logger.info(message.toString());
      session.layouts.showTextWall(`${message.toString()}`);
    });
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