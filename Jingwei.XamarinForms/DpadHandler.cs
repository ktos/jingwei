using System;

namespace Ktos.GoogleGlass
{
    public enum KeyCode
    {
        Back = 4,
        DpadUp = 19,
        DpadDown = 20,
        DpadLeft = 21,
        DpadRight = 22,
        DpadCenter = 23,
        Camera = 27,
        Enter = 66,
    }

    public class GlassDpadEventArgs
    {
        public KeyCode KeyCode { get; set; }
        public bool Handled { get; set; }
    }

    public static class GlassDpadHandler
    {
        public static event EventHandler<GlassDpadEventArgs> KeyUp;

        public static bool InternalOnKeyUp(int keyCode)
        {
            var gdea = new GlassDpadEventArgs() { KeyCode = (KeyCode)keyCode, Handled = false };
            KeyUp?.Invoke(null, gdea);

            return gdea.Handled;
        }
    }
}