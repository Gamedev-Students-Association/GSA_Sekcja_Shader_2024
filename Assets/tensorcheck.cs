using UnityEngine;

public class tensorcheck : MonoBehaviour
{
    Rigidbody rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(rigidbody.inertiaTensor);
        rigidbody.inertiaTensor = new Vector3(0f, 0f, 2.08f);
    }
}
