#if UNITY_EDITOR
using UnityEditor.AssetImporters;
using UnityEngine;

namespace ProPixelizer
{
    /// <summary>
    /// I absolutely loath that I have to write this class.
    /// 
    /// HOWEVER, Unity sometimes imports the contents of ProPixelizer in a different order,
    /// and that can cause the import of the PixelizedWithOutline shader to fail - silently -
    /// so that it does not properly UsePass from the shadergraph shader. The fix is to right
    /// click and reimport the package a few times until it eventually gets things in the
    /// correct order.
    /// 
    /// There's no way to specify asset import order. As a workaround, I have made this importer
    /// for a custom type, '.ProPixelizer'. On import it simply triggers an import of the SG and
    /// PixelizedWithOutline shaders to guarantee they have been imported in the correct order.
    /// </summary>
    [ScriptedImporter(version: 1, ext: "ProPixelizer")]
    public class ProPixelizerPackageImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            ProPixelizerVerification.ReimportShaders();
            Debug.Log(string.Format("Added ProPixelizer to project."));
            ProPixelizerVerification.Check();
        }
    }
}
#endif