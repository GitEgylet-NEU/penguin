using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Data/Penguin", fileName = "New Penguin")]
public class PenguinData : BattleParticipantData
{
	[Header("Penguin Attributes")]
	public string description;
	public string LocalizedDescription
	{
		get
		{
			LocalizedString l = new()
			{
				TableReference = "Characters",
				TableEntryReference = name + "_d"
			};
			l.RefreshString();
			return l.GetLocalizedString();
		}
	}
	public Sprite slideSprite;
}