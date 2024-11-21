//Using Already existing functions
/*
float3 PerlinNoise (float3 p, float4 s)
{
//return noise(round(p / s) * s);
return float3(noise(round(p.x / s.x) * s.x), noise(round(p.y / s.y) * s.y), noise(round(p.z / s.z) * s.z));
}
*/

//Bare representation of gradient noise
float4 NoiseTex (float3 p, float4 s)
{
//p /= s;

float result = GradientNoise(p / s.xyz);

return float4(result, result, result, 1);
}

//Based on distance fields
float4 BrickTexture (float3 p, float4 s)
{

p = SquareSpaceLoop(p.xyz, s.xyz);

float result = CubeShape(p, float4(s.xyz / s.w / 2, 0));

//result = (sin(result) + 1) / 2;

if (result <= 0)
{
//result = 0.5;
//result = 0.5 - abs(result) / min(s.x / s.w / 2, min(s.y / s.w / 2, s.z / s.w / 2)) / 2;
result = 0.5 - abs(result) / abs( CubeShape(float3(0, 0, 0), float4(s.xyz / s.w / 2, 0)) ) / 2;
} else
{
//result = 1;
result = 0.5 + result / (length(s.xyz) - length(s.xyz / s.w / 2)) / 2;
}

return float4(result, result, result, 0);
}

//Noise based
float4 WoodTex (float3 p, float4 s)
{
p = Rotate3D(p, float3(GradientNoise(p / s.xyz) * 360, GradientNoise(p / s.xyz) * 360, GradientNoise(p / s.xyz) * 360));
//p += GradientNoise(p / s);
//p %= GradientNoise(p / s);
//p *= GradientNoise(p / s);

//p = round(p / s) * s;
float result = p.x + p.y + p.z;
//float result = length(p);
result *= s.w;

result = (sin(result) + 1) / 2;

return float4(result, result, result, 1);
}

//Work inprogress
float4 MetalText (float3 p, float4 s)
{

//p = Rotate3D(p, PerlinNoise(p, s) * 360);
//p = Rotate3D(p, noise(round(p / s) * s) * 360);

float result = p.x + p.y + p.z;

result = (sin(result)+ 1) / 2;
return float4(result, result, result, 0);
}

//SimpleCellularNoise
float4 RubberTex (float3 p, float4 s)
{
p = abs(p);
float result = 0;

float distance = 1;
//float3 newP = float3(0, 0, 0);

int3 i = int3(0, 0, 0);
//*
for (; i.x < 3; i.x++)
{

//*
for (; i.y < 3; i.y++)
{

for (; i.z < 3; i.z++)
{

float3 newP = float3(round(p.x / s.x + i.x - 1) * s.x, round(p.y / s.y + i.y - 1) * s.y, round(p.z / s.z + i.z - 1) * s.z);
float3 random = float3(PseudoRandom(newP.y + newP.z), PseudoRandom(newP.x + newP.z), PseudoRandom(newP.x + newP.y));
//float3 random = float3();
newP += float3((random.x - 0.5) * s.x * 2, (random.y - 0.5) * s.y * 2, (random.z - 0.5) * s.z * 2);

distance = min(length(p - newP), distance);
}
i.z = 0;

}
i.y = 0;

}

//distance *= 100;
result = (sin(distance) + 1) / 2;
return float4(result, result, result, 0);
}

//REGULAR TEXTURES (NO NOISE)

float4 SquarePattern(float3 p, float4 s)
{
p = SquareSpaceLoop(p.xyz, s.xyz);

p = abs(p);
return max(p.x / s.x / 2, max(p.y / s.y / 2, p.z / s.z / 2));
}


float4 StripesBoxTexture (float3 p)
{
p = abs(p);
float result = (sin(p.x + p.y + p.z) + 1) / 2;

return float4(result, result, result, 0);
}

float4 StripesSphereTexture (float3 p)
{

float result = (sin(length(p)) + 1) / 2;

return result;
}

/*
float4 HeightIndicator(float3 p)
{
	return float4((p.y + 10) / 20, (p.y + 10) / 20, (p.y + 10) / 20);
}
/*/

float4 SpaceAce(float3 p, float4 s)
{
	float4 result = float4(0, 0, 0, 0);
	p = abs(p);
	result.xyz = abs(p.xyz % s.xyz - s.xyz / 2) * 2 / s.xyz;
	//result.x = abs(p.x % s.x - s.x / 2) * 2 / s.x;
	//result.y = abs(p.y % s.y - s.y / 2) * 2 / s.y;
	//result.z = abs(p.z % s.z - s.z / 2) * 2 / s.z;

	return result;
}

float4 Triangle(float3 p, float4 s)
{
	float4 result = float4(1, 0, 0, 0);
	float lineSize = 0.05;
	if (abs(p.x) < lineSize && p.y > -lineSize)
	{
		return float4(0, 0, 1, 0);
	}
	else if (abs(p.x - p.y) < lineSize && p.x > -lineSize && p.y > -lineSize)
	{
		return float4(0, 0, 1, 0);
	}
	else if (abs(p.x - p.z) < lineSize && p.x > -lineSize && p.z > -lineSize)
	{
		return float4(0, 0, 1, 0);
	}

	return result;
}

