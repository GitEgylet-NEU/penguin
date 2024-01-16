using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Penguin : MonoBehaviour
{
	public void Move(float amount)
	{
		transform.position += new Vector3(amount, 0, 0);
	}
}
