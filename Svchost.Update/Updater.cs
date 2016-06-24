using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Svchost.Net;
using System.Reflection;
using System.Windows.Forms;

namespace Svchost.Update
{
    public class Updater
    {
        #region Constants
        /// <summary>
        /// The default check interval
        /// </summary>
        public const int DefaultCheckInterval = 900; // 900s == 15 min
        public const int FirstCheckDelay = 15;
                
        #endregion

        #region Fields
        private System.Threading.Timer _timer;
        private volatile bool _updating;
        private readonly Manifest _localConfig;
        private Manifest _remoteConfig;
        private string sessionName;
        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        public Updater ()
        {
            this._localConfig = new Manifest();
            this._localConfig.LoadFromConfig();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the monitoring.
        /// </summary>
        public void StartMonitoring (string name)
        {
            _timer = new System.Threading.Timer(Check, null, 5000, this._localConfig.CheckInterval * 1000);
            sessionName = name;
        }

        /// <summary>
        /// Stops the monitoring.
        /// </summary>
        public void StopMonitoring ()
        {
            if (_timer == null)
            {
                return;
            }
            _timer.Dispose();
        }

        /// <summary>
        /// Checks the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void Check (object state)
        {
            string bak = Process.GetCurrentProcess().MainModule.FileName + ".bak";
            if (File.Exists(bak))
                File.Delete(bak);

            if (_updating)
            {
                return;
            }
            var remoteUri = new Uri(this._localConfig.RemoteConfigUri);
            
            try
            {
                var http = new Fetch { Retries = 5, RetrySleep = 30000, Timeout = 30000 };
                http.Load(remoteUri.AbsoluteUri);
                if (!http.Success)
                {
                    this._remoteConfig = null;
                    return;
                }

                string data = Encoding.UTF8.GetString(http.ResponseData);
                this._remoteConfig = new Manifest();
                this._remoteConfig.Load(data);

                if (this._remoteConfig == null)
                    return;

                if (this._localConfig.SecurityToken != this._remoteConfig.SecurityToken)
                {
                    return;
                };

                if (this._remoteConfig.Version == this._localConfig.Version)
                {
                    return;
                }
                if (this._remoteConfig.Version < this._localConfig.Version)
                {
                    return;
                }

                _updating = true;
                Update();
                _updating = false;
            }
            catch { }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private void Update ()
        {
            WebUtil.SendLog(sessionName, "################ SYSTEM : Updating software");
            // Download files in manifest.
            foreach (string update in this._remoteConfig.Payloads)
            {
                try
                {
                    var url = this._remoteConfig.BaseUri + update;
                    var file = Fetch.Get(url);
                    if (file == null)
                    {
                        return;
                    }

                    File.WriteAllBytes(update, file);
                }
                catch { }
            }

            // Change the currently running executable so it can be overwritten.
            Process thisprocess = Process.GetCurrentProcess();
            string me = thisprocess.MainModule.FileName;
            string bak = me + ".bak";
            if (File.Exists(bak))
                File.Delete(bak);
            File.Move(me, bak);

            FileInfo newfile = new FileInfo(_remoteConfig.Payloads[0]);
            string newFileName = newfile.FullName;
            File.Move(newFileName, me);

            // Write out the new manifest.
            _remoteConfig.Write();

            // Restart.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.ErrorDialog = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = "NVIDIA Updater Service";
            startInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
            System.Diagnostics.Process.Start(startInfo);
            
            thisprocess.CloseMainWindow();
            thisprocess.Close();
            thisprocess.Dispose();
            Application.Exit();
            Environment.Exit(0);
        }
        #endregion
    }
}
