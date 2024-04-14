using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	public static BattleManager instance;
	private void Awake()
	{
		instance = this;
		participants = new();
	}

	public enum Team
	{
		Player = 0,
		Enemy = 1
	}

	public GameData gameData;

	[Min(0)] public float padding;
	public float startZ;
	[Min(0)] public float length;
	[Min(0)] public float width;
	float distX;
	float distZ;
	float cachedXP = 0f;

	[Header("Battle")]
	public List<BattleParticipant> participants;
	public Canvas worldSpaceCanvas;
	public GameObject healthBarPrefab;
	public GameObject abilityBarPrefab;

	[Header("Layout")]
	public string layoutName = "default";
	[SerializeField] GameObject penguinPrefab;
	public bool load;
	bool init = false;

	private void Start()
	{
		if (gameData == null)
		{
			Debug.LogError("No GameData has been set!");
			Debug.Break();
			Application.Quit();
			return;
		}

		if (load) Initialize();
	}

	public void Initialize()
	{
		//spawn penguins
		if (SaveManager.instance.LoadObject(Path.Combine(SaveManager.instance.layoutSavePath, layoutName), out object o))
		{
			try
			{
				bool saveAtEnd = false;
				BattleLayout layout = o as BattleLayout;

				distX = (width - padding * 2) / (gameData.columns - 1);
				distZ = (length - padding * 2) / (gameData.rows - 1);
				for (int c = 0; c < gameData.columns; c++)
				{
					for (int r = 0; r < layout.characterNames.GetLength(1); r++)
					{
						if (string.IsNullOrEmpty(layout.characterNames[c, r])) continue;
						if (!gameData.playerCharacters.Any(pc => pc.name == layout.characterNames[c,r]))
						{
							layout.characterNames[c, r] = string.Empty;
							saveAtEnd = true;
							continue;
						}
						if (!TeamManager.instance.penguins.Any(p => p.gameObject.name == layout.characterNames[c, r])) continue;
						PenguinData data = gameData.GetPenguinData(layout.characterNames[c, r]);
						if (data == null) continue;

						//spawn
						Vector3 pos = GetPosition(c, r);
						var obj = Instantiate(penguinPrefab, transform);
						obj.name = $"{data.name} ({c};{r})";
						obj.transform.position = pos;

						//obj.GetComponent<SpriteRenderer>().color = data.color;
						BattleParticipant p = obj.GetComponent<BattleParticipant>();
						p.Setup(data);
						p.team = layout.team;

						obj.SetActive(true);
					}
				}

				if (saveAtEnd) SaveManager.instance.SaveObject(Path.Combine(SaveManager.instance.layoutSavePath, layoutName), o);
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
		}


		//calculate difficulty score based on player level and remaining penguins
		int diff = Mathf.RoundToInt((SaveManager.instance.progressData.level + 1) / 2f);
		foreach (var penguin in TeamManager.instance.penguins)
		{
			diff += SaveManager.instance.progressData.characterLevels.GetElement(penguin.gameObject.name).Value + 1;
		}
		Debug.Log($"diff score: {diff}");

		float minDiff = (SaveManager.instance.progressData.level == 0) ? 0f : diff * .7f;
		//spawn enemies
		PremadeBattleLayout enemyLayout = gameData.premadeBattleLayouts.GetRandom(l => l.Difficulty >= minDiff && l.Difficulty <= diff * 1.2f);
		if (enemyLayout == null)
		{
			int maxDiff = gameData.premadeBattleLayouts.Select(l => l.Difficulty).Max();
			if (diff > maxDiff) enemyLayout = gameData.premadeBattleLayouts.GetRandom(l => l.Difficulty == maxDiff);
		}
		if (enemyLayout != null)
		{
			Debug.Log($"Loaded {enemyLayout.name} ({enemyLayout.Difficulty})");
			var matrix = enemyLayout.GetMatrix();
			for (int c = 0; c < matrix.GetLength(0); c++)
			{
				for (int r = 0; r < matrix.GetLength(1); r++)
				{
					if (string.IsNullOrEmpty(matrix[c, r])) continue;
					EnemyData data = gameData.GetEnemyData(matrix[c, r]);
					if (data == null) continue;

					//spawn
					Vector3 pos = GetPosition(c, r);
					var obj = Instantiate(penguinPrefab, transform);
					obj.name = $"{data.name} ({c};{r})";
					obj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, -180f, 0f));

					BattleParticipant p = obj.GetComponent<BattleParticipant>();
					p.team = Team.Enemy;
					p.Setup(data);

					obj.SetActive(true);
				}
			}
		}
		init = true;
	}

	private void Update()
	{
		if (!init) return;

		if (!participants.Any(p => p.team == Team.Player))
		{
			Debug.Log("you lose");
			//UIController.instance.ToggleDocument(UIController.instance.gameOverDoc, true);
			UIController.instance.EndGame(false, cachedXP);
			SaveXP(false);
			foreach (var participant in participants) participant.DisableParticipant();
			init = false;
		}
		if (!participants.Any(p => p.team == Team.Enemy))
		{
			Debug.Log("you win");
			//UIController.instance.setWin();
			UIController.instance.EndGame(true, cachedXP);
			SaveXP(true);
			foreach (var participant in participants) participant.DisableParticipant();
			init = false;
		}
	}

	public void AddXP(float amount)
	{
		cachedXP += amount;
		Debug.Log($"add {amount} XP");
	}

	public void SaveXP(bool won)
	{
		if (!won) cachedXP *= gameData.lostBattleXPModifier;

		SaveManager.instance.progressData.xp += cachedXP;
		if (SaveManager.instance.progressData.level < gameData.levelXPCosts.Length - 1 && SaveManager.instance.progressData.xp >= gameData.levelXPCosts[SaveManager.instance.progressData.level + 1])
		{
			SaveManager.instance.progressData.level++;
			SaveManager.instance.progressData.upgradePoints += gameData.levelUpgradePointRewards[SaveManager.instance.progressData.level];
		}
	}

	Vector3 GetPosition(int column, int row)
	{
		float x = -width / 2f + padding + column * distX;
		float z = startZ + padding + row * distZ;
		return new Vector3(x, 1f, z);
	}

	private void OnDrawGizmosSelected()
	{
		if (gameData.columns < 1 || gameData.rows < 1) return;
		float distX = (width - padding * 2) / (gameData.columns - 1);
		float distZ = (length - padding * 2) / (gameData.rows - 1);
		Gizmos.color = Color.yellow;
		for (int i = 0; i < gameData.rows; i++)
		{
			for (int j = 0; j < gameData.columns; j++)
			{
				float x = -width / 2f + padding + j * distX;
				float z = startZ + padding + i * distZ;
				Gizmos.DrawSphere(new Vector3(x, 0f, z), .2f);
			}
		}
	}
}
