
Shader "GLTF/SmoothToRough" {
	Properties{
		_MetallicGlossMap("Texture", 2D) = "white" {}
		_GlossScale("Smoothness", Float) = 0.5
		_FlipY("Flip texture Y", Int) = 0
	}

	SubShader {
		 Pass {
			 CGPROGRAM

			 #pragma vertex vert
			 #pragma fragment frag
			 #include "UnityCG.cginc"

			 struct vertInput {
			 float4 pos : POSITION;
			 float2 texcoord : TEXCOORD0;
			 };

			 struct vertOutput {
			 float4 pos : SV_POSITION;
			 float2 texcoord : TEXCOORD0;
			 };

			 sampler2D _MetallicGlossMap;
			 int _FlipY;
			 float _GlossScale;

			 vertOutput vert(vertInput input) {
				 vertOutput o;
				 o.pos = UnityObjectToClipPos(input.pos);
				 o.texcoord.x = input.texcoord.x;
				 if(_FlipY == 1)
					o.texcoord.y = 1.0 - input.texcoord.y;
				 else
					o.texcoord.y = input.texcoord.y;

				 return o;
			 }

			 float4 frag(vertOutput output) : COLOR {
			 	float4 original = tex2D(_MetallicGlossMap, output.texcoord);
				original.w = 1.0 - original.w * _GlossScale;
				return original;
			 }

			ENDCG
		}
	}
}
