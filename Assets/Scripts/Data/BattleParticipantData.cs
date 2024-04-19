using NohaSoftware.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class BattleParticipantData : ScriptableObject
{
	public string LocalizedName
	{
		get
		{
			LocalizedString l = new()
			{
				TableReference = "Characters",
				TableEntryReference = name
			};
			l.RefreshString();
			return l.GetLocalizedString();
		}
	}

	[Header("Visuals")]
	public Sprite frontSprite;
	public Sprite backSprite;

	[Header("Level Data")]
	public Level[] levels;

	[Serializable]
	public class Level
	{
		[Header("Battle Attributes")]
		[Tooltip("How many instances are allowed at once?")][Min(1)] public float maxNumber;
		public float maxHealth;
		public float hitsPerSecond;
		public float damagePerHit;
		public float range;
		public float speed;
		public float rotationSpeed;
		//[Tooltip("Whether the participant should move back to their desired range when their target gets too close")] public bool shouldMoveBack = false;

		[Header("Ability")]
		public bool hasAbility;
		public Ability ability;
	}

	[Serializable]
	public class Ability
	{
		public string id;
		[Tooltip("Mennyi okozott damage után használhat képességet?")] public float abilityCost;
		public List<SerializableTuple<string, float>> floats;
	}
}

public static class BattleParticipantDataExtensions
{
	public static BattleParticipantData GetCharacter(this IEnumerable<BattleParticipantData> participants, string name)
	{
		return participants.Where(c => c.name == name).FirstOrDefault();
	}
}