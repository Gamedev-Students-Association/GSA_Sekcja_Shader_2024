using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSwitch : MonoBehaviour
{
    public bool isFirst;
    public AudioSource first;
    public AudioSource second;
    public float FadeSpan;

    private float waitTime;
    public void ChangeAudio(AudioClip clip)
    {
        first.clip = clip;
        first.Stop();
        first.Play();
        first.loop = true;
        second.clip = clip;
        second.Stop();
        second.Play();
        second.loop = true;
        isFirst = !isFirst;

        waitTime = 0;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        waitTime += Time.deltaTime;

        if (isFirst)
		{
            second.volume = waitTime / FadeSpan;
            first.volume = 1 - waitTime / FadeSpan;
        }
        else
		{
            first.volume = waitTime / FadeSpan;
            second.volume = 1 - waitTime / FadeSpan;
        }
    }
}
