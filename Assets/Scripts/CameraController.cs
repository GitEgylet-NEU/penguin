using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform followTransform;
	bool run = true;

	private void Start()
	{
		run = followTransform != null;
	}

	void Update()
	{
		if (!run) return;
		transform.position = new Vector3(transform.position.x, transform.position.y, followTransform.position.z - 25f);
	}
}
