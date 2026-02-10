# Jingwei

![logo](https://raw.githubusercontent.com/ktos/jingwei/master/icon/jingwei_icon.svg)

Jingwei is a MQTT-based notification system, taking notes from your slides from
PowerPoint during your presentation via the add-in (Jingwei.PowerPointAddIn project),
publishing to the MQTT broker, where they are taken to client apps.

## Client Apps

### Google Glass

Jingwei.Glass, based on Jingwei.XamarinForms, which is a Xamarin.Forms client
for the original Google Glass. The application is listening to the topic and
then, when a new message is received, it shows their content on the device
screen.

Tap the DPad to connect to MQTT broker, slide down to disconnect. Slide down
again to return to Glass main screen.

### MentraOS

Jingwei.MentraOS is a [MentraOS](https://github.com/Mentra-Community/MentraOS) app,
working almost the same, except there is no configuration at all, the app is
connecting to the MQTT broker by a given configuration.
