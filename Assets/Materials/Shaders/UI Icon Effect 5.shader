Shader "Unlit/UI Icon Effect 5"
{
    Properties
    {
        [NoScaleOffset] _MainTex("MainTex", 2D) = "white" {}

        _Stencil("Stencil ID", Float) = 0
        _StencilComp("StencilComp", Float) = 8
        _StencilOp("StencilOp", Float) = 0
        _StencilReadMask("StencilReadMask", Float) = 255
        _StencilWriteMask("StencilWriteMask", Float) = 255
        _ColorMask("ColorMask", Float) = 15
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent+0"
        }

        Pass
        {
            Name "Sprite Lit"
            Tags
            {
                "LightMode" = "Universal2D"
            }

        // Render State
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZTest[unity_GUIZTestMode]
        ZWrite Off
        // ColorMask: <None>
        Stencil{
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        ColorMask[_ColorMask]


        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        // Pragmas
        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x
        #pragma target 2.0

        // Keywords
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_SCREENPOSITION
        #define SHADERPASS_SPRITELIT

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

        // --------------------------------------------------
        // Graph

        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        CBUFFER_END
        TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_TexelSize;
        SAMPLER(_SampleTexture2D_58F4179B_Sampler_3_Linear_Repeat);

        // Graph Functions

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }


        inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
        }

        inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
        {
            return (1.0 - t) * a + (t * b);
        }


        inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);

            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;

            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3 - 0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3 - 1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3 - 2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

            Out = t;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Posterize_float(float In, float Steps, out float Out)
        {
            Out = floor(In / (1 / Steps)) * (1 / Steps);
        }

        void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = Base * Blend;
            Out = lerp(Base, Out, Opacity);
        }

        void Unity_Blend_Exclusion_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = Blend + Base - (2.0 * Blend * Base);
            Out = lerp(Base, Out, Opacity);
        }

        void Unity_Blend_Overlay_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            float4 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
            float4 result2 = 2.0 * Base * Blend;
            float4 zeroOrOne = step(Base, 0.5);
            Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
            Out = lerp(Base, Out, Opacity);
        }

        // Graph Vertex
        // GraphVertex: <None>

        // Graph Pixel
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
        };

        struct SurfaceDescription
        {
            float4 Color;
            float4 Mask;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_B0EEDE66_Out_0 = IN.uv0;
            float _Split_CFC93EDC_R_1 = _UV_B0EEDE66_Out_0[0];
            float _Split_CFC93EDC_G_2 = _UV_B0EEDE66_Out_0[1];
            float _Split_CFC93EDC_B_3 = _UV_B0EEDE66_Out_0[2];
            float _Split_CFC93EDC_A_4 = _UV_B0EEDE66_Out_0[3];
            float _OneMinus_57F08501_Out_1;
            Unity_OneMinus_float(_Split_CFC93EDC_G_2, _OneMinus_57F08501_Out_1);
            float _OneMinus_4D5DC3F1_Out_1;
            Unity_OneMinus_float(_Split_CFC93EDC_G_2, _OneMinus_4D5DC3F1_Out_1);
            float _Add_89D4B21_Out_2;
            Unity_Add_float(_OneMinus_57F08501_Out_1, _OneMinus_4D5DC3F1_Out_1, _Add_89D4B21_Out_2);
            float _Clamp_D240331F_Out_3;
            Unity_Clamp_float(_Add_89D4B21_Out_2, 0, 1, _Clamp_D240331F_Out_3);
            float2 _TilingAndOffset_8C2F351E_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), float2 (0, 8.28), _TilingAndOffset_8C2F351E_Out_3);
            float _SimpleNoise_DF0D8309_Out_2;
            Unity_SimpleNoise_float(_TilingAndOffset_8C2F351E_Out_3, 22, _SimpleNoise_DF0D8309_Out_2);
            float _Multiply_3F554D8_Out_2;
            Unity_Multiply_float(_Clamp_D240331F_Out_3, _SimpleNoise_DF0D8309_Out_2, _Multiply_3F554D8_Out_2);
            float _Posterize_DF55E144_Out_2;
            Unity_Posterize_float(_Multiply_3F554D8_Out_2, 6, _Posterize_DF55E144_Out_2);
            float4 _SampleTexture2D_58F4179B_RGBA_0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv0.xy);
            float _SampleTexture2D_58F4179B_R_4 = _SampleTexture2D_58F4179B_RGBA_0.r;
            float _SampleTexture2D_58F4179B_G_5 = _SampleTexture2D_58F4179B_RGBA_0.g;
            float _SampleTexture2D_58F4179B_B_6 = _SampleTexture2D_58F4179B_RGBA_0.b;
            float _SampleTexture2D_58F4179B_A_7 = _SampleTexture2D_58F4179B_RGBA_0.a;
            float4 _Blend_CC5D8F7D_Out_2;
            Unity_Blend_Multiply_float4((_Posterize_DF55E144_Out_2.xxxx), _SampleTexture2D_58F4179B_RGBA_0, _Blend_CC5D8F7D_Out_2, 1);
            float4 _Blend_4F6F0094_Out_2;
            Unity_Blend_Exclusion_float4(float4(-0.5, -0.5, -0.5, -0.5), _Blend_CC5D8F7D_Out_2, _Blend_4F6F0094_Out_2, 1);
            float4 _Blend_3E0CC763_Out_2;
            Unity_Blend_Overlay_float4(_Blend_4F6F0094_Out_2, (_SampleTexture2D_58F4179B_A_7.xxxx), _Blend_3E0CC763_Out_2, 1);
            surface.Color = _Blend_3E0CC763_Out_2;
            surface.Mask = IsGammaSpace() ? float4(1, 1, 1, 1) : float4 (SRGBToLinear(float3(1, 1, 1)), 1);
            return surface;
        }

        // --------------------------------------------------
        // Structs and Packing

        // Generated Type: Attributes
        struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };

        // Generated Type: Varyings
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            float4 color;
            float4 screenPosition;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

        // Generated Type: PackedVaryings
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            float4 interp00 : TEXCOORD0;
            float4 interp01 : TEXCOORD1;
            float4 interp02 : TEXCOORD2;
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

        // Packed Type: Varyings
        PackedVaryings PackVaryings(Varyings input)
        {
            PackedVaryings output = (PackedVaryings)0;
            output.positionCS = input.positionCS;
            output.interp00.xyzw = input.texCoord0;
            output.interp01.xyzw = input.color;
            output.interp02.xyzw = input.screenPosition;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

        // Unpacked Type: Varyings
        Varyings UnpackVaryings(PackedVaryings input)
        {
            Varyings output = (Varyings)0;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp00.xyzw;
            output.color = input.interp01.xyzw;
            output.screenPosition = input.interp02.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

        // --------------------------------------------------
        // Build Graph Inputs

        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }


        // --------------------------------------------------
        // Main

        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteLitPass.hlsl"

        ENDHLSL
    }

    Pass
    {
        Name "Sprite Normal"
        Tags
        {
            "LightMode" = "NormalsRendering"
        }

            // Render State
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZTest LEqual
            ZWrite Off
            // ColorMask: <None>


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define SHADERPASS_SPRITENORMAL

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_TexelSize;
            SAMPLER(_SampleTexture2D_58F4179B_Sampler_3_Linear_Repeat);

            // Graph Functions

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }


            inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
            {
                return (1.0 - t) * a + (t * b);
            }


            inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                uv = abs(frac(uv) - 0.5);
                float2 c0 = i + float2(0.0, 0.0);
                float2 c1 = i + float2(1.0, 0.0);
                float2 c2 = i + float2(0.0, 1.0);
                float2 c3 = i + float2(1.0, 1.0);
                float r0 = Unity_SimpleNoise_RandomValue_float(c0);
                float r1 = Unity_SimpleNoise_RandomValue_float(c1);
                float r2 = Unity_SimpleNoise_RandomValue_float(c2);
                float r3 = Unity_SimpleNoise_RandomValue_float(c3);

                float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
                float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
                float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
                return t;
            }
            void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
            {
                float t = 0.0;

                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3 - 0));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3 - 1));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3 - 2));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                Out = t;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            void Unity_Posterize_float(float In, float Steps, out float Out)
            {
                Out = floor(In / (1 / Steps)) * (1 / Steps);
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Blend_Exclusion_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Blend + Base - (2.0 * Blend * Base);
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Blend_Overlay_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                float4 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
                float4 result2 = 2.0 * Base * Blend;
                float4 zeroOrOne = step(Base, 0.5);
                Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
                Out = lerp(Base, Out, Opacity);
            }

            // Graph Vertex
            // GraphVertex: <None>

            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float4 uv0;
            };

            struct SurfaceDescription
            {
                float4 Color;
                float3 Normal;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _UV_B0EEDE66_Out_0 = IN.uv0;
                float _Split_CFC93EDC_R_1 = _UV_B0EEDE66_Out_0[0];
                float _Split_CFC93EDC_G_2 = _UV_B0EEDE66_Out_0[1];
                float _Split_CFC93EDC_B_3 = _UV_B0EEDE66_Out_0[2];
                float _Split_CFC93EDC_A_4 = _UV_B0EEDE66_Out_0[3];
                float _OneMinus_57F08501_Out_1;
                Unity_OneMinus_float(_Split_CFC93EDC_G_2, _OneMinus_57F08501_Out_1);
                float _OneMinus_4D5DC3F1_Out_1;
                Unity_OneMinus_float(_Split_CFC93EDC_G_2, _OneMinus_4D5DC3F1_Out_1);
                float _Add_89D4B21_Out_2;
                Unity_Add_float(_OneMinus_57F08501_Out_1, _OneMinus_4D5DC3F1_Out_1, _Add_89D4B21_Out_2);
                float _Clamp_D240331F_Out_3;
                Unity_Clamp_float(_Add_89D4B21_Out_2, 0, 1, _Clamp_D240331F_Out_3);
                float2 _TilingAndOffset_8C2F351E_Out_3;
                Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), float2 (0, 8.28), _TilingAndOffset_8C2F351E_Out_3);
                float _SimpleNoise_DF0D8309_Out_2;
                Unity_SimpleNoise_float(_TilingAndOffset_8C2F351E_Out_3, 22, _SimpleNoise_DF0D8309_Out_2);
                float _Multiply_3F554D8_Out_2;
                Unity_Multiply_float(_Clamp_D240331F_Out_3, _SimpleNoise_DF0D8309_Out_2, _Multiply_3F554D8_Out_2);
                float _Posterize_DF55E144_Out_2;
                Unity_Posterize_float(_Multiply_3F554D8_Out_2, 6, _Posterize_DF55E144_Out_2);
                float4 _SampleTexture2D_58F4179B_RGBA_0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv0.xy);
                float _SampleTexture2D_58F4179B_R_4 = _SampleTexture2D_58F4179B_RGBA_0.r;
                float _SampleTexture2D_58F4179B_G_5 = _SampleTexture2D_58F4179B_RGBA_0.g;
                float _SampleTexture2D_58F4179B_B_6 = _SampleTexture2D_58F4179B_RGBA_0.b;
                float _SampleTexture2D_58F4179B_A_7 = _SampleTexture2D_58F4179B_RGBA_0.a;
                float4 _Blend_CC5D8F7D_Out_2;
                Unity_Blend_Multiply_float4((_Posterize_DF55E144_Out_2.xxxx), _SampleTexture2D_58F4179B_RGBA_0, _Blend_CC5D8F7D_Out_2, 1);
                float4 _Blend_4F6F0094_Out_2;
                Unity_Blend_Exclusion_float4(float4(-0.5, -0.5, -0.5, -0.5), _Blend_CC5D8F7D_Out_2, _Blend_4F6F0094_Out_2, 1);
                float4 _Blend_3E0CC763_Out_2;
                Unity_Blend_Overlay_float4(_Blend_4F6F0094_Out_2, (_SampleTexture2D_58F4179B_A_7.xxxx), _Blend_3E0CC763_Out_2, 1);
                surface.Color = _Blend_3E0CC763_Out_2;
                surface.Normal = float3 (0, 0, 1);
                return surface;
            }

            // --------------------------------------------------
            // Structs and Packing

            // Generated Type: Attributes
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            // Generated Type: Varyings
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS;
                float4 tangentWS;
                float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float3 interp00 : TEXCOORD0;
                float4 interp01 : TEXCOORD1;
                float4 interp02 : TEXCOORD2;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.normalWS;
                output.interp01.xyzw = input.tangentWS;
                output.interp02.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }

            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.normalWS = input.interp00.xyz;
                output.tangentWS = input.interp01.xyzw;
                output.texCoord0 = input.interp02.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





                output.uv0 = input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
            }


            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteNormalPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "Sprite Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

                // Render State
                Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                Cull Off
                ZTest LEqual
                ZWrite Off
                // ColorMask: <None>


                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                // Debug
                // <None>

                // --------------------------------------------------
                // Pass

                // Pragmas
                #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x
                #pragma target 2.0

                // Keywords
                #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
                // GraphKeywords: <None>

                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_COLOR
                #define SHADERPASS_SPRITEFORWARD

                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

                // --------------------------------------------------
                // Graph

                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                CBUFFER_END
                TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_TexelSize;
                SAMPLER(_SampleTexture2D_58F4179B_Sampler_3_Linear_Repeat);

                // Graph Functions

                void Unity_OneMinus_float(float In, out float Out)
                {
                    Out = 1 - In;
                }

                void Unity_Add_float(float A, float B, out float Out)
                {
                    Out = A + B;
                }

                void Unity_Clamp_float(float In, float Min, float Max, out float Out)
                {
                    Out = clamp(In, Min, Max);
                }

                void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
                {
                    Out = UV * Tiling + Offset;
                }


                inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
                {
                    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
                }

                inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
                {
                    return (1.0 - t) * a + (t * b);
                }


                inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
                {
                    float2 i = floor(uv);
                    float2 f = frac(uv);
                    f = f * f * (3.0 - 2.0 * f);

                    uv = abs(frac(uv) - 0.5);
                    float2 c0 = i + float2(0.0, 0.0);
                    float2 c1 = i + float2(1.0, 0.0);
                    float2 c2 = i + float2(0.0, 1.0);
                    float2 c3 = i + float2(1.0, 1.0);
                    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
                    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
                    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
                    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

                    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
                    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
                    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
                    return t;
                }
                void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
                {
                    float t = 0.0;

                    float freq = pow(2.0, float(0));
                    float amp = pow(0.5, float(3 - 0));
                    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                    freq = pow(2.0, float(1));
                    amp = pow(0.5, float(3 - 1));
                    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                    freq = pow(2.0, float(2));
                    amp = pow(0.5, float(3 - 2));
                    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                    Out = t;
                }

                void Unity_Multiply_float(float A, float B, out float Out)
                {
                    Out = A * B;
                }

                void Unity_Posterize_float(float In, float Steps, out float Out)
                {
                    Out = floor(In / (1 / Steps)) * (1 / Steps);
                }

                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }

                void Unity_Blend_Exclusion_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Blend + Base - (2.0 * Blend * Base);
                    Out = lerp(Base, Out, Opacity);
                }

                void Unity_Blend_Overlay_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    float4 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
                    float4 result2 = 2.0 * Base * Blend;
                    float4 zeroOrOne = step(Base, 0.5);
                    Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
                    Out = lerp(Base, Out, Opacity);
                }

                // Graph Vertex
                // GraphVertex: <None>

                // Graph Pixel
                struct SurfaceDescriptionInputs
                {
                    float4 uv0;
                };

                struct SurfaceDescription
                {
                    float4 Color;
                    float3 Normal;
                };

                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float4 _UV_B0EEDE66_Out_0 = IN.uv0;
                    float _Split_CFC93EDC_R_1 = _UV_B0EEDE66_Out_0[0];
                    float _Split_CFC93EDC_G_2 = _UV_B0EEDE66_Out_0[1];
                    float _Split_CFC93EDC_B_3 = _UV_B0EEDE66_Out_0[2];
                    float _Split_CFC93EDC_A_4 = _UV_B0EEDE66_Out_0[3];
                    float _OneMinus_57F08501_Out_1;
                    Unity_OneMinus_float(_Split_CFC93EDC_G_2, _OneMinus_57F08501_Out_1);
                    float _OneMinus_4D5DC3F1_Out_1;
                    Unity_OneMinus_float(_Split_CFC93EDC_G_2, _OneMinus_4D5DC3F1_Out_1);
                    float _Add_89D4B21_Out_2;
                    Unity_Add_float(_OneMinus_57F08501_Out_1, _OneMinus_4D5DC3F1_Out_1, _Add_89D4B21_Out_2);
                    float _Clamp_D240331F_Out_3;
                    Unity_Clamp_float(_Add_89D4B21_Out_2, 0, 1, _Clamp_D240331F_Out_3);
                    float2 _TilingAndOffset_8C2F351E_Out_3;
                    Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), float2 (0, 8.28), _TilingAndOffset_8C2F351E_Out_3);
                    float _SimpleNoise_DF0D8309_Out_2;
                    Unity_SimpleNoise_float(_TilingAndOffset_8C2F351E_Out_3, 22, _SimpleNoise_DF0D8309_Out_2);
                    float _Multiply_3F554D8_Out_2;
                    Unity_Multiply_float(_Clamp_D240331F_Out_3, _SimpleNoise_DF0D8309_Out_2, _Multiply_3F554D8_Out_2);
                    float _Posterize_DF55E144_Out_2;
                    Unity_Posterize_float(_Multiply_3F554D8_Out_2, 6, _Posterize_DF55E144_Out_2);
                    float4 _SampleTexture2D_58F4179B_RGBA_0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv0.xy);
                    float _SampleTexture2D_58F4179B_R_4 = _SampleTexture2D_58F4179B_RGBA_0.r;
                    float _SampleTexture2D_58F4179B_G_5 = _SampleTexture2D_58F4179B_RGBA_0.g;
                    float _SampleTexture2D_58F4179B_B_6 = _SampleTexture2D_58F4179B_RGBA_0.b;
                    float _SampleTexture2D_58F4179B_A_7 = _SampleTexture2D_58F4179B_RGBA_0.a;
                    float4 _Blend_CC5D8F7D_Out_2;
                    Unity_Blend_Multiply_float4((_Posterize_DF55E144_Out_2.xxxx), _SampleTexture2D_58F4179B_RGBA_0, _Blend_CC5D8F7D_Out_2, 1);
                    float4 _Blend_4F6F0094_Out_2;
                    Unity_Blend_Exclusion_float4(float4(-0.5, -0.5, -0.5, -0.5), _Blend_CC5D8F7D_Out_2, _Blend_4F6F0094_Out_2, 1);
                    float4 _Blend_3E0CC763_Out_2;
                    Unity_Blend_Overlay_float4(_Blend_4F6F0094_Out_2, (_SampleTexture2D_58F4179B_A_7.xxxx), _Blend_3E0CC763_Out_2, 1);
                    surface.Color = _Blend_3E0CC763_Out_2;
                    surface.Normal = float3 (0, 0, 1);
                    return surface;
                }

                // --------------------------------------------------
                // Structs and Packing

                // Generated Type: Attributes
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };

                // Generated Type: Varyings
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };

                // Generated Type: PackedVaryings
                struct PackedVaryings
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    float4 interp00 : TEXCOORD0;
                    float4 interp01 : TEXCOORD1;
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };

                // Packed Type: Varyings
                PackedVaryings PackVaryings(Varyings input)
                {
                    PackedVaryings output = (PackedVaryings)0;
                    output.positionCS = input.positionCS;
                    output.interp00.xyzw = input.texCoord0;
                    output.interp01.xyzw = input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }

                // Unpacked Type: Varyings
                Varyings UnpackVaryings(PackedVaryings input)
                {
                    Varyings output = (Varyings)0;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp00.xyzw;
                    output.color = input.interp01.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }

                // --------------------------------------------------
                // Build Graph Inputs

                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





                    output.uv0 = input.texCoord0;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                    return output;
                }


                // --------------------------------------------------
                // Main

                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteForwardPass.hlsl"

                ENDHLSL
            }

    }
        FallBack "Hidden/Shader Graph/FallbackError"
}
