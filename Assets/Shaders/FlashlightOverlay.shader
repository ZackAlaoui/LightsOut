Shader "Custom/FlashlightOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightPos ("Light Position", Vector) = (0.5, 0.5, 0, 0)
        _LightRadius ("Light Radius", Range(0, 1)) = 0.3
        _LightAngle ("Light Angle", Range(0, 360)) = 45
        _LightDirection ("Light Direction", Vector) = (1, 0, 0, 0)
        _Darkness ("Darkness", Range(0, 1)) = 0.9
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
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
            float4 _LightPos;
            float _LightRadius;
            float _LightAngle;
            float4 _LightDirection;
            float _Darkness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Clamp light position to valid range
                float2 lightPos = clamp(_LightPos.xy, float2(0, 0), float2(1, 1));
                
                // Calculate distance from light position
                float2 lightDir = i.uv - lightPos;
                float dist = length(lightDir);
                
                // Calculate angle from light direction
                float angle = atan2(lightDir.y, lightDir.x);
                float lightAngleRad = _LightDirection.x * 3.14159 / 180.0;
                float angleDiff = abs(angle - lightAngleRad);
                if (angleDiff > 3.14159) angleDiff = 6.28318 - angleDiff;
                
                // Check if point is within light cone
                bool inLight = dist < _LightRadius && angleDiff < _LightAngle * 3.14159 / 360.0;
                
                // Create smooth falloff
                float falloff = 1.0 - smoothstep(0.0, _LightRadius, dist);
                float angleFalloff = 1.0 - smoothstep(0.0, _LightAngle * 3.14159 / 360.0, angleDiff);
                float totalFalloff = falloff * angleFalloff;
                
                // Set alpha based on whether point is in light
                float alpha = inLight ? (1.0 - totalFalloff) * _Darkness : _Darkness;
                
                // Ensure alpha is never negative
                alpha = max(alpha, 0.0);
                
                return fixed4(0, 0, 0, alpha);
            }
            ENDCG
        }
    }
} 