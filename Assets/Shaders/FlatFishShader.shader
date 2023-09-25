Shader "Unlit/FlatFishShader"
{
Properties
{
[NoScaleOffset]_MainTex("Texture", 2D) = "white" {}
_SwimSpeed ("Swim Speed", Range(0,100)) = 5.0
_OffsetStrength("Offset Strength", Range(0, 100)) = 5.0
_Threshold("Threshold", Range(0,100)) = 20.0
}
SubShader
{
Cull Off

Pass
{

    CGPROGRAM

    #pragma target 3.0
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
    float4 _MainTex_TexelSize;

    float _SwimSpeed;
    float _OffsetStrength;
    float _Threshold;

    v2f vert (appdata v)
    {
        v2f o;

        float scale = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
        float offset = scale * .5 - .5;
        o.uv = float2(1-v.uv.x, v.uv.y * scale - offset);

        v.vertex.z = v.vertex.z + sin(-_Time.y * _SwimSpeed + v.uv.x) * _OffsetStrength / 100 * v.uv.x;


        o.vertex = UnityObjectToClipPos(v.vertex);
        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        // sample the texture
        fixed4 col = tex2D(_MainTex, i.uv);
        if (col.a == 0) {
            clip(-1);
        }
        return col * col.a;
    }

    ENDCG
}
}
}
