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

	Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
	{
		moveCommands = new Queue<Vector2>();
		speed = TeamManager.instance.initialRunSpeed;
	}

    private void FixedUpdate()
    {
        // gyorsulás
        if (TeamManager.instance.runMultiplier > 0f)
        {
            if (TeamManager.instance.exponential)
            {
                speed *= 1f + Time.fixedDeltaTime * (TeamManager.instance.runMultiplier - 1f);
            }
            else
            {
                //linear
                speed += TeamManager.instance.initialRunSpeed * TeamManager.instance.runMultiplier * Time.fixedDeltaTime;
            }
        }
		rb.linearVelocity = new Vector3(0f, 0f, TeamManager.instance.run ? speed : 0f);
    }

    private void Update()
	{
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

	void Move(float x)
	{
		//rb.AddForce(new Vector3((x - transform.position.x) * 100f, 0f, 0f), ForceMode.Impulse);
		transform.position = new Vector3(x, transform.position.y, transform.position.z);
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
			enabled = false;
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
