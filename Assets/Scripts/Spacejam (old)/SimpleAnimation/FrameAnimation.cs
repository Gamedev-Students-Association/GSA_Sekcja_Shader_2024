using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameAnimation : MonoBehaviour
{
    public SpriteRenderer target;
    public FrameSequence[] FrameSequences;

    private FrameSequence currentSequence;

    private float waitTime;
    private int curFrame;

    // Start is called before the first frame update
    void Start()
    {
        FrameSequences = gameObject.GetComponentsInChildren<FrameSequence>();
        currentSequence = FrameSequences[0];
    }

    // Update is called once per frame
    void Update()
    {

        float frameTime = currentSequence.playTime / (float)currentSequence.frames.Length;
        //Debug.Log(frameTime);

        //Debug.Log(curFrame);
        if (curFrame >= currentSequence.frames.Length)
        {
            curFrame = currentSequence.loopPlace;
        }

        if (waitTime >= frameTime)
		{
            if (target.sprite != currentSequence.frames[curFrame])
			{
                target.sprite = currentSequence.frames[curFrame];
            }

            waitTime -= frameTime;
            curFrame += 1;
		}

        waitTime += frameTime;
        /*
        int curFrame = 0;
        float frameTime = currentSequence.playTime / currentSequence.frames.Length;
        float accTime = frameTime;
        while (accTime <= waitTime)
		{
            accTime += frameTime;
            ++curFrame;
		}
        //int curFrame = waitTime / currentSequence.playTime;
        if (target.sprite != currentSequence.frames[curFrame])
		{
            target.sprite = currentSequence.frames[curFrame];
		}

        waitTime += Time.deltaTime;
        if (waitTime > currentSequence.playTime)
		{
            waitTime -= currentSequence.playTime;

        }
        */

    }

    public void PlaySequence(FrameSequence seq, bool overrideSeq)
	{
        if (!overrideSeq)
        {
            if (seq == currentSequence)
            {
                return;
            }
        }

        waitTime = 0;
        curFrame = 0;
        currentSequence = seq;
	}

    public void PlaySequence(string name, bool overrideSeq)
	{
        for (int i = 0; i < FrameSequences.Length; ++i)
		{
            if (name == FrameSequences[i].name)
			{
                PlaySequence(FrameSequences[i], overrideSeq);
			}
		}
	}
}
