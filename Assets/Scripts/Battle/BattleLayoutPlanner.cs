using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleLayoutPlanner : MonoBehaviour
{
	[Header("Settings")]
	public GameData gameData;
	[SerializeField] string layoutName = "default";
	public BattleManager.Team team = BattleManager.Team.Player;

	[Header("Planner Attributes")]
	[SerializeField] Transform battlefield;
	[SerializeField] float padding;
	public GameObject penguinPrefab;

	[SerializeField] Color availableMarker;
	[SerializeField] Color unavailableMarker;

	[Header("UI")]
	[SerializeField] CharacterSelection characterSelection;
	[SerializeField] RectTransform modalWindow;
	[SerializeField] RectTransform alertWindow;
	[SerializeField] Button saveButton;
	[SerializeField] Button backButton;

	BattleLayout layout;
	Transform[,] markers;
	bool changed = false;
	bool hasLayout = true;
	public int size;

	void Start()
	{
		if (gameData == null)
		{
			Debug.LogError("No GameData has been set!");
			Debug.Break();
			Application.Quit();
			return;
		}

		size = 0;

		layout = new BattleLayout(gameData.columns, gameData.rows, team);
		layout.CalculateLayout(battlefield, padding);

		SaveManager.instance.LoadProgress();
		characterSelection.Init(gameData.playerCharacters);

		//place markers
		GameObject markerTemplate = transform.Find("MarkerTemplate").gameObject;
		markers = new Transform[gameData.columns, Mathf.FloorToInt(gameData.rows / 2f)];
		for (int r = 0; r < gameData.rows; r++)
		{
			for (int c = 0; c < gameData.columns; c++)
			{
				Vector2 pos = layout.GetPosition(c, r);
				pos = new Vector2(battlefield.position.x - battlefield.localScale.x / 2f + padding + pos.x, battlefield.position.y - battlefield.localScale.y / 2f + padding + pos.y);
				var obj = Instantiate(markerTemplate, transform);
				obj.name = $"{c};{r}";
				obj.transform.position = pos;

				if (r < (gameData.rows / 2f))
				{
					//row is available for placement
					obj.GetComponentInChildren<SpriteRenderer>().color = availableMarker;
					markers[c, r] = obj.transform;
				}
				else
				{
					//don't add row to markers matrix so that it can't be placed upon
					obj.GetComponentInChildren<SpriteRenderer>().color = unavailableMarker;
				}

				obj.SetActive(true);
			}
		}

		//try to load layout
		if (SaveManager.instance.LoadObject(Path.Combine(SaveManager.instance.layoutSavePath, layoutName), out object o))
		{
			try
			{
				BattleLayout l = o as BattleLayout;
				layout.team = l.team;
				for (int c = 0; c < gameData.columns; c++)
				{
					for (int r = 0; r < layout.characterIDs.GetLength(1); r++)
					{
						if (!string.IsNullOrEmpty(l.characterIDs[c, r])) try
							{
								SetCharacter(c, r, l.characterIDs[c, r]);
							}
							catch
							{
								SetCharacter(c, r, string.Empty);
							}
					}
				}
				changed = false;
				hasLayout = true;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				throw;
			}
		}
		else hasLayout = false;

		if (size == 0)
		{
			alertWindow.gameObject.SetActive(true);
			characterSelection.IgnoreTouch(true);
		}
		else alertWindow.gameObject.SetActive(false);
	}

	private void Update()
	{
		saveButton.gameObject.SetActive(changed && size > 0);
		backButton.gameObject.SetActive(size > 0);
	}

	public void SaveLayout()
	{
		if (layout == null || string.IsNullOrEmpty(layoutName) || SaveManager.instance == null)
		{
			Debug.LogWarning("Can't initiate save");
			return;
		}
		if (SaveManager.instance.SaveObject(Path.Combine(SaveManager.instance.layoutSavePath, layoutName), layout))
		{
			Debug.Log("Successfully saved layout as " + layoutName);
			changed = false;
		}
	}

	/// <summary>Return the first marker which the given <see cref="Vector2"/> overlaps.</summary>
	public Transform GetMarker(Vector2 position)
	{
		foreach (var m in markers)
		{
			if (m.GetComponentInChildren<Collider2D>().OverlapPoint(position)) return m;
		}
		return null;
	}

	public bool SetCharacter(int column, int row, string id)
	{
		if (column > gameData.columns || row >= layout.characterIDs.GetLength(1))
		{
			Debug.LogError("Coordinates out of bounds!");
			return false;
		}

		if (!string.IsNullOrEmpty(id) && string.IsNullOrEmpty(layout.characterIDs[column, row]))
		{
			// set character where there wasn't any previously
			layout.characterIDs[column, row] = id;
			size++;

			CharacterData data = GetCharacter(column, row);
			var obj = Instantiate(penguinPrefab, markers[column, row]);
			obj.name = data.name;
			obj.transform.localPosition = Vector2.zero;
			obj.SetActive(true);
			//obj.GetComponent<SpriteRenderer>().color = data.color;
			obj.GetComponent<SpriteRenderer>().sprite = data.frontSprite;
		}
		else if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(layout.characterIDs[column, row]))
		{
			// remove character
			layout.characterIDs[column, row] = string.Empty;
			Debug.Log(size);
			size--;
			Debug.Log($"remove {size}");

			Destroy(markers[column, row].GetChild(1).gameObject);
		}
		else
		{
			//change character
			Debug.Log("Change");
			layout.characterIDs[column, row] = id;

			CharacterData data = GetCharacter(column, row);
			var obj = markers[column, row].GetChild(1).gameObject;
			obj.name = data.name;
			obj.GetComponent<SpriteRenderer>().sprite = data.frontSprite;
		}
		Debug.Log($"set ({column};{row}) to {id}");
		changed = true;
		
		if (size == 0)
		{
			alertWindow.gameObject.SetActive(true);
			characterSelection.IgnoreTouch(true);
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

		return gameData.GetCharacterData(layout.characterIDs[column, row], layout.team);
	}

	public void ReturnToMenu(bool confirm = false)
	{
		if (!confirm && changed)
		{
			modalWindow.gameObject.SetActive(true);
			modalWindow.Find("Decline").GetComponent<Button>().interactable = hasLayout;
			//wait for user input
		}
		else
		{
			SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
		}
	}

	public int Count(string id) => layout.Count(id);
}

[Serializable]
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

	public int Count(string id)
	{
		int i = 0;
		for (int col = 0; col < characterIDs.GetLength(0); col++)
		{
			for (int row = 0; row < characterIDs.GetLength(1); row++)
			{
				if (characterIDs[col, row] == id) i++;
			}
		}
		return i;
	}

	public string[] GetCharacters()
	{
		List<string> results = new();
		for (int col = 0; col < characterIDs.GetLength(0); col++)
		{
			for (int row = 0; row < characterIDs.GetLength(1); row++)
			{
				if (!string.IsNullOrEmpty(characterIDs[col, row])) results.Add(characterIDs[col, row]);
			}
		}
		return results.ToArray();
	}
}
