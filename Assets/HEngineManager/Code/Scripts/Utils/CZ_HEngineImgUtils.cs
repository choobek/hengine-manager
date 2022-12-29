using UnityEditor;
using UnityEngine;

namespace CZ.HEngineTools
{
    public static class CZ_HEngineImgUtils
    {
        public static Texture2D MakeBgTexture(Color aColor)
        {
            Texture2D bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, aColor);
            bgTexture.Apply();
            return bgTexture;
        }

        public static Texture FetchIcon(string path)
        {
            Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (loadedTexture) { return loadedTexture; }
            return null;
        }
    }
}