Shader "Custom/ShaderForInstancing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "White" {}
        
        _SmoothnessScale("Smoothness", Range(0.0, 1.0)) = 0.5

        _GlossinessScale("Glossiness scale", Range(0.0, 1.0)) = 0.0
        
        _MetallicScale("Metallic scale", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _BumpScale("Normal Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax("Height Map Scale", Range(0.005, 0.08)) = 0.005
        _ParallaxMap("Height Map", 2D) = "black" {}

        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        
        _PosTex ("Position Textures", 2D) = "black" {}
        _NmlTex ("Normal Textures", 2D) = "white" {}

        _osTexArray16 ("16 Position Textures", 2DArray) = "black" {}
        _NmlTexArray16 ("16 Normal Textures", 2DArray) = "white" {}
        
        _PosTexArray32 ("32 Position Textures", 2DArray) = "black" {}
        _NmlTexArray32 ("32 Normal Textures", 2DArray) = "white" {}
        
        _PosTexArray64 ("64 Position Textures", 2DArray) = "black" {}
        _NmlTexArray64 ("64 Normal Textures", 2DArray) = "white" {}
        
        _PosTexArray128 ("128 Position Textures", 2DArray) = "black" {}
        _NmlTexArray128 ("128 Normal Textures", 2DArray) = "white" {}
        
        _Length ("Animator Length", float) = 1
        
        _DT ("Delta time", float) = 0
    }
    
    SubShader
    {
        Tags{"RenderType" = "Opaque"}
        LOD 300
        
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "HLSLSupport.cginc"
            #define ts _PosTex_TexelSize
            #pragma require 2darray
            
            struct v2f
            {
                float2 uv: TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 worldPosition : TEXCOORD2;
            };
            
            sampler2D _MetallicGlossMap, _BumpMap, _ParallaxMap, _OcclusionMap;
            sampler2D _MainTex, _PosTex, _NmlTex;
            float4 _PosTex_TexelSize;
            float _Length, _DT;
            float _MetallicScale, _BumpScale, _ParallaxScale, _OcclusionStrength, _SmoothnessScale, _GlossinessScale;

            StructuredBuffer<float3> _Positions;
            StructuredBuffer<int> _Index;
            StructuredBuffer<int>_WhichTextureArray;
            StructuredBuffer<float4> _Rotations;

            UNITY_DECLARE_TEX2DARRAY(_PosTexArray16);
            UNITY_DECLARE_TEX2DARRAY(_NmlTexArray16);
            UNITY_DECLARE_TEX2DARRAY(_PosTexArray32);
            UNITY_DECLARE_TEX2DARRAY(_NmlTexArray32);
            UNITY_DECLARE_TEX2DARRAY(_PosTexArray64);
            UNITY_DECLARE_TEX2DARRAY(_NmlTexArray64);
            UNITY_DECLARE_TEX2DARRAY(_PosTexArray128);
            UNITY_DECLARE_TEX2DARRAY(_NmlTexArray128);
           
            float3x3 QuaternionToRotationMatrix(float4 q)
            {
                q.xyz = -q.xyz;
                float x2 = q.x * q.x;
                float y2 = q.y * q.y;
                float z2 = q.z * q.z;
                float xy = q.x * q.y;
                float xz = q.x * q.z;
                float yz = q.y * q.z;
                float wx = q.w * q.x;
                float wy = q.w * q.y;
                float wz = q.w * q.z;

                float3x3 rotationMatrix;
                rotationMatrix[0] = float3(1 - 2 * (y2 + z2), 2 * (xy - wz), 2 * (xz + wy));
                rotationMatrix[1] = float3(2 * (xy + wz), 1 - 2 * (x2 + z2), 2 * (yz - wx));
                rotationMatrix[2] = float3(2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (x2 + y2));

                return rotationMatrix;
            }
            float3 RotateX(float3 vertex, float3 angle)
            {
                float alpha = angle * UNITY_PI / 180;
                float sina, cosa;
                sincos(alpha,sina,cosa);
                float2x2 m = float2x2(cosa,-sina, sina, cosa);
                return float3(mul(m,vertex.yz), vertex.x).zxy;
            }
            
            v2f vert (appdata_full v, uint vid: SV_VertexID, uint instanceId : SV_InstanceID)
            {
                float3 worldPosition = _Positions[instanceId];
                float3 localPosition = v.vertex;
                float4 rotation = _Rotations[instanceId];

                float t = (_Time.y - _DT) / _Length;
                t = fmod(t, 1.0);

                float x = (vid + 0.5) * ts.x ;
                float y = t;

                float4 posTexSample = 0;
                float4 nmlTexSample = 0;
                
                switch (_WhichTextureArray[instanceId])
                {
                    case 16:
                        posTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_PosTexArray16, float3(x, y , _Index[instanceId]), -1);
                        nmlTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_NmlTexArray16, float3(x ,y , _Index[instanceId]), -1);
                        break;
                    case 32:
                        posTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_PosTexArray32, float3(x, y , _Index[instanceId]), -1);
                        nmlTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_NmlTexArray32, float3(x ,y , _Index[instanceId]), -1);
                        break;
                    case 64:
                        posTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_PosTexArray64, float3(x, y , _Index[instanceId]), -1);
                        nmlTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_NmlTexArray64, float3(x ,y , _Index[instanceId]), -1);
                        break;
                    case 128:
                        posTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_PosTexArray128, float3(x, y , _Index[instanceId]), -1);
                        nmlTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_NmlTexArray128, float3(x ,y , _Index[instanceId]), -1);
                        break;
                    default:
                        posTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_PosTexArray64, float3(x, y , 0), -1);
                        nmlTexSample = UNITY_SAMPLE_TEX2DARRAY_LOD(_NmlTexArray64, float3(x ,y , 0), -1);
                        break;
                }
                
                localPosition += posTexSample.xyz;
                
                float3x3 rotationMatrix = QuaternionToRotationMatrix(rotation);
                localPosition = mul(localPosition, rotationMatrix);

                worldPosition += localPosition;

                float normalPosition = worldPosition;
            
                normalPosition += nmlTexSample.xyz;
                
                v2f o;
                o.worldPosition = worldPosition;
                o.vertex = UnityObjectToClipPos(worldPosition); 
                o.normal = UnityObjectToWorldNormal(normalPosition);
                o.uv = v.texcoord;
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample textures and fetch properties
                const fixed4 albedo = tex2D(_MainTex, i.uv);
                const float smoothness = _SmoothnessScale;
                const float metallicScale = _MetallicScale;
                const float bumpScale = _BumpScale;
                const float parallaxScale = _ParallaxScale;
                const float occlusionStrength = _OcclusionStrength;

                // Sample metallic and occlusion maps
                fixed4 metallicMap = tex2D(_MetallicGlossMap, i.uv);
                fixed4 occlusionMap = tex2D(_OcclusionMap, i.uv);

                // Sample normal map and apply bump mapping
                float3 worldNormal = normalize(UnityObjectToWorldNormal(i.normal));
                float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
                normal = normalize(normal * bumpScale);

                // Sample height map and apply parallax mapping
                float parallaxMap = tex2D(_ParallaxMap, i.uv).r;
                i.vertex.xyz += normal * parallaxMap * parallaxScale;

                // Calculate lighting
                // fixed3 worldPosition = i.worldPosition.xyz;
                // fixed3 viewDirection = normalize(UnityWorldSpaceViewDir(worldPosition));
                // fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz - worldPosition);
                // fixed3 reflectionDirection = normalize(reflect(-lightDirection, worldNormal));
                // fixed3 diffuse = unity_LightColor[3].rgb * max(0, dot(worldNormal, lightDirection));
                // fixed3 specular = unity_LightColor[3].rgb * pow(max(0, dot(reflectionDirection, viewDirection)), _GlossinessScale);

                // Apply occlusion
                fixed4 finalColor = albedo * lerp(1.0, occlusionMap.r, occlusionStrength);

                // Apply metallic
                finalColor.rgb = lerp(finalColor.rgb * (1.0 - metallicScale), metallicMap.rgb, metallicScale);

                // Apply smoothness
                finalColor.a *= smoothness;

                // Apply lighting
                //finalColor.rgb *= diffuse * albedo + specular;

                return finalColor;
            }

            ENDCG
        }
        
    }
    FallBack "Diffuse"
}