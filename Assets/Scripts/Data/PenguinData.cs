using UnityEngine;

[CreateAssetMenu(menuName = "Data/Penguin", fileName = "New Penguin")]
public class PenguinData : BattleParticipantData
{
	[Header("Penguin Attributes")]
	public string description;
	public Sprite slideSprite;
}