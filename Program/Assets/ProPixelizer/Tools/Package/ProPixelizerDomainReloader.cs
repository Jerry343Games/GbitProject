#if UNITY_EDITOR
using UnityEditor;

namespace ProPixelizer
{
    /// <summary>
    /// This fixes an issue with Unity importing the shaders in the wrong order.
    /// 
    /// PixelizedWithOutline requires the forward rendering passes from ProPixelizerBase.
    /// Sometimes Unity imports them in the wrong order, and then ProPixelizer materials appear
    /// invisible.
    /// 
    /// The 'fix' is to reimport them in the correct order.
    /// </summary>
    public class ProPixelizerDomainReloader : AssetPostprocessor
    {
        #if UNITY_2021_2_OR_NEWER
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                ProPixelizerVerification.ReimportShaders();
            }
        }
        #endif
    }
}
#endif
