using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BalintHelper.Utils
{
    public class MiscUtils
    {
        public static Type? GetTypeFromCurrentDomain(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(x => x != null);
        }
    }
}
