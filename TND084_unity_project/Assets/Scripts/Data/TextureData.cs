using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;


    public Layer[] layers;

    float savedMinHeight;
    float savedMaxHeight;
    public void ApplyToMaterial( Material mat)
    {
        mat.SetInt("layerCount", layers.Length);
        mat.SetColorArray("baseColors", layers.Select(x => x.tint).ToArray());
        mat.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        mat.SetFloatArray("baseBlends", layers.Select(x => x.blendStrenght).ToArray());
        mat.SetFloatArray("baseColorStrength", layers.Select(x => x.tintStrenght).ToArray());
        mat.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());

        mat.SetFloatArray("gradientValue", layers.Select(x => x.gradientValue).ToArray());

        Texture2DArray textureArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());

        mat.SetTexture("baseTextures", textureArray);

        
        UpdateMeshHeight(mat, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeight(Material mat, float minHeight, float maxHeight)
    {
        savedMaxHeight = maxHeight;
        savedMinHeight = minHeight;

        mat.SetFloat("minHeight", minHeight);
        mat.SetFloat("maxHeight", maxHeight);
    }

    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for(int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }

        textureArray.Apply();
        return textureArray;
    }

    [System.Serializable]

    public class Layer
    {
        public Texture2D texture;
        public Color tint;
        [Range(0,1)]
        public float tintStrenght;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrenght;
        public float textureScale;
        [Range(0, 1)]
        public float gradientValue;
    }
}
