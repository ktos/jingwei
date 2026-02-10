import { AppServer, AppSession, ViewType } from '@mentra/sdk';


const PACKAGE_NAME = process.env.PACKAGE_NAME ?? (() => { throw new Error('PACKAGE_NAME is not set in .env file'); })();
const MENTRAOS_API_KEY = process.env.MENTRAOS_API_KEY ?? (() => { throw new Error('MENTRAOS_API_KEY is not set in .env file'); })();
const JINGWEI_MQTT = process.env.JINGWEI_MQTT ?? (() => { throw new Error('JINGWEI_MQTT is not set in .env file'); })();

const PORT = parseInt(process.env.PORT || '3000');
import mqtt, { MqttClient } from "mqtt";

const fs = require('fs')

const keyPath = process.env.JINGWEI_KEYFILE
const certPath = process.env.JINGWEI_CERTFILE
const caPath = process.env.JINGWEI_CACERTFILE

let KEY: Buffer | undefined
let CERT: Buffer | undefined
let TRUSTED_CA_LIST: Buffer | undefined

try {
  if (keyPath && fs.existsSync(keyPath)) {
    KEY = fs.readFileSync(keyPath)
  }
} catch (err) {
  KEY = undefined
}

try {
  if (certPath && fs.existsSync(certPath)) {
    CERT = fs.readFileSync(certPath)
  }
} catch (err) {
  CERT = undefined
}

try {
  if (caPath && fs.existsSync(caPath)) {
    TRUSTED_CA_LIST = fs.readFileSync(caPath)
  }
} catch (err) {
  TRUSTED_CA_LIST = undefined
}

class JingweiMentraOSApp extends AppServer {
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

    // Use TLS options only if at least one of the TLS files was loaded.
    if (KEY || CERT || TRUSTED_CA_LIST) {
      const opts: any = {
        rejectUnauthorized: true,
        clientId: "mentra_" + Math.random().toString(16).substring(2, 8)
      };
      if (KEY) opts.key = KEY;
      if (CERT) opts.cert = CERT;
      if (TRUSTED_CA_LIST) opts.ca = TRUSTED_CA_LIST;

      this.client = mqtt.connect(JINGWEI_MQTT, opts);
    } else {
      // No TLS files available — connect using only the URL.
      this.client = mqtt.connect(JINGWEI_MQTT);
    }

    this.client.on("connect", () => {
      this.logger.debug("Connected to MQTT!");
      session.layouts.showTextWall("Waiting for presentation to start...");
      this.client?.subscribe("powerpoint", (err) => {
        if (err) {
          this.logger.error(err, "error subscribing!");
        }
        this.logger.debug("Subscribed to topic!");
      });
    });

    this.client.on("message", (topic: any, message: Buffer) => {
      let msg = message.toString();

      // handling end presentation message
      if (msg == "|JINGWEI_END|") {
        session.layouts.clearView();
        return;
      }

      this.logger.info(msg);
      session.layouts.showTextWall(msg);
    });
  }

  protected async onStop(sessionId: string, userId: string, reason: string): Promise<void> {
    this.client?.end();
  }
}

const app = new JingweiMentraOSApp();

app.start().catch(console.error);