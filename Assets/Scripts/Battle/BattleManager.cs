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

	[Header("Battle")]
	public List<BattleParticipant> participants;
	public Canvas worldSpaceCanvas;
	public GameObject healthBarPrefab;
	public GameObject abilityBarPrefab;

	[Header("Layout")]
	[SerializeField] string layoutName = "default";
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

		SaveManager.instance.LoadProgress();

		if (load) Initialize();
	}

	public void Initialize()
	{
		//spawn penguins
		if (SaveManager.instance.LoadObject(Path.Combine(SaveManager.instance.layoutSavePath, layoutName), out object o))
		{
			try
			{
				BattleLayout layout = o as BattleLayout;

				distX = (width - padding * 2) / (gameData.columns - 1);
				distZ = (length - padding * 2) / (gameData.rows - 1);
				for (int c = 0; c < gameData.columns; c++)
				{
					for (int r = 0; r < layout.characterIDs.GetLength(1); r++)
					{
						if (string.IsNullOrEmpty(layout.characterIDs[c, r])) continue;
						CharacterData data = gameData.GetCharacterData(layout.characterIDs[c, r], layout.team);
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
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
		}

		//spawn enemies
		PremadeBattleLayout enemyLayout = gameData.premadeBattleLayouts.GetRandom();
		if (enemyLayout != null)
		{
			Debug.Log($"Loaded {enemyLayout.name} ({enemyLayout.difficulty})");
			var matrix = enemyLayout.GetMatrix();
			for (int c = 0; c < matrix.GetLength(0); c++)
			{
				for (int r = 0; r < matrix.GetLength(1); r++)
				{
					if (string.IsNullOrEmpty(matrix[c, r])) continue;
					CharacterData data = gameData.GetCharacterData(matrix[c, r], enemyLayout.team);
					if (data == null) continue;

					//spawn
					Vector3 pos = GetPosition(c, r);
					var obj = Instantiate(penguinPrefab, transform);
					obj.name = $"{data.name} ({c};{r})";
					obj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, -180f, 0f));

					obj.GetComponent<SpriteRenderer>().color = data.color;
					BattleParticipant p = obj.GetComponent<BattleParticipant>();
					p.team = enemyLayout.team;
					p.Setup(data);

					obj.SetActive(true);
				}
			}
		}
	}

	private void Update()
	{
		if (!init) return;
		foreach (Team t in System.Enum.GetValues(typeof(Team)))
		{
			if (!participants.Any(p => p.team == t))
			{
				Debug.Log($"{t} lost!");
				Debug.Break();
				return;
			}
		}
	}

	private void OnDestroy()
	{
		SaveManager.instance.SaveProgress();
	}

	public void AddXP(float amount)
	{
		SaveManager.instance.progressData.xp += amount;
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
