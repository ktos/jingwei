using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Jingwei.XamarinForms;

namespace Jingwei.Droid
{
    [Activity(
        Label = "Jingwei",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize
            | ConfigChanges.Orientation
            | ConfigChanges.UiMode
            | ConfigChanges.ScreenLayout
            | ConfigChanges.SmallestScreenSize
    )]
    [MetaData("com.google.android.glass.VoiceTrigger", Resource = "@xml/voice_trigger")]
    [IntentFilter(
        actions: new[] { "android.intent.action.MAIN" },
        Categories = new[] { "android.intent.category.LAUNCHER" }
    )]
    [IntentFilter(actions: new[] { "com.google.android.glass.action.VOICE_TRIGGER" })]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            [GeneratedEnum] Android.Content.PM.Permission[] grantResults
        )
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(
                requestCode,
                permissions,
                grantResults
            );

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            var result = Ktos.GoogleGlass.GlassDpadHandler.InternalOnKeyUp((int)keyCode);
            if (!result)
                return base.OnKeyUp(keyCode, e);
            else
                return result;
        }
    }
}