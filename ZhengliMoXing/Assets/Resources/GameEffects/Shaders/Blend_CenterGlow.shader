Shader "Hovl/Particles/Blend_CenterGlow"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _Flow("Flow", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}
        _SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
        _DistortionSpeedXYPowerZ("Distortion Speed XY Power Z", Vector) = (0,0,0,0)
        _Emission("Emission", Float) = 2
        _Color("Color", Color) = (0.5,0.5,0.5,1)
        _Opacity("Opacity", Range( 0 , 3)) = 1
        [Toggle]_Usecenterglow("Use center glow?", Float) = 0
        [MaterialToggle] _Usedepth ("Use depth?", Float ) = 0
        _Depthpower ("Depth power", Float ) = 1
        [Enum(Cull Off,0, Cull Front,1, Cull Back,2)] _CullMode("Culling", Float) = 0
        [HideInInspector] _texcoord( "", 2D ) = "white" {}
    }

    Category
    {
        SubShader
        {
            /*
            Shader
            Shader
            Shader
            Shader
            Shader
            Shader
            */
            Tags
            {
                "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            Cull[_CullMode]
            Lighting Off
            ZWrite Off
            ZTest LEqual

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_particles
                #pragma multi_compile_fog
                #include "UnityShaderVariables.cginc"
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float4 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float4 texcoord : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    #ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
                    #endif
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                #if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
                #else
                uniform sampler2D_float _CameraDepthTexture;
                #endif

                //Don't delete this comment
                // uniform sampler2D_float _CameraDepthTexture;
                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform float _Usecenterglow;
                uniform float4 _SpeedMainTexUVNoiseZW;
                uniform sampler2D _Flow;
                uniform float4 _DistortionSpeedXYPowerZ;
                uniform float4 _Flow_ST;
                uniform sampler2D _Mask;
                uniform float4 _Mask_ST;
                uniform sampler2D _Noise;
                uniform float4 _Noise_ST;
                uniform float4 _Color;
                uniform float _Emission;
                uniform float _Opacity;
                uniform fixed _Usedepth;
                uniform float _Depthpower;
                float _a;
                v2f vert(appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    v.vertex.xyz += float3(0, 0, 0);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    #ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
                    #endif
                    o.color = v.color;
                    o.texcoord = v.texcoord;
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float lp = 1;
                    #ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate ((sceneZ-partZ) / _Depthpower);
						lp *= lerp(1, fade, _Usedepth);
						i.color.a *= lp;
                    #endif

                    const float2 appendResult21 = (float2(_SpeedMainTexUVNoiseZW.x, _SpeedMainTexUVNoiseZW.y));
                    const float2 uv0_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                    const float2 panner107 = (1.0 * _Time.y * appendResult21 + uv0_MainTex);
                    const float2 appendResult100 = (float2(_DistortionSpeedXYPowerZ.x, _DistortionSpeedXYPowerZ.y));
                    float3 uv0_Flow = i.texcoord.xyz;
                    uv0_Flow.xy = i.texcoord.xy * _Flow_ST.xy + _Flow_ST.zw;
                    const float2 panner110 = (1.0 * _Time.y * appendResult100 + (uv0_Flow).xy);
                    const float2 uv_Mask = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
                    const float4 tex2DNode33 = tex2D(_Mask, uv_Mask);
                    const float flowPower102 = _DistortionSpeedXYPowerZ.z;
                    float4 tex2DNode13 = tex2D(_MainTex, (panner107 - (((tex2D(_Flow, panner110) * tex2DNode33)).rg * flowPower102)));
                    const float2 appendResult22 = (float2(_SpeedMainTexUVNoiseZW.z, _SpeedMainTexUVNoiseZW.w));
                    const float2 uv0_Noise = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
                    const float2 panner108 = (1.0 * _Time.y * appendResult22 + uv0_Noise);
                    float4 tex2DNode14 = tex2D(_Noise, panner108);
                    const float3 temp_output_78_0 = ((tex2DNode13 * tex2DNode14 * _Color * i.color)).rgb;
                    const float4 temp_cast_0 = ((1.0 + (uv0_Flow.z - 0.0) * (0.0 - 1.0) / (1.0 - 0.0))).xxxx;
                    const float4 clampResult38 = tex2DNode33 - temp_cast_0;
                    float4 clampResult40 = clamp((tex2DNode33 * clampResult38), float4(0, 0, 0, 0), float4(1, 1, 1, 1));
                    const float4 appendResult87 = (float4((lerp(temp_output_78_0, (temp_output_78_0 * (clampResult40).rgb), _Usecenterglow) * _Emission),
                                                          (tex2DNode13.a * tex2DNode14.a * _Color.a * i.color.a * _Opacity)));
                    fixed4 col = appendResult87;
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    return col;
                }
                ENDCG
            }
        }
    }
}