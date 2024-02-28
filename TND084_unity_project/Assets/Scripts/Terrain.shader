Shader"Custom/Terrain"
{
    Properties
    {
        testTexture("Texture", 2D) = "white"{}
        testScale("Scale", float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        // Defines the shader language, CG, a variant of high level shader language. Needs functions to be declared BEFORE it is called.
        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxLayerCount = 8;
        const static float epsilon = 1E-4;

        int layerCount;
        float3 baseColors[maxLayerCount];
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColorStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];

        float gradientValue[maxLayerCount];

        float minHeight;
        float maxHeight;

        sampler2D testTexture;
        float testScale;

        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float inverseLerp(float min, float max, float currentValue)
        {
            return saturate((currentValue - min) / (max - min)); // saturate clamps between 0 and 1
        }

        float3 triplanar(float3 worldPos, float scale, float3 blendAxis, int textureIndex)
        {
            // ensure RGB channels does not exceed 1 when the projections are added toghether
            float3 scaledWorldPos = worldPos / scale;
    
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxis.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxis.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxis.z;
    
            return xProjection + yProjection + zProjection;
        }

        // Called for every pixel that is visible in the mesh
        // Sets the color of the pixel using SurfaceOutputStandard by setting its Albedo property
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
    
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);

            float3 blendAxis = abs(IN.worldNormal);
            blendAxis /= blendAxis.x + blendAxis.y + blendAxis.z;
    
            for (int i = 0; i < layerCount; i++)
            {
                float heightFactor = inverseLerp(baseStartHeights[i], baseStartHeights[i] + baseBlends[i], IN.worldPos.y);
                float gradientDot = dot(gradientValue[i], IN.worldNormal);
    
            // Adjust the weights based on height and gradient
                float drawStrength = saturate(heightFactor * gradientDot);

                float3 baseColor = baseColors[i] * baseColorStrength[i];
                float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxis, i) * (1 - baseColorStrength[i]);

                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
            }

            /*// Height value   
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            
            float3 blendAxis = abs(IN.worldNormal);
            blendAxis /= blendAxis.x + blendAxis.y + blendAxis.z;
    
            for (int i = 0; i < layerCount; i++)
            {
                float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]); // clamped to [0,1]. 1 if heightPercent > baseStartHeight, 0 if else
                float3 baseColor = baseColors[i] * baseColorStrength[i];
                float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxis, i) * (1 - baseColorStrength[i]);
                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength; // if drawStrength is 0 we set it to its original color.
                
            }*/
    
            // Gradient
            /*float gradientPercent = inverseLerp(0, 1, IN.worldNormal.y);
            float3 blendAxis = abs(IN.worldNormal);
            blendAxis /= blendAxis.x + blendAxis.y + blendAxis.z;

            for (int i = 0; i < layerCount; i++)
            {
                // Assuming gradientValue[i] is a normalized vector pointing in the direction of the desired gradient
        float drawStrength = inverseLerp(0, 1, gradientValue[i] - gradientPercent); // clamped to [0,1]. 1 if heightPercent > baseStartHeight, 0 if else

                float3 baseColor = baseColors[i] * baseColorStrength[i];
                float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxis, i) * (1 - baseColorStrength[i]);
                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
            }*/
        
    
            
    
        }

        ENDCG
    }

    FallBack "Diffuse"
}

/*

// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
// #pragma instancing_options assumeuniformscaling
UNITY_INSTANCING_BUFFER_START(Props)
    // put more per-instance properties here
UNITY_INSTANCING_BUFFER_END(Props)

*/