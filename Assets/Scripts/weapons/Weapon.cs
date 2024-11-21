using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected List<Projectile> projectiles;
    //[SerializeField] protected float cooldown; //proj schould be heaving cooldown

    [SerializeField] protected float currentCooldown;

    [SerializeField] protected int combo;

    [HideInInspector]
    public Entity owner;

    protected bool needsReload = false;
    private void Awake()
	{
        for (int i = 0; i < projectiles.Count; ++i)
		{
            projectiles[i].gameObject.SetActive(false);
		}

        owner = GetComponentInParent<Entity>();
	}

    private void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    //if returned false then do not apply damage
    public virtual void OnTargetHit(Entity target, Projectile source, out bool hit)
	{
        hit = true;
        //projectile can have property to deal damage to damage owner
        if (target == owner && !source.canHitOwner)
		{
            hit = false;
		}
	}

    public virtual void Fire()
    {
        if (currentCooldown > 0)
		{
            needsReload = true;
		}
		else
		{
            needsReload = false;
            currentCooldown = projectiles[combo].cooldown;
        }
    }

    public virtual void Charge()
	{

	}

    public virtual void ReleaseCharge()
	{

	}

    //make overloads with different spawning points and offsets
    protected GameObject SpawnProj(int index, Vector3 direction)
    {
        Projectile proj = Instantiate(projectiles[index].gameObject).GetComponent<Projectile>();
        proj.rigidbody = proj.GetComponent<Rigidbody>(); //do not ask me why, but for some reason THIS is faster then awake in proj.
        if (proj.isChild)
		{
            proj.transform.SetParent(gameObject.transform);
		}
		else //place in render scene
		{
            proj.transform.SetParent(GameObject.FindAnyObjectByType<RayMarchingDatabase>().SceneObject.transform);
		}
        proj.gameObject.SetActive(true);
        //proj.transform.position = new Vector3(transform.position.x + direction.x * proj.offset, transform.position.y + direction.y * proj.offset, transform.position.z + direction.z * proj.offset);
        //proj.transform.position = transform.position + Vector3.RotateTowards(proj.positionOffset, direction, 6.5f, 0.0f);
        Vector3 offset = direction * proj.positionOffset.z; //z offset (dist)
        offset = Quaternion.Euler(proj.angularOffset) * offset; //angular offset
        offset = offset + Vector3.RotateTowards(new Vector3(proj.positionOffset.x, proj.positionOffset.y, 0), direction, 6.5f, 0.0f); //x y offset
        proj.transform.position = transform.position + offset;

        //proj.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), direction);
        proj.transform.forward = direction;
        proj.weapon = gameObject.GetComponent<Weapon>();
        return proj.gameObject;
    }
}
