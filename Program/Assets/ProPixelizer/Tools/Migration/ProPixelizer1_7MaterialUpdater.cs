// Copyright Elliot Bentine, 2022-
#if UNITY_EDITOR
using System.Collections.Generic;

namespace ProPixelizer.Tools.Migration
{
    public class ProPixelizer1_7MaterialUpdater : ProPixelizerMaterialUpdater
    {
        public override List<IMigratedProperty> GetMigratedProperties()
        {
            var list = new List<IMigratedProperty>();
            list.Add(new RenamedTexture { OldName = "Texture2D_FBC26130", NewName = "_Albedo" });
            list.Add(new RenamedTexture { OldName = "Texture2D_F406AA7C", NewName = "_LightingRamp" });
            list.Add(new RenamedTexture { OldName = "Texture2D_A4CD04C4", NewName = "_PaletteLUT" });
            list.Add(new RenamedTexture { OldName = "Texture2D_4084966E", NewName = "_NormalMap" });
            list.Add(new RenamedTexture { OldName = "Texture2D_9A2EA9A0", NewName = "_Emission" });
            list.Add(new RenamedColor { OldName = "Vector3_C98FB62A", NewName = "_AmbientLight" });
            list.Add(new SynchroniseKeywordFloatProperty("USE_OBJECT_POSITION"));
            list.Add(new SynchroniseKeywordFloatProperty("COLOR_GRADING"));
            return list;
        }
    }
}
#endif