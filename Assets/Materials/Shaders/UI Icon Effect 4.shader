Shader "Unlit/UI Icon effect 4"
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
        #define VARYINGS_NEED_POSITION_WS 
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_SCREENPOSITION
        #define SHADERPASS_SPRITELIT
        #define REQUIRE_OPAQUE_TEXTURE

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
        SAMPLER(_SampleTexture2D_FD26B85_Sampler_3_Linear_Repeat);

        // Graph Functions

        void Unity_Spherize_float(float2 UV, float2 Center, float2 Strength, float2 Offset, out float2 Out)
        {
            float2 delta = UV - Center;
            float delta2 = dot(delta.xy, delta.xy);
            float delta4 = delta2 * delta2;
            float2 delta_offset = delta4 * Strength;
            Out = UV + delta * delta_offset + Offset;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }


        float2 Unity_GradientNoise_Dir_float(float2 p)
        {
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
            p = p % 289;
            float x = (34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
        {
            float2 p = UV * Scale;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }

        void Unity_Posterize_float(float In, float Steps, out float Out)
        {
            Out = floor(In / (1 / Steps)) * (1 / Steps);
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_Blend_Negation_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = 1.0 - abs(1.0 - Blend - Base);
            Out = lerp(Base, Out, Opacity);
        }

        void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = Base * Blend;
            Out = lerp(Base, Out, Opacity);
        }

        // Graph Vertex
        // GraphVertex: <None>

        // Graph Pixel
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            float2 _Spherize_ACB33F36_Out_4;
            Unity_Spherize_float(IN.uv0.xy, float2 (0.5, 0.5), float2 (10, 10), float2 (0, 0), _Spherize_ACB33F36_Out_4);
            float2 _TilingAndOffset_D7D2890F_Out_3;
            Unity_TilingAndOffset_float(_Spherize_ACB33F36_Out_4, float2 (1, 1), float2 (8.72, 1.32), _TilingAndOffset_D7D2890F_Out_3);
            float _GradientNoise_6437A85B_Out_2;
            Unity_GradientNoise_float(_TilingAndOffset_D7D2890F_Out_3, 1.08, _GradientNoise_6437A85B_Out_2);
            float _Posterize_711F7AB0_Out_2;
            Unity_Posterize_float(_GradientNoise_6437A85B_Out_2, 5.12, _Posterize_711F7AB0_Out_2);
            float _Multiply_D91393F2_Out_2;
            Unity_Multiply_float(_Posterize_711F7AB0_Out_2, 1.2, _Multiply_D91393F2_Out_2);
            float3 _SceneColor_8AA1BAEB_Out_1;
            Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_8AA1BAEB_Out_1);
            float _Split_6C16775D_R_1 = _SceneColor_8AA1BAEB_Out_1[0];
            float _Split_6C16775D_G_2 = _SceneColor_8AA1BAEB_Out_1[1];
            float _Split_6C16775D_B_3 = _SceneColor_8AA1BAEB_Out_1[2];
            float _Split_6C16775D_A_4 = 0;
            float4 _SampleTexture2D_FD26B85_RGBA_0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv0.xy);
            float _SampleTexture2D_FD26B85_R_4 = _SampleTexture2D_FD26B85_RGBA_0.r;
            float _SampleTexture2D_FD26B85_G_5 = _SampleTexture2D_FD26B85_RGBA_0.g;
            float _SampleTexture2D_FD26B85_B_6 = _SampleTexture2D_FD26B85_RGBA_0.b;
            float _SampleTexture2D_FD26B85_A_7 = _SampleTexture2D_FD26B85_RGBA_0.a;
            float4 _Combine_945C2586_RGBA_4;
            float3 _Combine_945C2586_RGB_5;
            float2 _Combine_945C2586_RG_6;
            Unity_Combine_float(_Split_6C16775D_R_1, _Split_6C16775D_G_2, _Split_6C16775D_B_3, _SampleTexture2D_FD26B85_A_7, _Combine_945C2586_RGBA_4, _Combine_945C2586_RGB_5, _Combine_945C2586_RG_6);
            float4 _Blend_B3FD711D_Out_2;
            Unity_Blend_Negation_float4(_Combine_945C2586_RGBA_4, _SampleTexture2D_FD26B85_RGBA_0, _Blend_B3FD711D_Out_2, -1);
            float4 _Blend_FB86CF19_Out_2;
            Unity_Blend_Multiply_float4((_Multiply_D91393F2_Out_2.xxxx), _Blend_B3FD711D_Out_2, _Blend_FB86CF19_Out_2, 1);
            float4 _Blend_E598BE31_Out_2;
            Unity_Blend_Multiply_float4(float4(1.3, 1.3, 1.3, 1.3), _Blend_FB86CF19_Out_2, _Blend_E598BE31_Out_2, 1);
            surface.Color = _Blend_E598BE31_Out_2;
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
            float3 positionWS;
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
            float3 interp00 : TEXCOORD0;
            float4 interp01 : TEXCOORD1;
            float4 interp02 : TEXCOORD2;
            float4 interp03 : TEXCOORD3;
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
            output.interp00.xyz = input.positionWS;
            output.interp01.xyzw = input.texCoord0;
            output.interp02.xyzw = input.color;
            output.interp03.xyzw = input.screenPosition;
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
            output.positionWS = input.interp00.xyz;
            output.texCoord0 = input.interp01.xyzw;
            output.color = input.interp02.xyzw;
            output.screenPosition = input.interp03.xyzw;
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





            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
            #define VARYINGS_NEED_POSITION_WS 
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define SHADERPASS_SPRITENORMAL
            #define REQUIRE_OPAQUE_TEXTURE

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
            SAMPLER(_SampleTexture2D_FD26B85_Sampler_3_Linear_Repeat);

            // Graph Functions

            void Unity_Spherize_float(float2 UV, float2 Center, float2 Strength, float2 Offset, out float2 Out)
            {
                float2 delta = UV - Center;
                float delta2 = dot(delta.xy, delta.xy);
                float delta4 = delta2 * delta2;
                float2 delta_offset = delta4 * Strength;
                Out = UV + delta * delta_offset + Offset;
            }

            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }


            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            {
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

            void Unity_Posterize_float(float In, float Steps, out float Out)
            {
                Out = floor(In / (1 / Steps)) * (1 / Steps);
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            void Unity_SceneColor_float(float4 UV, out float3 Out)
            {
                Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
            }

            void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
            {
                RGBA = float4(R, G, B, A);
                RGB = float3(R, G, B);
                RG = float2(R, G);
            }

            void Unity_Blend_Negation_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = 1.0 - abs(1.0 - Blend - Base);
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            // Graph Vertex
            // GraphVertex: <None>

            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition;
                float4 ScreenPosition;
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
                float2 _Spherize_ACB33F36_Out_4;
                Unity_Spherize_float(IN.uv0.xy, float2 (0.5, 0.5), float2 (10, 10), float2 (0, 0), _Spherize_ACB33F36_Out_4);
                float2 _TilingAndOffset_D7D2890F_Out_3;
                Unity_TilingAndOffset_float(_Spherize_ACB33F36_Out_4, float2 (1, 1), float2 (8.72, 1.32), _TilingAndOffset_D7D2890F_Out_3);
                float _GradientNoise_6437A85B_Out_2;
                Unity_GradientNoise_float(_TilingAndOffset_D7D2890F_Out_3, 1.08, _GradientNoise_6437A85B_Out_2);
                float _Posterize_711F7AB0_Out_2;
                Unity_Posterize_float(_GradientNoise_6437A85B_Out_2, 5.12, _Posterize_711F7AB0_Out_2);
                float _Multiply_D91393F2_Out_2;
                Unity_Multiply_float(_Posterize_711F7AB0_Out_2, 1.2, _Multiply_D91393F2_Out_2);
                float3 _SceneColor_8AA1BAEB_Out_1;
                Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_8AA1BAEB_Out_1);
                float _Split_6C16775D_R_1 = _SceneColor_8AA1BAEB_Out_1[0];
                float _Split_6C16775D_G_2 = _SceneColor_8AA1BAEB_Out_1[1];
                float _Split_6C16775D_B_3 = _SceneColor_8AA1BAEB_Out_1[2];
                float _Split_6C16775D_A_4 = 0;
                float4 _SampleTexture2D_FD26B85_RGBA_0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv0.xy);
                float _SampleTexture2D_FD26B85_R_4 = _SampleTexture2D_FD26B85_RGBA_0.r;
                float _SampleTexture2D_FD26B85_G_5 = _SampleTexture2D_FD26B85_RGBA_0.g;
                float _SampleTexture2D_FD26B85_B_6 = _SampleTexture2D_FD26B85_RGBA_0.b;
                float _SampleTexture2D_FD26B85_A_7 = _SampleTexture2D_FD26B85_RGBA_0.a;
                float4 _Combine_945C2586_RGBA_4;
                float3 _Combine_945C2586_RGB_5;
                float2 _Combine_945C2586_RG_6;
                Unity_Combine_float(_Split_6C16775D_R_1, _Split_6C16775D_G_2, _Split_6C16775D_B_3, _SampleTexture2D_FD26B85_A_7, _Combine_945C2586_RGBA_4, _Combine_945C2586_RGB_5, _Combine_945C2586_RG_6);
                float4 _Blend_B3FD711D_Out_2;
                Unity_Blend_Negation_float4(_Combine_945C2586_RGBA_4, _SampleTexture2D_FD26B85_RGBA_0, _Blend_B3FD711D_Out_2, -1);
                float4 _Blend_FB86CF19_Out_2;
                Unity_Blend_Multiply_float4((_Multiply_D91393F2_Out_2.xxxx), _Blend_B3FD711D_Out_2, _Blend_FB86CF19_Out_2, 1);
                float4 _Blend_E598BE31_Out_2;
                Unity_Blend_Multiply_float4(float4(1.3, 1.3, 1.3, 1.3), _Blend_FB86CF19_Out_2, _Blend_E598BE31_Out_2, 1);
                surface.Color = _Blend_E598BE31_Out_2;
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
                float3 positionWS;
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
                float3 interp01 : TEXCOORD1;
                float4 interp02 : TEXCOORD2;
                float4 interp03 : TEXCOORD3;
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
                output.interp00.xyz = input.positionWS;
                output.interp01.xyz = input.normalWS;
                output.interp02.xyzw = input.tangentWS;
                output.interp03.xyzw = input.texCoord0;
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
                output.positionWS = input.interp00.xyz;
                output.normalWS = input.interp01.xyz;
                output.tangentWS = input.interp02.xyzw;
                output.texCoord0 = input.interp03.xyzw;
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





                output.WorldSpacePosition = input.positionWS;
                output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
                #define VARYINGS_NEED_POSITION_WS 
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_COLOR
                #define SHADERPASS_SPRITEFORWARD
                #define REQUIRE_OPAQUE_TEXTURE

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
                SAMPLER(_SampleTexture2D_FD26B85_Sampler_3_Linear_Repeat);

                // Graph Functions

                void Unity_Spherize_float(float2 UV, float2 Center, float2 Strength, float2 Offset, out float2 Out)
                {
                    float2 delta = UV - Center;
                    float delta2 = dot(delta.xy, delta.xy);
                    float delta4 = delta2 * delta2;
                    float2 delta_offset = delta4 * Strength;
                    Out = UV + delta * delta_offset + Offset;
                }

                void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
                {
                    Out = UV * Tiling + Offset;
                }


                float2 Unity_GradientNoise_Dir_float(float2 p)
                {
                    // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                    p = p % 289;
                    float x = (34 * p.x + 1) * p.x % 289 + p.y;
                    x = (34 * x + 1) * x % 289;
                    x = frac(x / 41) * 2 - 1;
                    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
                }

                void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
                {
                    float2 p = UV * Scale;
                    float2 ip = floor(p);
                    float2 fp = frac(p);
                    float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                    float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                    float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                    float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                    Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
                }

                void Unity_Posterize_float(float In, float Steps, out float Out)
                {
                    Out = floor(In / (1 / Steps)) * (1 / Steps);
                }

                void Unity_Multiply_float(float A, float B, out float Out)
                {
                    Out = A * B;
                }

                void Unity_SceneColor_float(float4 UV, out float3 Out)
                {
                    Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
                }

                void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
                {
                    RGBA = float4(R, G, B, A);
                    RGB = float3(R, G, B);
                    RG = float2(R, G);
                }

                void Unity_Blend_Negation_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = 1.0 - abs(1.0 - Blend - Base);
                    Out = lerp(Base, Out, Opacity);
                }

                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }

                // Graph Vertex
                // GraphVertex: <None>

                // Graph Pixel
                struct SurfaceDescriptionInputs
                {
                    float3 WorldSpacePosition;
                    float4 ScreenPosition;
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
                    float2 _Spherize_ACB33F36_Out_4;
                    Unity_Spherize_float(IN.uv0.xy, float2 (0.5, 0.5), float2 (10, 10), float2 (0, 0), _Spherize_ACB33F36_Out_4);
                    float2 _TilingAndOffset_D7D2890F_Out_3;
                    Unity_TilingAndOffset_float(_Spherize_ACB33F36_Out_4, float2 (1, 1), float2 (8.72, 1.32), _TilingAndOffset_D7D2890F_Out_3);
                    float _GradientNoise_6437A85B_Out_2;
                    Unity_GradientNoise_float(_TilingAndOffset_D7D2890F_Out_3, 1.08, _GradientNoise_6437A85B_Out_2);
                    float _Posterize_711F7AB0_Out_2;
                    Unity_Posterize_float(_GradientNoise_6437A85B_Out_2, 5.12, _Posterize_711F7AB0_Out_2);
                    float _Multiply_D91393F2_Out_2;
                    Unity_Multiply_float(_Posterize_711F7AB0_Out_2, 1.2, _Multiply_D91393F2_Out_2);
                    float3 _SceneColor_8AA1BAEB_Out_1;
                    Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_8AA1BAEB_Out_1);
                    float _Split_6C16775D_R_1 = _SceneColor_8AA1BAEB_Out_1[0];
                    float _Split_6C16775D_G_2 = _SceneColor_8AA1BAEB_Out_1[1];
                    float _Split_6C16775D_B_3 = _SceneColor_8AA1BAEB_Out_1[2];
                    float _Split_6C16775D_A_4 = 0;
                    float4 _SampleTexture2D_FD26B85_RGBA_0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv0.xy);
                    float _SampleTexture2D_FD26B85_R_4 = _SampleTexture2D_FD26B85_RGBA_0.r;
                    float _SampleTexture2D_FD26B85_G_5 = _SampleTexture2D_FD26B85_RGBA_0.g;
                    float _SampleTexture2D_FD26B85_B_6 = _SampleTexture2D_FD26B85_RGBA_0.b;
                    float _SampleTexture2D_FD26B85_A_7 = _SampleTexture2D_FD26B85_RGBA_0.a;
                    float4 _Combine_945C2586_RGBA_4;
                    float3 _Combine_945C2586_RGB_5;
                    float2 _Combine_945C2586_RG_6;
                    Unity_Combine_float(_Split_6C16775D_R_1, _Split_6C16775D_G_2, _Split_6C16775D_B_3, _SampleTexture2D_FD26B85_A_7, _Combine_945C2586_RGBA_4, _Combine_945C2586_RGB_5, _Combine_945C2586_RG_6);
                    float4 _Blend_B3FD711D_Out_2;
                    Unity_Blend_Negation_float4(_Combine_945C2586_RGBA_4, _SampleTexture2D_FD26B85_RGBA_0, _Blend_B3FD711D_Out_2, -1);
                    float4 _Blend_FB86CF19_Out_2;
                    Unity_Blend_Multiply_float4((_Multiply_D91393F2_Out_2.xxxx), _Blend_B3FD711D_Out_2, _Blend_FB86CF19_Out_2, 1);
                    float4 _Blend_E598BE31_Out_2;
                    Unity_Blend_Multiply_float4(float4(1.3, 1.3, 1.3, 1.3), _Blend_FB86CF19_Out_2, _Blend_E598BE31_Out_2, 1);
                    surface.Color = _Blend_E598BE31_Out_2;
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
                    float3 positionWS;
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
                    output.interp00.xyz = input.positionWS;
                    output.interp01.xyzw = input.texCoord0;
                    output.interp02.xyzw = input.color;
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
                    output.positionWS = input.interp00.xyz;
                    output.texCoord0 = input.interp01.xyzw;
                    output.color = input.interp02.xyzw;
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





                    output.WorldSpacePosition = input.positionWS;
                    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
