using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Univap.Programacao3.Engine.Resources
{
    public class FileResources
    {
        /// <summary>
        /// Set path location.
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>path</returns>
        static public string SolutionPath(string filePath)
        {
            string RootFolderPath = Directory.GetParent(Path.GetDirectoryName(Application.ExecutablePath.ToLower()
                                            .Replace("\\bin\\debug\\", "\\bin\\")
                                            .Replace("\\bin\\release\\", "\\bin\\"))).FullName + Path.DirectorySeparatorChar;
            return Path.Combine(RootFolderPath, filePath);
        }
    }
}
