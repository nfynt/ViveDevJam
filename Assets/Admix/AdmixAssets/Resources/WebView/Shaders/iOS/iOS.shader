﻿Shader "Hidden/iOS WebView" {
	Properties{
	  _MainTex("Base (RGB)", 2D) = "white" {}
	  _VideoCutoutRect("VideoCutoutRect", Vector) = (0, 0, 0, 0)
	  _CropRect("CropRect", Vector) = (0, 0, 0, 0)
	  _OverrideStereoToMono("Override Stereo To Mono", Float) = 0
	  [KeywordEnum(None, TopBottom, LeftRight)] _StereoMode("Stereo mode", Float) = 0
	  [Toggle(FLIP_X)] _FlipX("Flip X", Float) = 0
	  [Toggle(FLIP_Y)] _FlipY("Flip Y", Float) = 0
	}

		SubShader{
		  Pass {
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

			Lighting Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			  #pragma multi_compile ___ _STEREOMODE_TOPBOTTOM _STEREOMODE_LEFTRIGHT
			  #pragma multi_compile ___ FLIP_X
			  #pragma multi_compile ___ FLIP_Y
			  #pragma vertex vert
			  #pragma fragment frag
			  #include "UnityCG.cginc"

			  struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			  };

			  struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			  };

			  sampler2D _MainTex;
			  float4 _MainTex_ST;
			  float _OverrideStereoToMono;

			  v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float2 untransformedUV = v.uv;

				#ifdef FLIP_X
				  untransformedUV.x = 1.0 - untransformedUV.x;
				#endif // FLIP_X
				#ifdef FLIP_Y
				  untransformedUV.y = 1.0 - untransformedUV.y;
				#endif // FLIP_Y

				#ifdef _STEREOMODE_TOPBOTTOM
				  untransformedUV.y *= 0.5;
				  if (unity_StereoEyeIndex == 1 && _OverrideStereoToMono != 1.0) {
					untransformedUV.y += 0.5;
				  }
				#endif // _STEREOMODE_TOPBOTTOM
				#ifdef _STEREOMODE_LEFTRIGHT
				  untransformedUV.x *= 0.5;
				  if (unity_StereoEyeIndex != 0 && _OverrideStereoToMono != 1.0) {
					untransformedUV.x += 0.5;
				  }
				#endif // _STEREOMODE_LEFTRIGHT

				o.uv = TRANSFORM_TEX(untransformedUV, _MainTex);

				return o;
			  }

			  float4 _VideoCutoutRect;
			  float4 _CropRect;

			  fixed4 frag(v2f i) : SV_Target {

				fixed4 col = tex2D(_MainTex, i.uv);
				float cutoutWidth = _VideoCutoutRect.z;
				float cutoutHeight = _VideoCutoutRect.w;

				#ifdef FLIP_Y
				  float nonflippedY = 1.0 - i.uv.y;
				#else
				  float nonflippedY = i.uv.y;
				#endif // FLIP_Y

				#ifdef FLIP_X
				  float nonflippedX = i.uv.x;
				#else
				  float nonflippedX = 1.0 - i.uv.x;
				#endif // FLIP_X

				  // Make the pixels transparent if they fall within the video rect cutout and the they're black.
				  // Keeping non-black pixels allows the video controls to still show up on top of the video.
				  bool pointIsInCutout = cutoutWidth != 0.0 &&
										 cutoutHeight != 0.0 &&
										 nonflippedX >= _VideoCutoutRect.x &&
										 nonflippedX <= _VideoCutoutRect.x + cutoutWidth &&
										 nonflippedY >= _VideoCutoutRect.y &&
										 nonflippedY <= _VideoCutoutRect.y + cutoutHeight;

				  if (pointIsInCutout && all(col == float4(0.0, 0.0, 0.0, 1.0))) {
					col = float4(0.0, 0.0, 0.0, 0.0);
				  }

				  float cropWidth = _CropRect.z;
				  float cropHeight = _CropRect.w;
				  bool pointIsOutsideOfCrop = cropWidth != 0.0 &&
											  cropHeight != 0.0 &&
											  (nonflippedX < _CropRect.x || nonflippedX > _CropRect.x + cropWidth || nonflippedY < _CropRect.y || nonflippedY > _CropRect.y + cropHeight);
				  if (pointIsOutsideOfCrop) {
					col = float4(0.0, 0.0, 0.0, 0.0);
				  }
				  return col;
				}
			  ENDCG
			}
	  }
		  Fallback "Unlit/Texture"
}