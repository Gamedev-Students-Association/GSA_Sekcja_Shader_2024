using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBlaster : Weapon
{
	public override void Fire()
	{
		base.Fire();
		if (needsReload) return;
		if (combo == 0)
		{
			Debug.DrawRay(transform.position, transform.forward, Color.red, 1.0f);
			SpawnProj(combo, transform.forward);
		}
		else if (combo == 1)
		{
			Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, 15) *  transform.forward, Color.red, 1.0f);
			Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -15) *  transform.forward, Color.red, 1.0f);
			SpawnProj(combo, Quaternion.Euler(0, 0, 15) * transform.forward);
			SpawnProj(combo, Quaternion.Euler(0, 0, -15) * transform.forward);
		}
		combo += 1;

		if (combo >= 2) //second part of combo will never activate
		{
			combo = 0;
		}
	}
}
