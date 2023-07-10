// Copyright Elliot Bentine, 2022-
#if UNITY_EDITOR
using System.Collections.Generic;

namespace ProPixelizer.Tools.Migration
{
    public class ProPixelizer1_8MaterialUpdater : ProPixelizer1_7MaterialUpdater
    {
        public override List<IMigratedProperty> GetMigratedProperties()
        {
            var list = base.GetMigratedProperties();
            list.Add(new SynchroniseKeywordFloatProperty("PROPIXELIZER_DITHERING"));
            return list;
        }
    }
}
#endif