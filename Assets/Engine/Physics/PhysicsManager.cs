using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour
{
    //public List<int> Rigidbodies;
    //public GameObject testSubject;

    public void OnEnable()
    {
        Physics.ContactModifyEvent += ModificationEvent;
    }

    public void OnDisable()
    {
        Physics.ContactModifyEvent -= ModificationEvent;
    }

    //*
    public void ModificationEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
    {
        // For each contact pair, ignore the contact points that are close to origin
        foreach (var pair in pairs)
        {
            //just specyfic testing
            /*
            if (pair.bodyInstanceID != objectId)
			{
                Debug.Log("not this");
                continue;
			}
            */
            //by default do 2d binding
            for (int i = 0; i < pair.contactCount; ++i)
            {
                Vector3 direction = pair.rotation * Vector3.right;

                Quaternion rotation = pair.rotation;
                Quaternion inverseRotation = Quaternion.Inverse(pair.rotation);


                //get local point, normally it is global
                Vector3 PairPoint = pair.GetPoint(i) - pair.position;
                //rotate to match local
                PairPoint = inverseRotation * PairPoint;
                //remove z 
                PairPoint.z = 0;
                
                //unrotate
                PairPoint = rotation * PairPoint + pair.position;
                //set point without local z
                pair.SetPoint(i, PairPoint);

                //Debug.Log(PairPoint + "point ");

                //do same thing with contact normals
                Vector3 pairNormal = pair.GetNormal(i);

                pairNormal = inverseRotation * pairNormal;
                Vector2 pairNormal2D = new Vector2(pairNormal.x, pairNormal.y);
                //re normalize since we lost normalization
                pairNormal2D.Normalize();
                pairNormal = new Vector3(pairNormal2D.x, pairNormal2D.y, 0);
                //sometimes object hits only in local z, then resulted normal is (0, 0, 0), we catch this and apply simple outer normal by hand via object origins
                //TODO this method does not work well with multi-collider objects and may cause hitted object to stuck between colliders

                if (pairNormal.magnitude < 0.9f)
                {
                    Vector3 pairDist = pair.position - pair.otherPosition;
                    pairDist = rotation * pairDist;
                    pairDist.z = 0;
                    pairDist.Normalize();
                    //if this fails as well (yes, it's possible) simply give 1 in local x
                    if (pairDist.magnitude < 0.9f)
                    {
                        pairNormal = direction;
                    }
                    else
                    {
                        pairNormal = direction;
                        //does not work for now
                        //pairNormal = inverseRotation * pairDist;
                    }

                }
                else
                {
                    pairNormal = rotation * pairNormal;
                }

                pair.SetNormal(i, pairNormal);

                //Debug.Log(pairNormal + " normal");
            }
        }
    }
}
