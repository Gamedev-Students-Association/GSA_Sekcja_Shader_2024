//SimpleConnections

float Add(float dist1, float dist2)
{
return min(dist1, dist2);
}

float Subtract(float dist1, float dist2)
{
return max(dist1, -dist2);
}

float Intersect(float dist1, float dist2)
{
return max(dist1, dist2);
}

float Fill(float dist1, float dist2, float factor)
{

return dist1 + dist2 - factor;
}

float SmoothAdd(float dist1, float dist2, float factor)
{
float h = max(factor - abs(dist1 - dist2), 0) / factor;
return min(dist1, dist2) - h * h * h * factor * 1 / 6.0f;
}

float MathAdd(float dist1, float dist2)
{
return dist1 + dist2;
}