Shader "Custom RP/Unlit" {
	Properties {
		_BaseMap("Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0 //clipping勾选框
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
	}
	
	SubShader {
		
		Pass {
			Blend [_SrcBlend] [_DstBlend] //混合模式
			ZWrite [_ZWrite] //深度写入模式
			
			HLSLPROGRAM //HLSL代码
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex UnlitPassVertex // 别名
			#pragma fragment UnlitPassFragment
			#include "./UnlitPass.hlsl"
			ENDHLSL
		}
	}
}