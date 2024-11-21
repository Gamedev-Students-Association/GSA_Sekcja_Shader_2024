

float3 ToDirection (float3 v)
{
float vMax = max(max(abs(v.x), abs(v.y)), abs(v.z));
//v = v / vMax;

return v / abs(vMax);
}

//Immidently add, it is not a direction, it is an already calculated distance in float3
float3 DistanceToVector (float dist, float3 dir)
{
//float3 power = float3(v.x * v.x);
float root = dist * dist / (dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);

return dir * root;
}

//v is regular vector, rot is in deegres
float3 Rotate3D(float3 v, float3 rot) //using deegres
{
float3 rad = 0.0174532924 * rot;
float3 Cos = cos(rad);
float3 Sin = sin(rad);

//x axis
v = float3(v.x, Cos.x * v.y + Sin.x * v.z, -Sin.x * v.y + Cos.x * v.z);
//y axis
v = float3(Cos.y * v.x - Sin.y * v.z, v.y, Sin.y * v.x + Cos.y * v.z);
//z axis
v = float3(Cos.z * v.x + Sin.z * v.y, -Sin.z * v.x + Cos.z * v.y, v.z);

return v;
}

//This can be used as Rotate3D with a 1, 1, 1 Starting Vector (as a property)
float3 DegreesToVector(float3 rot)
{
float3 rad = 0.0174532925 * rot;
float3 Cos = cos(rad);
float3 Sin = sin(rad);

float3 v = float3(1, 1, 1);
//x axis
v = float3(v.x, Cos.x * v.y + Sin.x * v.z, -Sin.x * v.y + Cos.x * v.z);
//y axis
v = float3(Cos.y * v.x - Sin.y * v.z, v.y, Sin.y * v.x + Cos.y * v.z);
//z axis
v = float3(Cos.z * v.x + Sin.z * v.y, -Sin.z * v.x + Cos.z * v.y, v.z);

return v;
}

float3 VectorToDegrees(float3 v)
{
float pi = 3.14;
float3 degrees = float3(
atan2(v.y, v.z) * 180 / pi,
atan2(v.x, v.z) * 180 / pi,
atan2(v.x, v.y) * 180 / pi);
return degrees;
}
//In deegress
float3 RotationDistance (float3 Rot1, float3 Rot2)
{
Rot1 %= 360;
Rot2 %= 360;

float3 result = Rot1 - Rot2;
result = float3(abs(result.x), abs(result.y), abs(result.z));
return result;
}

float Distance (float3 Cord)
{
return sqrt(Cord.x * Cord.x + Cord.y * Cord.y + Cord.z * Cord.z);
}

//result is direction
float3 LookAt(float3 Sp, float3 Rp)
{
float3 LookDir = Rp - Sp;
LookDir /= max(LookDir.x, max(LookDir.y, LookDir.z));

return LookDir;
}

//NoiseHERE
float3 hash( float3 p ){ // replace this by something better
	p = float3( dot(p,float3(127.1,311.7, 74.7)),
			  dot(p,float3(269.5,183.3,246.1)),
			  dot(p,float3(113.5,271.9,124.6)));
	return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}


float GradientNoise( in float3 p ){
    float3 i = floor( p );
    float3 f = frac( p );
	float3 u = f*f*(3.0-2.0*f);
    return lerp( lerp( lerp( dot( hash( i + float3(0.0,0.0,0.0) ), f - float3(0.0,0.0,0.0) ), 
                          dot( hash( i + float3(1.0,0.0,0.0) ), f - float3(1.0,0.0,0.0) ), u.x),
                     lerp( dot( hash( i + float3(0.0,1.0,0.0) ), f - float3(0.0,1.0,0.0) ), 
                          dot( hash( i + float3(1.0,1.0,0.0) ), f - float3(1.0,1.0,0.0) ), u.x), u.y),
                lerp( lerp( dot( hash( i + float3(0.0,0.0,1.0) ), f - float3(0.0,0.0,1.0) ), 
                          dot( hash( i + float3(1.0,0.0,1.0) ), f - float3(1.0,0.0,1.0) ), u.x),
                     lerp( dot( hash( i + float3(0.0,1.0,1.0) ), f - float3(0.0,1.0,1.0) ), 
                          dot( hash( i + float3(1.0,1.0,1.0) ), f - float3(1.0,1.0,1.0) ), u.x), u.y), u.z );
}

float PseudoRandom (float Seed)
{

//Classic Random value
float result = frac(sin( dot(Seed,float3(12.9898,78.233,45.5432) )) * 43758.5453);
return result;
}

float2 PseudoRandom2(float2 p) {
return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453);
}

float AlphaBlend (float bgA, float adA)
{
float result;
result = 1 - (1 - adA) * (1 - bgA);
return result;
}

float4 ColorBlend(float4 bgCol, float4 adCol)
{
float4 result = float4(1, 1, 1, 1);
result.a = 1 - (1 - adCol.a) * (1 - bgCol.a);
result.rgb = adCol.rgb * adCol.a / result.a + bgCol.rgb * bgCol.a * (1 - adCol.a) / result.a;

return result;
}

float4 ColorToHue (float4 col)
{
float4 result = float4(0, 0, 0, col.a);
//value //brightness
result.y = min(col.x, min(col.y, col.z));
//saturation //intensity
result.z = max(col.x, max(col.y, col.z)) - result.y;

if (col.x - (result.y + result.z) == 0)
{
	result.x = 4;

	if (col.z > col.y)
	{
	result.x += (col.z - result.y) / result.z + 1;
	} else
	{
	result.x += (1 - (col.y - result.y) / result.z);
	}

} else if (col.y - (result.y + result.z) == 0)
{
	result.x = 2;

	if (col.r > col.z)
	{
	result.x += (col.r - result.y) / result.z + 1;
	} else
	{
	result.x += (1 - (col.z - result.y) / result.z);
	}
} else
{
	result.x = 0;

	if (col.y > col.x)
	{
	result.x += (col.y - result.y) / result.z + 1;
	} else
	{
	result.x += (1 - (col.x - result.y) / result.z);
	}
}

result.x /= 6;
return result;

}

float4 HueToColor (float4 hue)
{
hue.x = abs(hue.x) % 1;
hue.x *= 6;

float4 result = float4(0, 0, 0, hue.w);

if (hue.x >= 4)
{
	result.x = hue.z;

	if ((hue.x - 4) <= 1)
	{
	result.y = (1 - (hue.x - 4)) * hue.z;
	} else
	{
	result.z = (hue.x - 5) * hue.z;
	}
} else if (hue.x <= 2)
{
	result.z = hue.z;

	if (hue.x <= 1)
	{
	result.x = (1 - hue.x) * hue.z;
	} else
	{
	result.y = (hue.x - 1) * hue.z;
	}
} else
{
	result.y = hue.z;

	if ((hue.x - 2) <= 1)
	{
	result.z = (1 - (hue.x - 2)) * hue.z;
	} else
	{
	result.x = (hue.x - 3) * hue.z;
	}
}

result.xyz += hue.y;

return result;

}