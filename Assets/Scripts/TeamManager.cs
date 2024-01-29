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
	[SerializeField] int spawnCount;
	[Tooltip("Két pingvin közötti távolság (Unity unitban)")] public float penguinDistance;
	[SerializeField] bool randomColors = true;

	[Header("Run")]
	public float initialRunSpeed;
	[Tooltip("Hányszorosára gyorsul a sebesség másodpercenként?")] public float runMultiplier = 0;
	[SerializeField] float runLength;
	bool run;
	[Tooltip("Milyen gyorsan fog elõreszaladni a pingvin, ha az elõtte lévõ meghal?")] public float penguinCatchUpSpeed = 8f;

	[Header("Controls")]
	[SerializeField] string horizontalAxisName;
	public float horizontalSensitivity;

	[Header("Camera")]
	[SerializeField] CameraController cameraController;
	[SerializeField] float cameraOffset = 20f;

	private void Start()
	{
		// pingvinek generálása
		for (int i = 0; i < spawnCount; i++)
		{
			GameObject obj = Instantiate(penguinPrefab);
			obj.name = "Penguin " + i;
			obj.transform.position = new Vector3(0, 0, -i * penguinDistance);

			if (randomColors)
			{
				Color col = new Color(Random.Range(.4f, .8f), Random.Range(.4f, .8f), Random.Range(.4f, .8f));
				obj.GetComponentInChildren<SpriteRenderer>().color = col;
			}

			Penguin penguin = obj.GetComponent<Penguin>();
			penguin.id = i;
			penguins.Add(penguin);
		}

		cameraController.followTransform = penguins[0].transform;
		cameraController.offset = cameraOffset;

		run = true;
	}

	private void Update()
	{
		if (run)
		{
			// mindegyik pingvin mozgatása elõre
			foreach (Penguin penguin in penguins)
			{
				if (penguin == null) continue;
				penguin.transform.position += new Vector3(0, 0, penguin.speed * Time.deltaTime);
			}

			if (!penguins.Any())
			{
				Debug.Log("big oof");
				Debug.Break();
				Application.Quit();
			}
			// run leállítása, ha az elsõ pingvin a végére ér
			if (penguins.FirstOrDefault().transform.position.z >= runLength)
			{
				Debug.Log("epic battle");
				run = false;
				ControlHandler.instance.canStrafe = false;
			}
		}

		if (Input.GetKeyDown(KeyCode.K)) RemovePenguinAtPosition(1);
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
			penguin.AddMoveCommand(penguins.First().transform.position.z, amount);
			i++;
		}
	}

	public Penguin GetPenguinByID(int id)
	{
		return penguins.Where(p => p.id == id).FirstOrDefault();
	}

	/// <summary>Pingvin kitörlése az x. pozíción</summary>
	/// <param name="index">0 = elsõ</param>
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
			int idx = penguins.IndexOf(penguin);
			penguins.Remove(penguin);
			Destroy(penguin.gameObject);

			if (idx == 0)
			{
				cameraController.SetTransform(penguins[0].transform, true);
				foreach (Penguin p in penguins)
				{
					if (p.id == penguins[0].id) continue;
					p.GetBehind(penguins[0]);
				}
			}

			Penguin previous = penguins[0];
			foreach (Penguin p in penguins)
			{
				if (p.id == penguins[0].id) continue;
				p.CatchUp(previous);
				previous = p;
			}
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
