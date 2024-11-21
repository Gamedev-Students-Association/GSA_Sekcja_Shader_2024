using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicActivator : MonoBehaviour
{
    public AudioClip clip;
    void Start()
    {
        GameObject.FindObjectOfType<MusicSwitch>().ChangeAudio(clip);
    }

}
