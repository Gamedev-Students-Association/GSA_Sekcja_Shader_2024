using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody rigidbody;
    [HideInInspector]
    public Weapon weapon;

    //offset is taken from transform of the projectile
    [Tooltip("relative to spawn direction")]
    public Vector3 positionOffset; //offset is located in transform
    [Tooltip("rotates offset around spawn point")]
    public Vector3 angularOffset; //offset located in transform
    [Tooltip("cooldown the weapon takes after spawning proj, not cooldown of proj itself")]
    public float cooldown;
    public float speed;
    public float damage;
    [Tooltip("projectile range in time units")]
    public float lifetime;
    [Tooltip("projectile range in distance units")]
    public float range;
    [Tooltip("defines how often continous damage can take place")]
    public float damageCooldown = 0; //not implemented
    [Tooltip("proj destroyed upon hitting target (must be a succesful hit)")]
    public bool destroyOnCollide = true;
    [Tooltip("proj will collide with every collider, not only entities")]
    public bool collideWithEnviroment = true;
    [Tooltip("projectile can hit each target only once")]
    public bool oneHit = true; //not implemented
    [Tooltip("can projectile damage it's weapon owner?")]
    public bool canHitOwner = false;
    [Tooltip("projectile will spawn as child of it's weapon")]
    public bool isChild = false;

    private Vector3 lastPos;
    private float distFromSpawn;
    
	private void Awake()
	{
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }
	private void Start()
    {
        Destroy(gameObject, lifetime);
        //should give more flexibility?
        rigidbody.AddForce(gameObject.transform.forward * speed, ForceMode.Impulse);

        lastPos = gameObject.transform.position;
    }

	private void Update()
	{
        distFromSpawn += Vector3.Distance(lastPos, gameObject.transform.position);
        lastPos = gameObject.transform.position;

        if (distFromSpawn >= range)
		{
            Destroy(gameObject);
		}
    }

	//expand to different types of triggers
	private void OnTriggerEnter(Collider other)
    {
        Entity target = other.gameObject.GetComponentInParent<Entity>();
        if (target)
        {
                bool hit;
                if (target != null)
				{
                    weapon.OnTargetHit(target, this, out hit);
                    if (!hit)
                    {
                        return;
                    }
                    target.hp -= damage;
                    if (destroyOnCollide)
                        Destroy(gameObject);
                }
        }
    }
}
