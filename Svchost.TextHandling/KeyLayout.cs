using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svchost.TextHandling
{
    class KeyLayout
    {
        public string Key { get; }
        public string ShiftKey { get; }
        public string AltftKey { get; }

        public KeyLayout(string key, string shiftKey = "none", string altKey = "none")
        {
            Key = key;

            if (shiftKey == "none")
                ShiftKey = Key;
            else
                ShiftKey = shiftKey;

            if (altKey == "none")
                AltftKey = Key;
            else
                AltftKey = altKey;
        }
    }
}
