Shader "Custom/DebugShader" 
{
	Properties
	{
		permSampler("PermTexture",2D) = "white"{}
		perlinTableSampler("PerlinTableTexture",2D) = "white"{}
		noiseResolution("NoiseReslution", int) = 10
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
		uniform sampler2D perlinTableSampler;
		uniform int noiseResolution;

		#pragma only_renderers d3d11
		

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

		float perm(float x, float y)
		{
			float2 d = floor(float2(x,y));	
			
			float2 t = float2(floor(d)/(noiseResolution-1.0f) + + float2(0.5f,0.0f));
			return floor(tex2D(permSampler,t).r*256);
			
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
			float X = floor(p.x);
			float Y = floor(p.y);
	
			float x = frac(p.x);
			float y = frac(p.y);
      
			float u = fade(x);
			float v = fade(y);
	
			float AA = perm(X, Y);
			float AB = perm(X + 1,Y );
			float BA = perm(X, Y +1);
			float BB = perm(X+1, Y+1);
	
			return lerp(v, lerp(u, grad(AA, x,y ),
								   grad(AB, x-1, y  )),
						   lerp(u, grad(BA, x  , y-1),
								   grad(BB, x-1, y-1)));

		}
		
		float4 frag(v2f i) : COLOR
		{
			
			//float n = noise2d(float2((noiseResolution - 1.0f)*i.texCoord));
			//n = 0.5f*n + 0.5f;
			
			//float4 outColor = float4(n,n,n,1.0f);
			
			float4 outColor = tex2D(permSampler, float2(i.texCoord.xy)*(noiseResolution - 1.0f)/noiseResolution);
			return outColor;
		}
		ENDCG
		} 
	
	}
Fallback "Diffuse"
}
