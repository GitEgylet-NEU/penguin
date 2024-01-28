using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform followTransform;
	public float offset;
	public float xLimit;

	Coroutine lerpCoroutine;

	void Update()
	{
		if (followTransform != null)
		{
			// kövesse a followTransform pozícióját, miközben az xLimiten belül marad
			transform.position = new Vector3(Mathf.Clamp(followTransform.position.x, -xLimit, xLimit), transform.position.y, followTransform.position.z - offset);
		}
	}

	public void SetTransform(Transform newTransform, bool lerp)
	{
		if (lerp)
		{
			// fancy lerp cucc ami nem mûködik úgy ahogy kellene neki
			float startZ = followTransform.position.z;
			followTransform = null;

			IEnumerator Lerp()
			{
				float t = 0;
				while (t <= .5f)
				{
					if (newTransform == null)
					{
						followTransform = TeamManager.instance.penguins[0].transform;
						yield break;
					}
					transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(startZ + t * TeamManager.instance.penguins[0].speed, newTransform.position.z, t * 2f) - offset);
					t += Time.deltaTime;
					yield return null;
				}
				if (newTransform == null)
				{
					followTransform = TeamManager.instance.penguins[0].transform;
					yield break;
				}
				followTransform = newTransform;
			}

			if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
			lerpCoroutine = StartCoroutine(Lerp());
		}
		else
		{
			followTransform = newTransform;
		}
	}
}
