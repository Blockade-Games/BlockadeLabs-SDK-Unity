Shader "BlockadeLabsSDK/BlockadeSkybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthMap ("Depth Map", 2D) = "white" {}
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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            TEXTURE2D(_DepthMap);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_DepthMap);
            float _DepthScale;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.uv = IN.uv;

                float depth = SAMPLE_TEXTURE2D_LOD(_DepthMap, sampler_DepthMap, IN.uv, 0).g;
                depth = clamp(1.0 / depth + 10 / _DepthScale, 0, _DepthScale);

                IN.positionOS.xyz = normalize(IN.positionOS.xyz) * depth;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _DepthMap;
            float _DepthScale;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.uv = v.uv;

                float4 uvLOD = float4(o.uv, 0, 0);
                float depth = tex2Dlod(_DepthMap, uvLOD).g;

                depth = clamp(1.0 / depth + 10 / _DepthScale, 0, _DepthScale);

                v.vertex.xyz = normalize(v.vertex.xyz) * depth;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
