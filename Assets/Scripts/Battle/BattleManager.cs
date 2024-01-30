using NohaSoftware.Utilities;
using System.Collections.Generic;
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

	[SerializeField] Transform battlefield;
	[SerializeField][Min(0)] int columns;
	[SerializeField][Min(0)] int rows;
	[SerializeField][Min(0)] float padding;

	[Header("Battle")]
	public List<BattleParticipant> participants;
	public Canvas worldSpaceCanvas;
	public GameObject healthBarPrefab;
	public GameObject abilityBarPrefab;

	[Header("Layout")]
	[SerializeField] string layoutName = "default";
	[SerializeField] GameObject penguinPrefab;

	private void Start()
	{
		if (gameData == null)
		{
			Debug.LogError("No GameData has been set!");
			Debug.Break();
			Application.Quit();
			return;
		}

		//try to load layout
		if (SaveManager.instance.LoadLayout(layoutName, out BattleLayout layout))
		{
			for (int c = 0; c < gameData.columns; c++)
			{
				for (int r = 0; r < layout.characterIDs.GetLength(1); r++)
				{
					if (string.IsNullOrEmpty(layout.characterIDs[c, r])) continue;
					CharacterData data = gameData.GetCharacterData(layout.characterIDs[c, r], layout.team);
					if (data == null) continue;

					//spawn penguin
					Vector2 pos = layout.GetPosition(c, r);
					pos = new Vector2(battlefield.position.x - battlefield.localScale.x / 2f + padding + pos.x, battlefield.position.y - battlefield.localScale.y / 2f + padding + pos.y);
					var obj = Instantiate(penguinPrefab, transform);
					obj.name = $"{data.name} ({c};{r})";
					obj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, 0f, 90f));

					obj.GetComponent<SpriteRenderer>().color = data.color;
					BattleParticipant p = obj.GetComponent<BattleParticipant>();
					p.Setup(data);
					p.team = layout.team;

					obj.SetActive(true);
				}
			}
		}

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

					//spawn enemy
					Vector2 pos = layout.GetPosition(c, r);
					pos = new Vector2(battlefield.position.x - battlefield.localScale.x / 2f + padding + pos.x, battlefield.position.y - battlefield.localScale.y / 2f + padding + pos.y);
					var obj = Instantiate(penguinPrefab, transform);
					obj.name = $"{data.name} ({c};{r})";
					obj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, 0f, -90f));

					obj.GetComponent<SpriteRenderer>().color = data.color;
					BattleParticipant p = obj.GetComponent<BattleParticipant>();
					p.Setup(data);
					p.team = enemyLayout.team;

					obj.SetActive(true);
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (battlefield == null || columns < 1 || rows < 1) return;
		float distX = (battlefield.localScale.x - padding * 2) / (columns - 1);
		float distY = (battlefield.localScale.y - padding * 2) / (rows - 1);
		Gizmos.color = Color.yellow;
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				float x = battlefield.position.x - battlefield.localScale.x / 2f + padding + j * distX;
				float y = battlefield.position.y - battlefield.localScale.y / 2f + padding + i * distY;
				Gizmos.DrawSphere(new Vector2(x, y), .2f);
			}
		}
	}
}
