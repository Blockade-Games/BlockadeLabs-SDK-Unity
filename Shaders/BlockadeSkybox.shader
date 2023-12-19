Shader "Custom/BlockadeSkybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthMap ("Depth Map", 2D) = "white" {}
        _DepthScale ("Depth Scale", Float) = 1.0
        _DepthCurve ("Depth Curve", Float) = 2.0
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
            float _DepthCurve;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.uv = v.uv;
                o.uv.y = 1 - o.uv.y;

                float4 uvLOD = float4(o.uv, 0, 0);

                float depth = 1 - tex2Dlod(_DepthMap, uvLOD).r;

                depth = pow(depth, _DepthCurve);

                float3 norm = normalize(v.vertex.xyz);
                v.vertex.xyz += norm * depth * _DepthScale;

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
