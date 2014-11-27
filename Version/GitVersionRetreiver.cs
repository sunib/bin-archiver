using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinArchiver.Version
{
    /// <summary>
    /// This class allows you to retreive the automatically generated git version at compile time.
    /// Note: You must do 2 things for this to work: 
    /// 1. Open a command window and type "git", git help should be sown. If not: add git bin to your PATH.
    /// 2. Add this line to pre-build event in your project settings: "git describe --dirty --tags > $(ProjectDir)/Version/pre-build-git-describe-output.txt"
    /// </summary>
    public class GitVersionRetreiver
    {
        /// <summary>
        /// Get the current version as reported by git describe.
        /// Make sure that you set the PreBuildGitDescribeOutput to binary, so that your VersionResource.Designer.cs is not beeing changed on every version change!
        /// TODO: Make something to convert "release-2.2-42-gd788e0e" into "release-2.2 build 42 (d788e0e)", as described in http://stackoverflow.com/a/6921307
        /// </summary>
        /// <returns>The current git version as a string.</returns>
        public static string getVersion()
        {
            return Encoding.UTF8.GetString(VersionResource.PreBuildGitDescribeOutput);
        }
    }
}
