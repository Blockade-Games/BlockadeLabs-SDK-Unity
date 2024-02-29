Shader "BlockadeLabsSDK/BlockadeSkyboxDepth"
{
    Properties
    {
        _MainTex ("Texture", Cube) = "white" {}
        _DepthMap ("Depth Map", 2D ) = "white" {}
        _DepthScale ("Depth Scale", Range(3, 10)) = 5.3
    }

    // Universal Render Pipeline
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest+51"
        }

        LOD 100

        Pass
        {
            PackageRequirements
            {
                "com.unity.render-pipelines.universal": "10.2.1"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURECUBE(_MainTex);
            TEXTURE2D(_DepthMap);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_DepthMap);
            float _DepthScale;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float depth = SAMPLE_TEXTURE2D_LOD(_DepthMap, sampler_DepthMap, IN.uv, 0).g;
                depth = clamp(1.0 / depth + 10 / _DepthScale, 0, _DepthScale);

                IN.positionOS.xyz = normalize(IN.positionOS.xyz) * depth;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Unity generates cubemaps with -90 deg rotation for some reason
                float3 rotated = float3(IN.positionOS.z, IN.positionOS.y, -IN.positionOS.x);
                OUT.viewDir = normalize(rotated);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURECUBE(_MainTex, sampler_MainTex, IN.viewDir);
                return col;
            }
            ENDHLSL
        }
    }

    // Built-in render pipeline (and works in HDRP apparently)
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = ""
        }

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float3 viewDir : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            samplerCUBE _MainTex;
            sampler2D _DepthMap;
            float _DepthScale;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 uvLOD = float4(v.uv, 0, 0);
                float depth = tex2Dlod(_DepthMap, uvLOD).g;

                depth = clamp(1.0 / depth + 10 / _DepthScale, 0, _DepthScale);
                v.vertex.xyz = normalize(v.vertex.xyz) * depth;

                o.vertex = UnityObjectToClipPos(v.vertex.xyz);

                // Unity generates cubemaps with -90 deg rotation for some reason
                float3 rotated = float3(v.vertex.z, v.vertex.y, -v.vertex.x);
                o.viewDir = normalize(rotated);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return texCUBE(_MainTex, i.viewDir);
            }
            ENDCG
        }
    }
}
