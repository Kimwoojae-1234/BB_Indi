// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ShockWave" {
//Shader "ShockWave" {
	Properties {
		_MainTex ("", 2D) = "white" {}
		_CenterX ("CenterX", Range(-1,2)) = 0.5
		_CenterY ("CenterY", Range(-1,2)) = 0.5
		_Radius ("Radius", Range(-0.20,1)) = 0.2
		_Amplitude ("Amplitude", Range(-10,10)) = 0.05

		_ScreenRatio ("ScreenRatio", Float) = 1 //Width/Height
	}
	 
	SubShader {
	 
		ZTest Always Cull Off ZWrite Off Fog { Mode Off } //Rendering settings
	 
	 	Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			//we include "UnityCG.cginc" to use the appdata_img struct
			
			float _CenterX;
			float _CenterY;
			float _Radius;
			float _Amplitude;
			float _ScreenRatio;

			struct v2f {
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};
	   
			//Our Vertex Shader
			v2f vert (appdata_img v){
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
				return o;
			}

			sampler2D _MainTex; //Reference in Pass is necessary to let us use this variable in shaders

			//Our Fragment Shader
			fixed4 frag (v2f i) : COLOR
			{

//				float2 diff=float2(i.uv.x-_CenterX,i.uv.y-_CenterY);
				float2 diff=float2(i.uv.x-_CenterX, (i.uv.y-_CenterY) * _ScreenRatio); 

//				float2 diff=float2(i.uv.x-_CenterX,i.uv.y-_CenterY);
//
//				if (_ScreenRatio >= 1)
//				{
//					diff.y *= _ScreenRatio;
//				}
//				else
//				{
//					diff.x *= (1/_ScreenRatio);
//				}


				float dist=sqrt(diff.x*diff.x+diff.y*diff.y);

				float2 uv_displaced = float2(i.uv.x,i.uv.y);

				float wavesize=0.2f;
				if (dist>_Radius) 
				{
					if (dist<_Radius+wavesize) 
					{
						float angle=(dist-_Radius)*2*3.141592654/wavesize;
						float cossin=(1-cos(angle))*0.5;
						uv_displaced.x-=cossin*diff.x*_Amplitude/dist;
						uv_displaced.y-=cossin*diff.y*_Amplitude/dist;
					}
				}
				
				
				fixed4 orgCol = tex2D(_MainTex, uv_displaced); //Get the orginal rendered color
				return orgCol;
			}
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
//Raw  WaveExploPostProcessing.cs
//using UnityEngine;
//using System;
//using System.Collections.Generic;

// You call it like this : WaveExploPostProcessing.Get().StartIt(myVector2Position);

//public class WaveExploPostProcessing : MonoBehaviour {
//	public Material mat;
//	public WaveExploPostProcessing() {
//		mat=new Material(Shader.Find("Custom/WaveExplo"));
//	}
//	protected float _radius;
//	public float radius {
//		get { return _radius; }
//		set { 
//			_radius=value;
//			mat.SetFloat("_Radius",_radius);
//		}
//	}
//	public void StartIt(Vector2 center) {
//		mat.SetFloat("_CenterX",(center.x+Futile.screen.halfWidth)/Futile.screen.width);
//		mat.SetFloat("_CenterY",(center.y+Futile.screen.halfHeight)/Futile.screen.height);
//		radius=0f;
//
//		GoTweenConfig config=new GoTweenConfig().floatProp("radius",1.34f);
//		//config.easeType=GoEaseType.ExpoOut;
//		config.onComplete(HandleComplete);
//		Go.to(this,0.6f,config);
//	}
//	protected void HandleComplete(AbstractGoTween tween) {
//		Destroy(this);
//	}
//	static WaveExploPostProcessing _postProcessing;
//	static public WaveExploPostProcessing Get() {
//		WaveExploPostProcessing postProcessing=Futile.instance.camera.gameObject.AddComponent<WaveExploPostProcessing>(); 
//		return postProcessing;
//	}
//	void OnRenderImage(RenderTexture src, RenderTexture dest) {
//		Graphics.Blit(src, dest, mat);
//	}
//}
