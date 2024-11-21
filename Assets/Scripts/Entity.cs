using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Entity : MonoBehaviour
{
	[SerializeField] private float Hp;
    public float hp {
	    get { return Hp; }
	    set
	    {
		    // Debug.Log(name);
		    if (value <= 0)
			    Die();
		    
		    Hp = value;
	    }
    }
    
    public Weapon weapon;
	[SerializeField]
	public Rigidbody rigidbody;
	
    private void Awake()
    {
	    rigidbody = GetComponent<Rigidbody>();
		if (weapon == null)
		{
			weapon = GetComponentInChildren<Weapon>();
		}
    }

	public virtual void Die()
    {
		/*
	    if (SoundManager.Instance)
			SoundManager.Instance.PlaySound(1);
		Destroy(gameObject);
		*/
    }
}
