using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDisplay : MonoBehaviour
{
    public Vector3 rotSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localRot = gameObject.transform.localRotation.eulerAngles;
        gameObject.transform.localRotation = Quaternion.Euler(localRot.x + rotSpeed.x, localRot.y + rotSpeed.y, localRot.z + rotSpeed.z);
    }
}
