Shader "Custom/PerlinShader" 
{
	Properties
	{
		permSampler("PermTexture",2D) = "white"{}
		gradSampler("GradTexture",2D) = "white"{}
	}
	SubShader 
	{
		Tags{"RenderType" = "Opaque"}
		Pass
		{
		CGPROGRAM



		#pragma vertex vert
		#pragma fragment frag
		#include"UnityCG.cginc"
		#pragma target 3.0
		uniform sampler2D permSampler;
		uniform sampler2D gradSampler;
		#pragma only_renderers d3d11
		//#pragma profileoption MaxTexIndirections=64

		struct vertexInput
		{
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD0;
		};
		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 texCoord : TEXCOORD0;
		};

		
		v2f vert(vertexInput input)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, input.vertex);
			o.texCoord = input.texcoord.xy;
			return o;
		}

		
		float2 fade(float2 t)
		{
			return t*t*t*(t*(t*6-15)+10);
		}

		float perm(float d)
		{
			d = floor(d);	
			//float2 t = float2(fmod(d, 16.0),floor(d/16.0))/15.0;
			float2 t = float2((floor(d)+0.5f)/255.0f, 0.0f);
			return floor(tex2D(permSampler,t).r *255);
		}			

		float grad(int t, float x, float y)
		{
			float h = fmod((float)t, 16.0);
			float u = h<8.0 ? x : y;
			float v = h<4.0 ? y : x;
			float a = fmod(h, 2.0) == 0 ? u : -u; h /=2.0;
			float b = fmod(h, 2.0) == 0 ? v : -v;
			return a+b;
		}
		float lerp(float t, float a, float b)
		{
			return a+t*(b-a);
		}
		float noise2d(float2 p)
		{
			float X = fmod(p.x, 256);
			float Y = fmod(p.y, 256);
	
			float x = frac(p.x);
			float y = frac(p.y);
      
			
			// float x = p.x - floor(p.x);
			//float y = p.y - floor(p.y);
			
			float u = fade(x);
			float v = fade(y);

			float A	= perm(X  	)+Y;
			float B	= perm(X+1	)+Y;

			float AA	= perm(A	);
			float AB	= perm(A+1	); 	
			float BA	= perm(B	);
			float BB	= perm(B+1	);
	
			return lerp(v, lerp(u, grad(perm(AA  ), x,y ),
								   grad(perm(BA  ), x-1, y  )),
						   lerp(u, grad(perm(AB  ), x  , y-1),
								   grad(perm(BB  ), x-1, y-1)));

		}
		
		float4 frag(v2f i) : COLOR
		{
			
			float n = noise2d(float2(5.0f*i.texCoord.xy));
			n = 0.5f*n + 0.5f;
			
			float4 outColor = float4(n,n,n,1.0f);
			

			return outColor;
		}
		ENDCG
		} 
	
	}
Fallback "Diffuse"
}
