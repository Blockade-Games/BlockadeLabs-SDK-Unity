Shader "Custom/BlockadeSkybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthMap ("Depth Map", 2D) = "white" {}
        _DepthScale ("Depth Scale", Range(3, 10)) = 5.3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
