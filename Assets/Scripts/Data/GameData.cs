using UnityEngine;

[CreateAssetMenu(menuName = "Data/Game Data")]
public class GameData : ScriptableObject
{
	[Header("Battle Settings")]
	[Min(0)] public int columns;
	[Min(0)] public int rows;
	public PremadeBattleLayout[] premadeBattleLayouts;

	[Space]
	public CharacterData[] playerCharacters;
	public CharacterData[] enemyCharacters;

	private void OnValidate()
	{
		if (rows > 0 && rows % 2 != 0)
		{
			Debug.LogWarning($"The number of rows must be an even number!");
		}
	}

	public CharacterData GetCharacterData(string id, BattleManager.Team team)
	{
		switch (team)
		{
			case BattleManager.Team.Player:
				return playerCharacters.GetCharacter(id);
			case BattleManager.Team.Enemy:
				return enemyCharacters.GetCharacter(id);
			default:
				Debug.LogError("There's no character list defined for team " + team);
				return null;
		}
	}
}
