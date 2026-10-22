Shader "Meridian/Layered Ocean"
{
    Properties
    {
        _NormalTex("Ripple slopes",2D)="gray"{}
        _DeepColor("Deep water",Color)=(0.018,0.125,0.18,1)
        _BodyColor("Body water",Color)=(0.035,0.25,0.29,1)
        _SkyColor("Sky reflection",Color)=(0.48,0.64,0.69,1)
        _FarWater("Distant water only",Float)=0
    }
    SubShader
    {
        Tags{"RenderType"="Opaque" "Queue"="Geometry"}
        Pass
        {
            Tags{"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            sampler2D _NormalTex;
            fixed4 _DeepColor,_BodyColor,_SkyColor;
            float4 _MeridianSea;
            float4 _MeridianPatch;
            float _FarWater,_MeridianAmplitude;
            struct appdata{float4 vertex:POSITION;float3 normal:NORMAL;};
            struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;UNITY_FOG_COORDS(2)};
            v2f vert(appdata v)
            {
                v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.pos);return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float t=_Time.y;
                // Far water must not fill the near patch's troughs at the mean sea level.
                if(_FarWater>.5)
                {
                    float2 edge=abs(i.world.xz-_MeridianPatch.xy)/max(_MeridianPatch.zw,float2(1,1));
                    clip(max(edge.x,edge.y)-1.0001);
                }
                float2 warp=(tex2D(_NormalTex,i.world.xz*.018+float2(t*.002,-t*.001)).rg*2-1)*.55;
                float2 wind=normalize(_MeridianSea.xy+float2(.00001,0));
                float2 local=float2(dot(i.world.xz,wind),dot(i.world.xz,float2(-wind.y,wind.x)));
                float2 rotated=float2(local.x*.71+local.y*.70,-local.x*.70+local.y*.71);
                float3 a=tex2D(_NormalTex,local*.105+warp+float2(-t*.013,t*.008)).rgb;
                float3 b=tex2D(_NormalTex,rotated*.227-warp*.6+float2(-t*.009,t*.017)).rgb;
                float distanceToCamera=distance(_WorldSpaceCameraPos,i.world);
                float detailFade=1-smoothstep(50,230,distanceToCamera);
                float2 localSlope=((a.rg*2-1)+(b.rg*2-1)*.45)*lerp(.025,.08,detailFade)*max(.35,_MeridianSea.z);
                float2 slope=wind*localSlope.x+float2(-wind.y,wind.x)*localSlope.y;
                float3 n=normalize(i.normal+float3(slope.x,0,slope.y));
                float3 view=normalize(_WorldSpaceCameraPos-i.world);
                float facing=saturate(dot(n,view));float fresnel=.035+.67*pow(1-facing,4);
                float3 reflection=reflect(-view,n);
                float3 sky=lerp(_SkyColor.rgb*.68,_SkyColor.rgb,saturate(reflection.y*.65+.35));
                float tint=saturate(.32+a.b*.36+b.b*.18+i.world.y*.15);
                float elevation=(i.world.y+.72)/max(.05,_MeridianAmplitude);
                float3 water=lerp(_DeepColor.rgb,_BodyColor.rgb,saturate(tint+elevation*.45));
                float3 light=normalize(_WorldSpaceLightPos0.xyz);
                float3 halfVector=view+light;halfVector*=rsqrt(max(.01,dot(halfVector,halfVector)));
                // Restrained glints, not a broad screen-centred solar blob.
                float spec=pow(saturate(dot(n,halfVector)),240)*.12*detailFade*saturate(dot(n,light));
                float diffuse=.62+.38*saturate(dot(n,light));
                float3 color=lerp(water*diffuse,sky,fresnel)+spec*_LightColor0.rgb;
                float crest=smoothstep(.38,.85,elevation)*lerp(.35,1,smoothstep(.35,.62,a.b));
                color=lerp(color,float3(.68,.83,.84),crest*_MeridianSea.w*3.4*detailFade);
                fixed4 result=fixed4(color,1);UNITY_APPLY_FOG(i.fogCoord,result);return result;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
