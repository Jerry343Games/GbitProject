using UnityEngine;
using System.Collections.Generic;

namespace ProPixelizer.Examples
{
    /// <summary>
    /// Intermittently randomises The ProPixelizer material this script is attached to.
    /// 
    /// Showcases the variety of styles possible with the shader.
    /// </summary>
    public class ProPixelizerMaterialRandomizer : MonoBehaviour
    {
        public List<Texture2D> PaletteLUTs;
        public List<Texture2D> LightRamps;
        public List<Color> OutlineColors;
        [Tooltip("Interval between material randomisation, seconds.")]
        public float Interval = 5f;
        private float _Timer;

        public ProPixelizerMaterialRandomizer()
        {
            PaletteLUTs = new List<Texture2D>();
            LightRamps = new List<Texture2D>();
        }

        void Start()
        {
            _Timer = Interval;
        }

        void Update()
        {
            _Timer -= Time.deltaTime;
            if (_Timer < 0f)
            {
                _Timer = Interval;
                Randomize();
            }    
        }

        public void Randomize()
        {
            var material = GetComponent<MeshRenderer>().material;
            var lut = PaletteLUTs[Random.Range(0, PaletteLUTs.Count)];
            var ramp = LightRamps[Random.Range(0, LightRamps.Count)];
            material.SetTexture("_PaletteLUT", lut);
            material.SetTexture("_LightingRamp", ramp);

            var pixelSize = Random.Range(2, 6);
            material.SetFloat("_PixelSize", pixelSize);

            var outerColor = OutlineColors[Random.Range(0, OutlineColors.Count)];
            var innerColor = OutlineColors[Random.Range(0, OutlineColors.Count)];
            material.SetColor("_OutlineColor", outerColor);
            material.SetColor("_EdgeHighlightColor", innerColor);
        }
    }
}
