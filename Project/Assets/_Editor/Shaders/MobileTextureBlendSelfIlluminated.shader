Shader "Akaito Ai/Mobile/Blend/TextureBlendSelfIlluminated"
{
    Properties
    {
        // Texture 1
        _MainTex ("Texture 1", 2D) = "white" {}
        _MainColor1 ("Main Color 1", Color) = (1, 1, 1, 1)
        _MainTex_ST ("Texture 1 Tiling & Offset", Vector) = (1, 1, 0, 0) // Tiling and Offset for Texture 1

        // Texture 2
        _SecondTex ("Texture 2", 2D) = "white" {}
        _MainColor2 ("Main Color 2", Color) = (1, 1, 1, 1)
        _SecondTex_ST ("Texture 2 Tiling & Offset", Vector) = (1, 1, 0, 0) // Tiling and Offset for Texture 2

        // Blending control
        _Blend ("Blend Factor", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _SecondTex;

            fixed4 _MainColor1;
            fixed4 _MainColor2;

            float _Blend;

            float4 _MainTex_ST;   // Tiling & Offset for Texture 1
            float4 _SecondTex_ST; // Tiling & Offset for Texture 2

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_SecondTex : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);   // Apply tiling and offset for Texture 1
                o.uv_SecondTex = TRANSFORM_TEX(v.uv, _SecondTex); // Apply tiling and offset for Texture 2
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample both textures and apply their respective colors
                fixed4 col1 = tex2D(_MainTex, i.uv_MainTex) * _MainColor1; // Multiply by color property
                fixed4 col2 = tex2D(_SecondTex, i.uv_SecondTex) * _MainColor2; // Multiply by color property

                // Blend between the two main colors/textures
                fixed4 baseColor = lerp(col1, col2, _Blend);

                // Final output: self-illuminated color
                return baseColor; // Return the final self-illuminated color
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
