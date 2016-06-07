using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Svchost.Update
{
    internal class Manifest
    {
        #region Fields
        private string _data;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Manifest"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Manifest()
        { 

        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; private set; }

        /// <summary>
        /// Gets the check interval.
        /// </summary>
        /// <value>The check interval.</value>
        public int CheckInterval { get; private set; }

        /// <summary>
        /// Gets the remote configuration URI.
        /// </summary>
        /// <value>The remote configuration URI.</value>
        public string RemoteConfigUri { get; private set; }

        /// <summary>
        /// Gets the security token.
        /// </summary>
        /// <value>The security token.</value>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <value>The base URI.</value>
        public string BaseUri { get; private set; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>The payload.</value>
        public string[] Payloads { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Load (string data)
        {
            _data = data;
            try
            {
                // Load config from XML
                data = data.Replace(((char)0xFEFF).ToString(), "");
                data = data.Replace(('\n').ToString(), "");
                var xml = XDocument.Parse(data);
                if (xml.Root.Name.LocalName != "Manifest")
                {
                    return;
                }

                // Set properties.
                Version = int.Parse(xml.Root.Attribute("version").Value);
                CheckInterval = int.Parse(xml.Root.Element("CheckInterval").Value);
                SecurityToken = xml.Root.Element("SecurityToken").Value;
                RemoteConfigUri = xml.Root.Element("RemoteConfigUri").Value;
                BaseUri = xml.Root.Element("BaseUri").Value;
                Payloads = xml.Root.Elements("Payload").Select(x => x.Value).ToArray();
            }
            catch (Exception )
            {
                return;
            }
        }

        public void LoadFromConfig()
        {
            Version = Properties.Settings.Default.Version;
            CheckInterval = Properties.Settings.Default.CheckInterval;
            SecurityToken = Properties.Settings.Default.SecurityToken;
            RemoteConfigUri = Properties.Settings.Default.RemoteConfigUri;
            BaseUri = Properties.Settings.Default.BaseUri;
            Payloads = new string[1];
            Payloads[0] = Properties.Settings.Default.Payload;
        }

        /// <summary>
        /// Writes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Write ()
        {
            Properties.Settings.Default.Version = Version;
            Properties.Settings.Default.CheckInterval =  CheckInterval;
            Properties.Settings.Default.SecurityToken = SecurityToken;
            Properties.Settings.Default.RemoteConfigUri = RemoteConfigUri;
            Properties.Settings.Default.BaseUri = BaseUri;
            Properties.Settings.Default.Payload = Payloads[0];
        }
        #endregion
    }
}
