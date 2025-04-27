Shader "Custom/SpriteGlow" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 2
        _GlowFalloff ("Glow Falloff", Range(0.1, 2)) = 0.5
    }
    SubShader {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100"  // Renders after regular transparent objects
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        Blend One OneMinusSrcAlpha  // Better blending mode for glow
        ZWrite Off
        Cull Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _GlowIntensity;
            float _GlowFalloff;
            
            v2f vert (appdata v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * i.color;
                
                // Enhanced glow effect with falloff
                float glowFactor = _GlowIntensity * (1 - (tex.a * _GlowFalloff));
                col.rgb *= (1 + glowFactor);
                col.a = tex.a;  // Maintain original alpha
                
                // Premultiplied alpha correction
                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}