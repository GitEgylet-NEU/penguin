using UnityEngine;

[CreateAssetMenu(menuName = "Data/Game Data")]
public class GameData : ScriptableObject
{
	[Header("Characters")]
	public CharacterData[] playerCharacters;
	public CharacterData[] enemyCharacters;

	[Header("Battle Settings")]
	[Min(0)] public int columns;
	[Min(0)] public int rows;

	private void OnValidate()
	{
		if (rows > 0 && rows % 2 != 0)
		{
			Debug.LogWarning($"The number of rows must be an even number!");
		}
	}
}
