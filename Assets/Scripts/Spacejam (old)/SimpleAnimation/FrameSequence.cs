using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSequence : MonoBehaviour
{
    //this actually is only a container for frames
    //whait no, this is where stuff happens
    public float playTime;
    public Sprite[] frames;
    public int loopPlace = 0;
}
