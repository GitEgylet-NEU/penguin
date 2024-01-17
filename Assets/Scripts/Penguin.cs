using UnityEngine;

public class Penguin : MonoBehaviour
{
	[HideInInspector] public int id;

	public void Move(float amount)
	{
		transform.position += new Vector3(amount, 0, 0);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
		{
			TeamManager.instance.RemovePenguin(this);
		}
	}
}
