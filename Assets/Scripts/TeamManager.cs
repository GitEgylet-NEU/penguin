using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
	public static TeamManager instance;
	private void Awake()
	{
		instance = this;
	}

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

	[Header("Camera")]
	[SerializeField] CameraController cameraController;
	[SerializeField] float cameraOffset = 20f;

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

		cameraController.followTransform = penguins[0].transform;
		cameraController.offset = cameraOffset;

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
			if (penguin == null) continue;
			penguin.transform.position += new Vector3(0, 0, runSpeed * Time.deltaTime);
		}

		Move();


	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.K)) RemovePenguin(0);
	}

	void Move()
	{
		float horizontal = Input.GetAxisRaw(horizontalAxisName);
		float amount = horizontal * horizontalSensitivity * Time.deltaTime;

		int i = 0;
		foreach (Penguin penguin in penguins)
		{
			if (penguin == null) continue;
			IEnumerator MoveDelay()
			{
				yield return new WaitForSeconds(i * delay * .85f);
				penguin.Move(amount);
			}
			StartCoroutine(MoveDelay());
			i++;
		}
	}

	public void RemovePenguin(int index)
	{
		if (penguins.Count == 0) return;
		RemovePenguin(penguins[index]);
	}
	public void RemovePenguin(Penguin penguin)
	{
		if (penguins.Count == 0 || !penguins.Contains(penguin)) return;
		try
		{
			int idx = penguins.IndexOf(penguin);
			penguins.Remove(penguin);
			Destroy(penguin.gameObject);

			if (idx == 0) cameraController.SetTransform(penguins[0].transform, true);
		}
		catch
		{
			Debug.LogError("Can't remove Penguin");
			return;
		}
		if (penguins.Count == 0)
		{
			Debug.Log("ur ded not big soup rice");
			Debug.Break();
		}
	}
}
