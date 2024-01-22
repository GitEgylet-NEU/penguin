using UnityEngine;

public class BattleLayoutPlanner : MonoBehaviour
{
	[SerializeField] public GameData gameData;
	[SerializeField] int teamSize;

	[SerializeField] Transform battlefield;
	[SerializeField] float padding;
	[SerializeField] GameObject penguinPrefab;

	[Header("UI")]
	[SerializeField] RectTransform characterSelection;

	int currentSize;
	BattleLayout layout;
	Transform[,] markers;

	void Start()
	{
		if (gameData == null)
		{
			Debug.LogError("No GameData has been set!");
			Debug.Break();
			Application.Quit();
			return;
		}

		currentSize = 0;

		layout = new BattleLayout(gameData.columns, gameData.rows, BattleManager.Team.Player);
		layout.CalculateLayout(battlefield, padding);

		//place markers
		GameObject markerTemplate = transform.Find("MarkerTemplate").gameObject;
		markers = new Transform[gameData.columns, gameData.rows];
		for (int r = 0; r < gameData.rows; r++)
		{
			for (int c = 0; c < gameData.columns; c++)
			{
				Vector2 pos = layout.GetPosition(c, r);
				pos = new Vector2(battlefield.position.x - battlefield.localScale.x / 2f + padding + pos.x, battlefield.position.y - battlefield.localScale.y / 2f + padding + pos.y);
				var obj = Instantiate(markerTemplate, transform);
				obj.name = $"Marker ({c};{r})";
				obj.transform.position = pos;
				obj.SetActive(true);
				markers[c, r] = obj.transform;
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P)) SetCharacter(1, 1, "p_peasant");
		if (Input.GetKeyDown(KeyCode.K)) SetCharacter(1, 1, "");
	}

	public bool SetCharacter(int column, int row, string id)
	{
		if (column > gameData.columns || row > gameData.rows)
		{
			Debug.LogError("Coordinates out of bounds!");
			return false;
		}
		if (currentSize >= teamSize)
		{
			Debug.LogError("Adding more characters would excess the team size limit!");
			return false;
		}

		if (!string.IsNullOrEmpty(id) && string.IsNullOrEmpty(layout.characterIDs[column, row]))
		{
			Debug.Log("set");
			// set character where there wasn't any previously
			layout.characterIDs[column, row] = id;

			CharacterData data = GetCharacter(column, row);
			var obj = Instantiate(penguinPrefab, markers[column, row]);
			obj.name = data.name;
			obj.transform.localPosition = Vector2.zero;
			obj.SetActive(true);
			obj.GetComponent<SpriteRenderer>().color = data.color;

			currentSize++;
		}
		else if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(layout.characterIDs[column, row]))
		{
			// remove character
			layout.characterIDs[column, row] = string.Empty;

			Destroy(markers[column, row].GetChild(0).gameObject);

			currentSize--;
		}
		else
		{
			//change character
			layout.characterIDs[column, row] = id;

			CharacterData data = GetCharacter(column, row);
			var obj = markers[column,row].GetChild(0).gameObject;
			obj.name = data.name;
			obj.GetComponent<SpriteRenderer>().color = data.color;
		}
		
		return true;
	}

	public CharacterData GetCharacter(int column, int row)
	{
		if (layout == null)
		{
			Debug.LogError("No layout is set!");
			return null;
		}
		if (string.IsNullOrWhiteSpace(layout.characterIDs[column, row]))
		{
			Debug.LogError($"There's no character placed at ({column};{row})");
			return null;
		}

		switch (layout.team)
		{
			case BattleManager.Team.Player:
				return gameData.playerCharacters.GetCharacter(layout.characterIDs[column, row]);
			case BattleManager.Team.Enemy:
				return gameData.enemyCharacters.GetCharacter(layout.characterIDs[column, row]);
			default:
				Debug.LogError("There's no character list defined for team " + layout.team);
				return null;
		}
	}
}

public class BattleLayout
{
	public string[,] characterIDs; // [col,row] = [x,y]
	public BattleManager.Team team;

	float distX = -1f;
	float distY = -1f;

	public BattleLayout(int columns, int rows, BattleManager.Team team)
	{
		characterIDs = new string[columns, rows];
		this.team = team;
	}

	public void CalculateLayout(Transform battlefield, float padding)
	{
		distX = (battlefield.localScale.x - padding * 2) / (characterIDs.GetLength(0) - 1);
		distY = (battlefield.localScale.y - padding * 2) / (characterIDs.GetLength(1) - 1);
	}
	/// <summary>Calculates the relative position of a cell. Padding must be added afterwards.</summary>
	public Vector2 GetPosition(int column, int row)
	{
		if (distX == -1f || distY == -1f)
		{
			Debug.LogError("You must calculate the layout before trying to access its positions! See CalculateLayout()");
			return Vector2.zero;
		}
		float x = column * distX;
		float y = row * distY;
		return new Vector2(x, y);
	}
}
