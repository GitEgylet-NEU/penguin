using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	Dictionary<int, List<Coroutine>> penguinCoroutines;
	[SerializeField] int spawnCount;
	[SerializeField] float penguinDistance;

	[Header("Run")]
	[SerializeField] float initialRunSpeed;
	[SerializeField] float runMultiplier;
	public float delay;
	public float runSpeed;
	[SerializeField] float runLength;
	bool run;

	[Header("Controls")]
	[SerializeField] string horizontalAxisName;
	[SerializeField] float horizontalSensitivity;

	[Header("Camera")]
	[SerializeField] CameraController cameraController;
	[SerializeField] float cameraOffset = 20f;

	private void Start()
	{
		penguinCoroutines = new();
		for (int i = 0; i < spawnCount; i++)
		{
			GameObject obj = Instantiate(penguinPrefab);
			obj.name = "Penguin " + i;
			obj.transform.position = new Vector3(0, 0, -i * penguinDistance);
			Color col = new Color(Random.Range(.4f, .8f), Random.Range(.4f, .8f), Random.Range(.4f, .8f));
			obj.GetComponentInChildren<SpriteRenderer>().color = col;

			Penguin penguin = obj.GetComponent<Penguin>();
			penguin.id = i;
			penguins.Add(penguin);

			penguinCoroutines[i] = new();
		}

		cameraController.followTransform = penguins[0].transform;
		cameraController.offset = cameraOffset;

		runSpeed = initialRunSpeed;
		delay = penguinDistance / runSpeed;
		run = true;
	}

	private void Update()
	{
		if (run)
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
			if (penguins.FirstOrDefault().transform.position.z >= runLength)
			{
				Debug.Log("epic battle");
				run = false;
				ControlHandler.instance.canStrafe = false;
			}
		}
	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.K)) RemovePenguinAtPosition(0);
	}

	/// <summary>Must be called in Update()</summary>
	public void Move(float horizontal)
	{
		float amount = horizontal * horizontalSensitivity * Time.deltaTime;

		int i = 0;
		foreach (Penguin penguin in penguins)
		{
			if (penguin == null) continue;
			IEnumerator MoveDelay()
			{
				yield return new WaitForSeconds(i * delay * .95f);
				penguin.Move(amount);
				penguinCoroutines[penguin.id].RemoveAt(0);
			}
			penguinCoroutines[penguin.id].Add(StartCoroutine(MoveDelay()));
			i++;
		}
	}

	public void RemovePenguinAtPosition(int index)
	{
		if (penguins.Count == 0) return;
		RemovePenguin(penguins[index]);
	}
	public void RemovePenguinWithID(int id)
	{
		if (penguins.Count == 0) return;
		RemovePenguin(penguins.Where(p => p.id == id).FirstOrDefault());
	}
	public void RemovePenguin(Penguin penguin)
	{
		if (penguins.Count == 0 || !penguins.Contains(penguin)) return;
		try
		{
			foreach (Coroutine c in penguinCoroutines[penguin.id])
			{
				if (c != null) StopCoroutine(c);
			}
			penguinCoroutines.Remove(penguin.id);

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
			Application.Quit();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(new Vector3(-cameraController.xLimit, 0, runLength), new Vector3(cameraController.xLimit, 0, runLength));
	}
}
