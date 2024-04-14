using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy", fileName = "New Enemy")]
public class EnemyData : BattleParticipantData
{
	[Header("Enemy Attributes")]
	[Tooltip("How much XP the player gets from killing this character")] public float xpYield;
	[Tooltip("How difficult it is to defeat this character")] public int difficulty = 0;
}