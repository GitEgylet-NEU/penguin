using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
	[Header("Spawn options")]
	[SerializeField] GameObject penguinPrefab;
	public List<Penguin> penguins;
	[SerializeField] int spawnCount;
	[SerializeField] float penguinDistance;

	[Header("Run")]
	[SerializeField] float initialRunSpeed;
	[SerializeField] float runMultiplier;
	public float delay;
	public float runSpeed;

	[Header("Controls")]
	[SerializeField] string horizontalAxisName;
	[SerializeField] float horizontalSensitivity;

	private void Start()
	{
		for (int i = 0; i < spawnCount; i++)
		{
			GameObject obj = Instantiate(penguinPrefab);
			obj.name = "Penguin " + i;
			obj.transform.position = new Vector3(0, 0, -i * penguinDistance);
			Color col = new Color(Random.Range(.4f, .8f), Random.Range(.4f, .8f), Random.Range(.4f, .8f));
			obj.GetComponentInChildren<SpriteRenderer>().color = col;
			penguins.Add(obj.GetComponent<Penguin>());
		}
		FindObjectOfType<CameraController>().followTransform = penguins[0].transform;

		runSpeed = initialRunSpeed;
		delay = penguinDistance / runSpeed;
	}

	private void Update()
	{
		if (runMultiplier > 0)
		{
			runSpeed *= 1f + Time.deltaTime * (runMultiplier - 1f);
			delay = penguinDistance / runSpeed;
		}

		foreach (Penguin penguin in penguins)
		{
			penguin.transform.position += new Vector3(0, 0, runSpeed * Time.deltaTime);
		}

		Move();
	}

	void Move()
	{
		float horizontal = Input.GetAxisRaw(horizontalAxisName);
		float amount = horizontal * horizontalSensitivity * Time.deltaTime;

		int i = 0;
		foreach (Penguin penguin in penguins)
		{
			IEnumerator MoveDelay()
			{
				yield return new WaitForSeconds(i * delay);
				penguin.Move(amount);
			}
			StartCoroutine(MoveDelay());
			i++;
		}
	}
}
