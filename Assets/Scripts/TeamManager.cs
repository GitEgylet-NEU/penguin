using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class TeamManager : MonoBehaviour
{
	public static TeamManager instance;
	private void Awake()
	{
		instance = this;
	}

	public GameData gameData;

	[Header("Spawn options")]
	[SerializeField] GameObject penguinPrefab;
	public List<Penguin> penguins;
	[Tooltip("Két pingvin közötti távolság (Unity unitban)")] public float penguinDistance;
	[SerializeField] bool randomColors = true;

	[Header("Run")]
	public float initialRunSpeed;
	[Tooltip("Hányszorosára gyorsul a sebesség másodpercenként?")] public float runMultiplier = 0;
	public bool exponential;
	[SerializeField] public float runLength;
	public float runStartZ = float.PositiveInfinity;
	bool run;
	[Tooltip("Milyen gyorsan fog elõreszaladni a pingvin, ha az elõtte lévõ meghal?")] public float penguinCatchUpSpeed = 8f;

	[Header("Controls")]
	[SerializeField] string horizontalAxisName;
	public float horizontalSensitivity;

	[Header("Camera")]
	[SerializeField] CameraController cameraController;
	[SerializeField] float cameraOffset = 20f;

	[Header("UI")]
	[SerializeField] ProgressBar xpBar;
	[SerializeField] TextMeshProUGUI xpText;
	[SerializeField] LocalizedString newLevelString, newCharacterString, upgradePointsString, beatGameString;

	float setRunMultiplierToThis;

	private void Start()
	{
		// load or init layout
		if (!SaveManager.instance.LoadObject(Path.Combine(SaveManager.instance.layoutSavePath, BattleManager.instance.layoutName), out BattleLayout layout))
		{
			layout = new BattleLayout(gameData.columns, gameData.rows, BattleManager.Team.Player);
			layout.characterNames[2, 2] = "Penguin";
			if (!SaveManager.instance.SaveObject(Path.Combine(SaveManager.instance.layoutSavePath, BattleManager.instance.layoutName), layout))
			{
				Debug.LogError("can't init default layout");
				return;
			}
		}
		// pingvinek generálása
		int i = 0;
		foreach (var c in layout.GetCharacters().Shuffle().Select(x => gameData.GetPenguinData(x)))
		{
			GameObject obj = Instantiate(penguinPrefab);
			obj.name = c.name;
			obj.transform.position = new Vector3(0, 0, -i * penguinDistance);
			if (c.slideSprite != null) obj.GetComponentInChildren<SpriteRenderer>().sprite = c.slideSprite;

			if (randomColors)
			{
				Color col = new Color(Random.Range(.4f, .8f), Random.Range(.4f, .8f), Random.Range(.4f, .8f));
				obj.GetComponentInChildren<SpriteRenderer>().color = col;
			}

			Penguin penguin = obj.GetComponent<Penguin>();
			penguin.id = i;
			penguins.Add(penguin);

			i++;
		}

		AudioManager.instance.StartBM();
		cameraController.followTransform = penguins[0].transform;
		cameraController.offset = cameraOffset;

		xpBar.min = SaveManager.instance.progressData.level >= 1 ? gameData.levelXPCosts[SaveManager.instance.progressData.level - 1] + 1 : 0f;
		xpBar.max = SaveManager.instance.progressData.level < gameData.levelXPCosts.Length - 1 ? gameData.levelXPCosts[SaveManager.instance.progressData.level + 1] : gameData.levelXPCosts[SaveManager.instance.progressData.level] + 1;
		xpBar.disappearOnZero = false;

		runLength = gameData.levelRunLength[SaveManager.instance.progressData.level];

		CheckLevels();

		setRunMultiplierToThis = runMultiplier;
		runMultiplier = 0f;

		run = true;
	}

	void CheckLevels()
	{
		if (SaveManager.instance.progressData.lastCheckedLevel != SaveManager.instance.progressData.level)
		{
			List<string> popupText = new();

			int upPoints = 0;
			//leveled up
			for (int i = SaveManager.instance.progressData.lastCheckedLevel + 1; i < SaveManager.instance.progressData.level; i++)
			{
				upPoints += gameData.levelUpgradePointRewards[i + 1];
				if (gameData.levelUpgradeCharacterRewards.Length <= i) continue;
				PenguinData penguin = gameData.levelUpgradeCharacterRewards[i];
				if (penguin == null) continue;
				newCharacterString.Arguments = new object[] { penguin.LocalizedName };
				popupText.Add(newCharacterString.GetLocalizedString());
				SaveManager.instance.progressData.characterLevels.GetElement(penguin.name).Value = 0;
			}
			if (upPoints > 0)
			{
				upgradePointsString.Arguments = new object[] { upPoints };
				popupText.Add(upgradePointsString.GetLocalizedString());
			}
			SaveManager.instance.progressData.lastCheckedLevel = SaveManager.instance.progressData.level - 1;
			SaveManager.instance.SaveProgress();

			if (popupText.Any())
			{
				if (SaveManager.instance.progressData.level == 8) popupText.Add(beatGameString.GetLocalizedString());
				UIController.instance.SetPopup(newLevelString.GetLocalizedString(), string.Join("\n\n", popupText));
			}
		}
	}

	private void Update()
	{
		//if (Input.GetKeyDown(KeyCode.X)) BattleManager.instance.AddXP(20f);
		//if (Input.GetKeyDown(KeyCode.C)) CheckLevels();

		if (xpBar.isActiveAndEnabled)
		{
			xpBar.SetValue(SaveManager.instance.progressData.xp);

			if (SaveManager.instance.progressData.level < gameData.levelXPCosts.Length - 1)
				xpText.text = $"{SaveManager.instance.progressData.xp} / {gameData.levelXPCosts[SaveManager.instance.progressData.level + 1]}";
			else
				xpText.text = string.Empty;
		}

		if (run)
		{
			// mindegyik pingvin mozgatása elõre
			foreach (Penguin penguin in penguins)
			{
				if (penguin == null) continue;
				penguin.transform.position += new Vector3(0, 0, penguin.speed * Time.deltaTime);
			}

			if (UIController.instance.runProgressBar.gameObject.activeInHierarchy) UIController.instance.runProgressBar.SetValue(penguins.First().transform.position.z - runStartZ);

			// run leállítása, ha az elsõ pingvin a végére ér
			if (penguins.FirstOrDefault().transform.position.z >= runStartZ + runLength + 5f)
			{
				Debug.Log("battle");
				run = false;
				ControlHandler.instance.canStrafe = false;

				UIController.instance.runProgressBar.gameObject.SetActive(false);

				BattleManager.instance.startZ = penguins.FirstOrDefault().transform.position.z - BattleManager.instance.length / 2f;
				BattleManager.instance.Initialize();
				foreach (var penguin in penguins)
				{
					penguin.gameObject.SetActive(false);
				}
			}
		}
	}

	public void StartGame()
	{
		runMultiplier = setRunMultiplierToThis;
		xpBar.gameObject.SetActive(false);
		runStartZ = penguins.First().transform.position.z;

		UIController.instance.runProgressBar.max = runLength;
		UIController.instance.runProgressBar.SetValue(0f);
		UIController.instance.runProgressBar.gameObject.SetActive(true);
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
		if (penguins.Count == 1)
		{
			GetComponent<LevelSpawner>().enabled = false;
			AudioManager.instance.StopBM();
			AudioManager.instance.PlaySound("gameover");
			//UIController.instance.ToggleDocument(UIController.instance.gameOverDoc,true);
			UIController.instance.EndGame(false, -1f);
			GetComponent<TeamManager>().enabled = false;
			//return;
		}
		if (penguins.Count == 0 || !penguins.Contains(penguin)) return;
		try
		{
			int idx = penguins.IndexOf(penguin);
			penguins.Remove(penguin);
			Destroy(penguin.gameObject);
			AudioManager.instance.PlaySound("treehit");

			if (idx == 0 && penguins.Any())
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

	}
}
