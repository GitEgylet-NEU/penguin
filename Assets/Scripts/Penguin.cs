using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Penguin : MonoBehaviour
{
	[HideInInspector] public int id;
	[HideInInspector] public List<MoveCommand> moveCommands;

	Coroutine getBehindCoroutine, catchUpCoroutine;
	bool executeCommands = true;

	public float speed;

	private void Start()
	{
		moveCommands = new List<MoveCommand>();
		speed = TeamManager.instance.initialRunSpeed;
	}

	private void Update()
	{
		// gyorsulás
		if (TeamManager.instance.runMultiplier > 0f) speed *= 1f + Time.deltaTime * (TeamManager.instance.runMultiplier - 1f);

		// mozgás parancsok végrehajtása
		foreach (var command in moveCommands.Where(c => transform.position.z >= c.z).ToArray())
		{
			if (executeCommands) Move(command.amount);
			moveCommands.Remove(command);
		}
	}

	public void AddMoveCommand(float z, float amount)
	{
		if (!executeCommands) return;
		moveCommands.Add(new MoveCommand(z, amount));
	}

	void Move(float amount)
	{
		transform.position += new Vector3(amount, 0, 0);
	}


	public void GetBehind(Penguin other)
	{
		if (getBehindCoroutine != null) StopCoroutine(getBehindCoroutine);

		IEnumerator Follow()
		{
			executeCommands = false;

			float distance = Mathf.Abs(transform.position.x - other.transform.position.x);
			while (distance > 0f)
			{
				if (other == null) break;

				float horizontal = Mathf.Min(distance, TeamManager.instance.horizontalSensitivity * Time.deltaTime);
				if (transform.position.x > other.transform.position.x) horizontal *= -1f;
				Move(horizontal);

				distance = Mathf.Abs(transform.position.x - other.transform.position.x);

				yield return null;
			}

			executeCommands = true;
		}
		getBehindCoroutine = StartCoroutine(Follow());
	}
	public void CatchUp(Penguin other)
	{
		if (catchUpCoroutine != null) StopCoroutine(catchUpCoroutine);
		Debug.Log($"{name} catch up with {other.name}");
		IEnumerator Follow()
		{
			float t = 0f;

			float distance = other.transform.position.z - transform.position.z;
			Debug.Log($"{distance} > {TeamManager.instance.penguinDistance}");
			while (distance > TeamManager.instance.penguinDistance || t < .4f)
			{
				if (other == null) break;

				float amount = Mathf.Min(distance - TeamManager.instance.penguinDistance, TeamManager.instance.penguinCatchUpSpeed * Time.deltaTime);
				//Debug.Log(amount);
				transform.position += new Vector3(0f, 0f, amount);

				distance = other.transform.position.z - transform.position.z;
				t += Time.deltaTime;

				yield return null;
			}
		}
		catchUpCoroutine = StartCoroutine(Follow());
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
		{
			TeamManager.instance.RemovePenguin(this);
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public struct MoveCommand
	{
		public float z;
		public float amount;

		public MoveCommand(float z, float amount)
		{
			this.z = z;
			this.amount = amount;
		}
	}
}
