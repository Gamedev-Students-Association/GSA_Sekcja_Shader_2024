using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WallsGenerator : MonoBehaviour
{
    public GameObject FillerPrevab;
    public GameObject WallsPlace;
    public bool GenerateAllLayers;
    public bool GenerateSpecyficLayer; //does not work for now
    public bool WipeOut;
    public bool RecalculateRooms;

    [SerializeField]
    private List<EmptyRoom> SpaceElements;

    [SerializeField]
    private List<GameObject> GeneratedWalls;

	void Start()
	{
        GenerateInLayers();
	}

	void Update()
    {
        if (GenerateAllLayers == true)
        {
            GenerateInLayers();
            GenerateAllLayers = false;
        }
        if (GenerateSpecyficLayer == true)
		{
            GetSpaceData();
            DestroyOldWalls();
            GenerateHorizontalWalls(new List<EmptyRoom>(SpaceElements), new Vector2(SpaceElements[0].Lower.y, SpaceElements[0].Upper.y));
            GenerateSpecyficLayer = false;
		}
        if (WipeOut == true)
		{
            DestroyOldWalls();
            WipeOut = false;
		}

        if (RecalculateRooms == true)
		{
            //ResynchRooms();
            RecalculateRooms = false;
		}
    }

    /*
    public void ResynchRooms()
	{
        GetSpaceData();
        for (int i = 0; i < SpaceElements.Count; i++)
		{
            SpaceElements[i].ResynchRoom();
		}
	}
    */

    public void GenerateInLayers ()
	{
        GetSpaceData();
        DestroyOldWalls();
        SweepVerticalLayers(SpaceElements);
	}

    private void GetSpaceData()
    {
        if (SpaceElements.Count <= 0)
		{
            SpaceElements = new List<EmptyRoom>(gameObject.GetComponentsInChildren<EmptyRoom>());
        }


        for (int i = 0; i < SpaceElements.Count; i++)
        {
            //removes rooms that do not tend to form walls
            if (SpaceElements[i].GenerateWalls == false)
			{
                Debug.Log("Not Recalculating " + SpaceElements[i].name);
                SpaceElements.RemoveAt(i);
			}
			else
			{
                SpaceElements[i].RecalculateBounds();
            }
        }
    }

    public void DestroyOldWalls()
	{
        if (GeneratedWalls.Count > 0)
		{
            for (int i = 0; i < GeneratedWalls.Count; i++)
            {
                DestroyImmediate(GeneratedWalls[i]);
            }
            GeneratedWalls.Clear();
        } else
		{
            Debug.LogWarning(gameObject.name + "No generated objects to destroy, you propably tried to destroy old generation multiple times");
		}
              
	}

    //for sorting whole array set offset = array.length;
    private List<EmptyRoom> SubarraySort(List<EmptyRoom> original, float[] compareValue, int offset)
    {
        for (int i = 0; i < offset; i++)
        {
            //for (int j = offset - 1; j > i; j--)
            for (int j = 0; j < offset - 1; j++)
            {
                if (compareValue[j] > compareValue[j + 1])
                {
                    //swapBoth
                    float temp = compareValue[j + 1];
                    compareValue[j + 1] = compareValue[j];
                    compareValue[j] = temp;

                    EmptyRoom temp2 = original[j + 1];
                    original[j + 1] = original[j];
                    original[j] = temp2;
                }
            }
        }

        return original;
    }

    private List<Vector2> SplitRanges(List<Vector2> ranges, Vector2 subtractRange)
    {
        int i = 0;
        while (i < ranges.Count)
        {
            if (subtractRange.x > ranges[i].x && subtractRange.y < ranges[i].y)//split
            {
                Vector2 oldRange = ranges[i];
                ranges.RemoveAt(i);
                //has to add in reverse for keeping order
                ranges.Insert(i, new Vector2(subtractRange.y, oldRange.y));
                ranges.Insert(i, new Vector2(oldRange.x, subtractRange.x));
                return ranges;
            }
            else if (subtractRange.x <= ranges[i].x && subtractRange.y >= ranges[i].y) //swallow
            {
                ranges.RemoveAt(i);
                continue;
                //has to continue to do not itterate because our list has just shortened
            }
            else if (subtractRange.x < ranges[i].y && subtractRange.x > ranges[i].x) //subtract left
            {
                ranges[i] = new Vector2(ranges[i].x, subtractRange.x);
            }
            else if (subtractRange.y > ranges[i].x && subtractRange.y < ranges[i].y)//subtract right
            {
                ranges[i] = new Vector2(subtractRange.y, ranges[i].y);
                return ranges;
                //if we subtract right, then surely nothing further will be affected
            }

            i += 1;
        }

        return ranges;
    }

    void SweepVerticalLayers (List<EmptyRoom> AllSpaces)
	{

        //getting lower bound y
        float[] lowerY = new float[AllSpaces.Count];
        for (int l = 0; l < AllSpaces.Count; l++)
        {
            lowerY[l] = AllSpaces[l].Lower.y;
        }

        AllSpaces = SubarraySort(AllSpaces, lowerY, AllSpaces.Count);

        List<EmptyRoom> SweeplineElements = new List<EmptyRoom>();
        SweeplineElements.Clear();
        float Sweepline = AllSpaces[0].Lower.y;

        for (int i = 0; i < AllSpaces.Count || SweeplineElements.Count > 0;) //|| SweeplineElements.Count > 0
        {
            //finding lowest element from current sweepline
            float lowestEnd = 0; //so to make it work if list is empty
            if (i >= AllSpaces.Count)
			{
                //lowestEnd = SweeplineElements[0].Upper.y;
                lowestEnd = AllSpaces[i - 1].Upper.y + 1;
                //lowestEnd = 1000000;

            }
            else
			{
                lowestEnd = AllSpaces[i].Lower.y + 1;

            }

            int lowestIndex = 0;
            for (int j = 0; j < SweeplineElements.Count; j++)
            {
                if (lowestEnd > SweeplineElements[j].Upper.y)
                {
                    lowestEnd = SweeplineElements[j].Upper.y;
                    lowestIndex = j;
                }
            }

            if (i >= AllSpaces.Count || lowestEnd <= AllSpaces[i].Lower.y) //first subtract element than add
            {
                GenerateHorizontalWalls(SweeplineElements, new Vector2(Sweepline, lowestEnd));
                //subtract object
                SweeplineElements.RemoveAt(lowestIndex);
                Sweepline = lowestEnd;
            } else
			{
                //generate in layer between sweepline and added object
                GenerateHorizontalWalls(SweeplineElements, new Vector2(Sweepline, AllSpaces[i].Lower.y));
                //add element to layer list
                SweeplineElements.Add(AllSpaces[i]);
                Sweepline = AllSpaces[i].Lower.y;
                i += 1; //incrementation do not exists in all paths because we want to remove objects until all > than the one to add
            }
        }

    }

    private void GenerateHorizontalWalls(List<EmptyRoom> LayerElements, Vector2 layerHeight)
	{
        if (LayerElements.Count == 0)
		{
            Debug.LogWarning("tries to generate layer with 0 elements at:" + layerHeight);
            return;
		}
        if (layerHeight.x == layerHeight.y)
		{
            Debug.LogWarning("tries to generate 0 high layer, what doesn't make any sense");
            return;
		}
        //copy everything from generate walls

        //getting lower bound x
        float[] lowerX = new float[LayerElements.Count];
        for (int i = 0; i < LayerElements.Count; i++)
        {
            lowerX[i] = LayerElements[i].Lower.x;
        }

        //sorting before creation
        LayerElements = SubarraySort(LayerElements, lowerX, LayerElements.Count);

        //actualGenerator:
        for (int i = 0; i < LayerElements.Count; i++)
        {
            //second reversed sort
            float[] upperX = new float[i];
            for (int j = 0; j < i; j++)
            {
                upperX[j] = LayerElements[j].Upper.x;
            }

            //reverse sorting up to curent element
            LayerElements = SubarraySort(LayerElements, upperX, i);

            //creates universum for generating walls
            List<Vector2> UnfilledGaps = new List<Vector2>();
            UnfilledGaps.Add(new Vector2(LayerElements[i].Lower.z, LayerElements[i].Upper.z));
            //inner loop for reverse generating
            for (int j = i - 1; j >= 0; j--) //schould be -- because sorting sorts from min to max
            {
                //adjusting uniwersum to work only for current block

                //excluding 0 size gaps in x axis
                if (LayerElements[j].Upper.x >= LayerElements[i].Lower.x)
                {
                    UnfilledGaps = SplitRanges(UnfilledGaps, new Vector2(LayerElements[j].Lower.z, LayerElements[j].Upper.z));
                    continue;
                }

                /*
                if (UnfilledGaps.Count <= 0)
                {
                    Debug.LogWarning("WallsGen: No Gaps to fill! " + LayerElements[j]);
                }
                */

                //iterating throught generation uniwersum
                for (int k = 0; k < UnfilledGaps.Count; k++)
                {
                    Vector2 currentGap = UnfilledGaps[k]; //has to do it because must not affect variables in list
                    //excluding additional non-important walls
                    //simple check if the range is within local range of the space piece
                    if (currentGap.x <= LayerElements[j].Lower.z && currentGap.y <= LayerElements[j].Lower.z
                        || currentGap.x >= LayerElements[j].Upper.z && currentGap.y >= LayerElements[j].Upper.z)
                    {
                        //completely out of range
                        //so ignore
                        continue;
                    }
                    if (currentGap.x < LayerElements[j].Lower.z) //left merge
                    {
                        currentGap.x = LayerElements[j].Lower.z;
                    }
                    if (currentGap.y > LayerElements[j].Upper.z) //right merge
                    {
                        currentGap.y = LayerElements[j].Upper.z;
                    }

                    //generate every wall
                    Vector3 generationScale = new Vector3(
                        Mathf.Abs(LayerElements[i].Lower.x - LayerElements[j].Upper.x),
                        Mathf.Abs(layerHeight.y - layerHeight.x), //until figured out Y axis
                        Mathf.Abs(currentGap.y - currentGap.x)
                        );

                    Vector3 generationPos = new Vector3(
                        LayerElements[i].Lower.x - generationScale.x / 2,
                        layerHeight.x + generationScale.y / 2,
                        currentGap.x + generationScale.z / 2
                        );
                    GeneratedWalls.Add(Instantiate(FillerPrevab, generationPos, new Quaternion(0, 0, 0, 0), WallsPlace.transform));
                    GeneratedWalls[GeneratedWalls.Count - 1].transform.localScale = generationScale;
                }

                //subtracts newly used space element from uniwersum
                UnfilledGaps = SplitRanges(UnfilledGaps, new Vector2(LayerElements[j].Lower.z, LayerElements[j].Upper.z));

            }

        }

	}

    /*
    public void GenerateWalls()
    {
        GetSpaceData();
        DestroyOldWalls();

        //getting lower bound x
        float[] lowerX = new float[SpaceElements.Length];
        for (int i = 0; i < SpaceElements.Length; i++)
        {
            lowerX[i] = SpaceElements[i].Lower.x;
        }

        //sorting before creation
        SpaceElements = SubarraySort(SpaceElements, lowerX, SpaceElements.Length);

        //actualGenerator:
        for (int i = 0; i < SpaceElements.Length; i++)
        {
            //second reversed sort
            float[] upperX = new float[i];
            for (int j = 0; j < i; j++)
            {
                upperX[j] = SpaceElements[j].Upper.x;
            }

            //reverse sorting up to curent element
            SpaceElements = SubarraySort(SpaceElements, upperX, i);

            //creates universum for generating walls
            List<Vector2> UnfilledGaps = new List<Vector2>();
            UnfilledGaps.Add(new Vector2(SpaceElements[i].Lower.z, SpaceElements[i].Upper.z));
            //inner loop for reverse generating
            for (int j = i - 1; j >= 0; j--) //schould be -- because sorting sorts from min to max
            {
                //adjusting uniwersum to work only for current block

                //excluding 0 size gaps in x axis
                if (SpaceElements[j].Upper.x >= SpaceElements[i].Lower.x)
                {
                    //subtracts newly used space element from uniwersum
                    UnfilledGaps = SplitRanges(UnfilledGaps, new Vector2(SpaceElements[j].Lower.z, SpaceElements[j].Upper.z));
                    continue;
                }

                if (UnfilledGaps.Count <= 0)
                {
                    Debug.LogWarning("WallsGen: No Gaps to fill! " + SpaceElements[j]);
                }

                //iterating throught generation uniwersum
                for (int k = 0; k < UnfilledGaps.Count; k++)
                {
                    Vector2 currentGap = UnfilledGaps[k]; //has to do it because must not affect variables in list
                    //excluding additional non-important walls
                    //simple check if the range is within local range of the space piece
                    //if (currentGap.x > SpaceElements[j].Upper.z || currentGap.y < SpaceElements[j].Lower.z)
                    if (currentGap.x <= SpaceElements[j].Lower.z && currentGap.y <= SpaceElements[j].Lower.z
                        || currentGap.x >= SpaceElements[j].Upper.z && currentGap.y >= SpaceElements[j].Upper.z)
                    {
                        //completely out of range
                        //so ignore
                        continue;
                    }
                    else if (currentGap.x < SpaceElements[j].Lower.z) //left merge
                    {
                        currentGap.x = SpaceElements[j].Lower.z;
                    }
                    else if (currentGap.y > SpaceElements[j].Upper.z) //right merge
                    {
                        currentGap.y = SpaceElements[j].Upper.z;
                    }

                    //generate every wall
                    Vector3 generationScale = new Vector3(
                        Mathf.Abs(SpaceElements[i].Lower.x - SpaceElements[j].Upper.x),
                        SpaceElements[i].transform.lossyScale.y, //until figured out Y axis
                        Mathf.Abs(currentGap.y - currentGap.x) //as for now
                        );

                    Vector3 generationPos = new Vector3(
                        SpaceElements[i].Lower.x - generationScale.x / 2,
                        gameObject.transform.position.y,
                        currentGap.x + generationScale.z / 2
                        );
                    GeneratedWalls.Add(Instantiate(FillerPrevab, generationPos, new Quaternion(0, 0, 0, 0), gameObject.GetComponent<Transform>()));
                    GeneratedWalls[GeneratedWalls.Count - 1].transform.localScale = generationScale;
                }

                //subtracts newly used space element from uniwersum
                UnfilledGaps = SplitRanges(UnfilledGaps, new Vector2(SpaceElements[j].Lower.z, SpaceElements[j].Upper.z));

            }


            //Instantiate(FillerPrevab, generationPos,);
        }
    }
    */


    //do not has to be in order in this case to work
    //unused for now
    private List<Vector2> MergeRanges(List<Vector2> ranges, Vector2 subtractRange)
    {
        int i = 0;
        while (i < ranges.Count)
        {
            if (subtractRange.x >= ranges[i].x && subtractRange.y <= ranges[i].y)//counter swallow
            {
                return ranges;
            }
            else if (subtractRange.x < ranges[i].x && subtractRange.y > ranges[i].y) //swallow
            {
                ranges.RemoveAt(i);
                continue;
            }
            else if (subtractRange.x <= ranges[i].y && subtractRange.x >= ranges[i].x) //join left
            {
                subtractRange.x = ranges[i].x;
                ranges.RemoveAt(i);
                if (i < ranges.Count && subtractRange.y < ranges[i].x)//join only left
                                                                      //first check for not going out of range
                {
                    ranges.Insert(i, subtractRange);
                    return ranges;
                }
                continue;
            }
            else if (subtractRange.y >= ranges[i].x && subtractRange.y <= ranges[i].y)//join right
            {
                subtractRange.y = ranges[i].y;
                ranges.RemoveAt(i);
                //in this case we know we do not need to iterate more
                //continue;
                ranges.Insert(i, subtractRange);
                return ranges;
            }
            else if (subtractRange.x > ranges[i].x && i + 1 < ranges.Count && subtractRange.y < ranges[i + 1].y)//being beetween ranges
            {
                ranges.Insert(i + 1, subtractRange);
                return ranges;
            }


            i += 1;
        }
        //if not found any intersection just add at the end
        ranges.Add(subtractRange);

        return ranges;
    }

}
