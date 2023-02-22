using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManager.Tools
{
    internal static class Utils
    {
        private static readonly object indexLock = new();
        private static int index;

        internal static int GetNextIndex()
        {
            index++;
            return index;
            /*
            lock(indexLock)
            {
                index++;
                return index;
            }*/
        }
    }
}
