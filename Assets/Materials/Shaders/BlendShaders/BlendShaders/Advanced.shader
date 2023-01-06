// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Advanced"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
         [MaterialToggle] _alphaCutoffValue("AlphaCutoff", Range(0, 1)) = 0

         _BlendSrc1("__src1", Range(0, 10)) = 1.0
         _BlendDst1("__dst1", Range(0, 10)) = 0.0
         _BlendSrcAlpha1("__src_alpha1", Range(0, 10)) = 1.0
         _BlendDstAlpha1("__dst_alpha1", Range(0, 10)) = 0.0
            _BlendOp("BlendOp", Range(0, 20)) = 0.0
               _StencilComp("Stencil Comparison", Float) = 8
         _Stencil("Stencil ID", Float) = 0
         _StencilOp("Stencil Operation", Float) = 0
         _StencilWriteMask("Stencil Write Mask", Float) = 255
         _StencilReadMask("Stencil Read Mask", Float) = 255
         _ColorMask("Color Mask", Float) = 15
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
                AlphaToMask[_alphaCutoffValue]
            Fog { Mode Off }
                 BlendOp[_BlendOp]
                Blend[_BlendSrc1][_BlendDst1],[_BlendSrcAlpha1][_BlendDstAlpha1]

                           Stencil
            {
                 Ref[_Stencil]
                 Comp[_StencilComp]
                 Pass[_StencilOp]
                 ReadMask[_StencilReadMask]
                 WriteMask[_StencilWriteMask]
            }
            ColorMask[_ColorMask]
            //Blend DstColor SrcColor
            Pass
            {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile DUMMY PIXELSNAP_ON
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    half2 texcoord  : TEXCOORD0;
                };

                fixed4 _Color;

                v2f vert(appdata_t IN)
                {
                    v2f OUT;
                    OUT.vertex = UnityObjectToClipPos(IN.vertex);
                    OUT.texcoord = IN.texcoord;
                    OUT.color = IN.color * _Color;
                    #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap(OUT.vertex);
                    #endif

                    return OUT;
                }

                sampler2D _MainTex;

                fixed4 frag(v2f IN) : SV_Target
                {
                    fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                    c.rgb *= c.a;

                    return c;
                }
            ENDCG
            }

        }
}