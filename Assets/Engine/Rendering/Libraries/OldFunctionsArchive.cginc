//CoF functions: (Circle of Influance)

float SphereRadius(float4 s)
{

	return max(s.x, max(s.y, s.z)) + s.w;
}

float CubeRadius(float4 s)
{

	return length(float3(s.x, s.y, s.z)) + s.w;
}

float CylinderRadius(float4 s)
{

	return length(float2(max(s.x, s.z), s.y)) + s.w;
}

//Octahedron and its variations are actually having the same formula, besides this formula is the same as sphere formula
float OctahedronRadius(float4 s)
{

	return max(s.x, max(s.y, s.z)) + s.w;
}

//-----------------------------------------------------------
