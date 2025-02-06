using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Threading.Tasks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using System.Linq;

namespace Poi.Tools.Package
{
    public class PoiPackageHandler : EditorWindow
    {
        static List<PackageInfo> packageCache = new List<PackageInfo>();

        public static async Task<PackageInfo> GetPackageInfoAsync(string packageName, bool offlineMode, bool showProgressBar)
        {
            PackageInfo packageInfo = null;
            bool showingProgressBar = false;
            try
            {
                if(offlineMode)
                {
                    packageInfo = packageCache.FirstOrDefault(pkg => pkg.name == packageName);
                    if(packageInfo != null)
                        return packageInfo;
                }

                var listRequest = Client.List(offlineMode);

                if(showProgressBar)
                {
                    EditorUtility.DisplayProgressBar("Getting Package", $"Getting package info for {packageName}...", 0);
                    showingProgressBar = true;
                }

                while(!listRequest.IsCompleted)
                    await Task.Yield(); // Yield control back to Unity until the request is complete

                packageInfo = listRequest.Result.FirstOrDefault(pkg => pkg.name == packageName);

                if(packageInfo != null)
                    packageCache.Add(packageInfo);
            }
            finally
            {
                if(showingProgressBar)
                    EditorUtility.ClearProgressBar();
            }
            return packageInfo;
        }
    }
}