using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Penguin : MonoBehaviour
{
	[HideInInspector] public int id;
	[HideInInspector] public Queue<Vector2> moveCommands;

	Coroutine getBehindCoroutine, catchUpCoroutine;
	bool executeCommands = true;

	public float speed;

	private void Start()
	{
		moveCommands = new Queue<Vector2>();
		speed = TeamManager.instance.initialRunSpeed;
	}

	private void Update()
	{
		// gyorsulás
		if (TeamManager.instance.runMultiplier > 0f)
		{
			if (TeamManager.instance.exponential)
			{
				speed *= 1f + Time.deltaTime * (TeamManager.instance.runMultiplier - 1f);
			}
			else
			{
				//linear
				speed += TeamManager.instance.initialRunSpeed * TeamManager.instance.runMultiplier * Time.deltaTime;
			}
		}

		// mozgás parancsok végrehajtása
		while (moveCommands.TryPeek(out Vector2 command) && command.x <= transform.position.z)
		{
			if (executeCommands) Move(moveCommands.Dequeue().y);
			else moveCommands.Dequeue();
		}
	}

	public void AddMoveCommand(Vector2 command, bool first)
	{
		if (moveCommands == null) return;
		if (executeCommands || first)
		{
			if (first)
				moveCommands.Clear();
            moveCommands.Enqueue(command);
        }
	}

	void Move(float amount)
	{
		transform.position = new Vector3(amount, transform.position.y, transform.position.z);
	}


	public void GetBehind(Penguin other)
	{
		if (getBehindCoroutine != null) StopCoroutine(getBehindCoroutine);
		moveCommands.Clear();

		IEnumerator Follow()
		{
			executeCommands = false;

			float distance = Mathf.Abs(transform.position.x - other.transform.position.x);
			while (distance > 0f)
			{
				if (other == null) break;

				float horizontal = Mathf.Min(distance, TeamManager.instance.horizontalSensitivity * Time.deltaTime);
				if (transform.position.x > other.transform.position.x) horizontal *= -1f;
				Move(transform.position.x + horizontal);

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
		//Debug.Log($"{name} catch up with {other.name}");
		IEnumerator Follow()
		{
			float t = 0f;

			float distance = other.transform.position.z - transform.position.z;
			//Debug.Log($"{distance} > {TeamManager.instance.penguinDistance}");
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
