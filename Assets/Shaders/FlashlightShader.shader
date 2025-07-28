Shader "Custom/FlashlightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
            float4 _MainTex_ST;
            float4 _LightColor;
            float _Intensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calculate distance from center (for radial falloff)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // Create radial falloff
                float falloff = 1.0 - saturate(dist * 2.0);
                falloff = pow(falloff, 2.0); // Smooth falloff
                
                // Apply light color and intensity
                col *= _LightColor * _Intensity * falloff;
                
                // Ensure alpha is properly set for transparency
                col.a = _LightColor.a * falloff;
                
                return col;
            }
            ENDCG
        }
    }
} 