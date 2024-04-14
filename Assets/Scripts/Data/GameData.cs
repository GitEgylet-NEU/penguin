using UnityEngine;

[CreateAssetMenu(menuName = "Data/Game Data")]
public class GameData : ScriptableObject
{
	[Header("Battle Settings")]
	[Min(0)] public int columns;
	[Min(0)] public int rows;
	public PremadeBattleLayout[] premadeBattleLayouts;
	[Tooltip("Multiply all XP by this number when the player loses the battle")] public float lostBattleXPModifier = .5f;

	[Space]
	public PenguinData[] playerCharacters;
	public EnemyData[] enemyCharacters;

	[Header("Player Progress")]
	public float[] levelXPCosts;
	public int[] levelUpgradePointRewards;
	public PenguinData[] levelUpgradeCharacterRewards;
	public float[] levelRunLength;
	public int[] characterUpgradeCosts;
	public PenguinData defaultPenguin;

	private void OnValidate()
	{
		if (rows > 0 && rows % 2 != 0)
		{
			Debug.LogWarning($"The number of rows must be an even number!");
		}
	}

	public PenguinData GetPenguinData(string id)
	{
		return (PenguinData)playerCharacters.GetCharacter(id);
	}
	public EnemyData GetEnemyData(string id)
	{
		return (EnemyData)enemyCharacters.GetCharacter(id);
	}
}
