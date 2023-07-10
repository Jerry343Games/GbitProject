// Copyright Elliot Bentine, 2018-
#if (UNITY_EDITOR) 

using System;
using UnityEditor;
using UnityEngine;

namespace ProPixelizer.Tools
{
    [CreateAssetMenu(fileName = "dither", menuName = "ProPixelizer/Dither Pattern")]
    public class DitherPattern : ScriptableObject
    {
        [SerializeField]
        public float m00;

        [SerializeField]
        public float m01;

        [SerializeField]
        public float m02;

        [SerializeField]
        public float m03;

        [SerializeField]
        public float m10;

        [SerializeField]
        public float m11;

        [SerializeField]
        public float m12;

        [SerializeField]
        public float m13;

        [SerializeField]
        public float m20;

        [SerializeField]
        public float m21;

        [SerializeField]
        public float m22;

        [SerializeField]
        public float m23;

        [SerializeField]
        public float m30;

        [SerializeField]
        public float m31;

        [SerializeField]
        public float m32;

        [SerializeField]
        public float m33;

        public int GetOrder(int offX, int offY)
        {
            return 4 * (offX % 4) + (offY % 4);
        }

        public bool PickColor(float fraction, int order)
        {
            switch (order)
            {
                case 0: return m00 > fraction;
                case 1: return m01 > fraction;
                case 2: return m02 > fraction;
                case 3: return m03 > fraction;
                case 4: return m10 > fraction;
                case 5: return m11 > fraction;
                case 6: return m12 > fraction;
                case 7: return m13 > fraction;
                case 8: return m20 > fraction;
                case 9: return m21 > fraction;
                case 10: return m22 > fraction;
                case 11: return m23 > fraction;
                case 12: return m30 > fraction;
                case 13: return m31 > fraction;
                case 14: return m32 > fraction;
                case 15: return m33 > fraction;
                default: throw new Exception("Unhandled order");
            }
        }
    }
}

 #endif