
//USE ADDITIONAL DATA TO SET A NEW ORIGIN (0, 0, 0) POINT FOR LOCAL SPACE OF THE REPETING FUNCTIONS (TRANSLATE BEFORE CALCULATING)

//Infinite space creator
//*
float3 SquareSpaceLoop (float3 p, float3 s)
{

return abs(p % s) - s / 2;
//return (p + 0.5 * s) % s - 0.5 * s;
}
//*/

//returns the local position of the object
float3 SquareSpaceRound (float3 p, float3 s)
{
return round(p / s) * s;
}