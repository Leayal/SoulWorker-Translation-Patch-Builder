using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SoulWorker_Translation_Patch_Builder.Misc
{
    public static class AssemblyLoader
    {
        internal static Dictionary<string, Assembly> myDict;

        public static Assembly AssemblyResolve(object sender, ResolveEventArgs e)
        {
            if (myDict == null)
                myDict = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
            string RealName = e.Name.Split(',')[0].Trim();
            // System.Windows.MessageBox.Show(e.Name);
            if (myDict.ContainsKey(RealName))
                return myDict[RealName];
            else
            {
                byte[] bytes;
                string resourceName = "SoulWorker_Translation_Patch_Builder.Dlls." + RealName + ".dll";
                if (resourceName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                    return null;
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                using (Stream stream = currentAssembly.GetManifestResourceStream(resourceName))
                {
                    bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                }
                Assembly result = Assembly.Load(bytes);
                myDict.Add(RealName, result);
                bytes = null;
                return result;
            }
        }
    }
}
