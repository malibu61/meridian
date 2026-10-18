Shader "Meridian/Hull Marking"
{
    Properties
    {
        _MainTex ("Stencil paint", 2D) = "white" {}
        _Color ("Paint colour", Color) = (0.88,0.89,0.85,1)
        _Cutoff ("Cutoff", Range(0,1)) = 0.45
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest+10" "RenderType"="TransparentCutout" }
        Cull Back
        ZWrite On
        ZTest LEqual
        Offset -1, -1
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; UNITY_FOG_COORDS(1) };
            sampler2D _MainTex;
            float4 _Color;
            float _Cutoff;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 paint = tex2D(_MainTex, i.uv);
                clip(paint.a - _Cutoff);
                fixed4 colour = fixed4(paint.rgb * _Color.rgb,1);
                UNITY_APPLY_FOG(i.fogCoord,colour);
                return colour;
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent Cutout"
}
