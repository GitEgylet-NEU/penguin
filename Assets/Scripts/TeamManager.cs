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
	Dictionary<int, List<Coroutine>> penguinCoroutines; // azon coroutine-ok listája, amiket a bizonyos pingvineken kell majd lefuttatni (késleltetett mozgás)
	[SerializeField] int spawnCount;
	[SerializeField][Tooltip("Két pingvin közötti távolság (Unity unitban)")] float penguinDistance;

	[Header("Run")]
	[SerializeField] float initialRunSpeed;
	[SerializeField][Tooltip("Hányszorosára gyorsul a sebesség másodpercenként?")] float runMultiplier;
	[Tooltip("Két pingvin közötti távolság (másodpercben)")] public float delay;
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
		// pingvinek generálása
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
			// sebesség gyorsítása
			if (runMultiplier > 0)
			{
				runSpeed *= 1f + Time.deltaTime * (runMultiplier - 1f);
				delay = penguinDistance / runSpeed;
			}

			// mindegyik pingvin mozgatása elõre
			foreach (Penguin penguin in penguins)
			{
				if (penguin == null) continue;
				penguin.transform.position += new Vector3(0, 0, runSpeed * Time.deltaTime);
			}

			// run leállítása, ha az elsõ pingvin a végére ér
			if (penguins.FirstOrDefault().transform.position.z >= runLength)
			{
				Debug.Log("epic battle");
				run = false;
				ControlHandler.instance.canStrafe = false;
			}
		}
	}

	/// <summary>A pingvinek mozgatása oldalra egy megadott érték alapján.</summary>
	/// <param name="horizontal">Horizontális érték, pl. <see cref="Input.GetAxisRaw(string)"/>-bõl. Negatív esetén balra, pozitív esetén jobbra mozog.</param>
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
			penguinCoroutines[penguin.id].Add(StartCoroutine(MoveDelay())); // hozzáadás a listához, hogy szükség esetén le lehessen állítani
			i++;
		}
	}

	/// <summary>Pingvin kitörlése az x. pozíción</summary>
	public void RemovePenguinAtPosition(int index)
	{
		if (penguins.Count == 0) return;
		RemovePenguin(penguins[index]);
	}
	/// <summary>Megadott ID-jû pingvin kitörlése</summary>
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
			// coroutine-ok leállítása
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
