using NohaSoftware.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Character", fileName = "New Character")]
public class CharacterData : ScriptableObject
{
	public string id;
	public string description;
	public float xpYield;

	[Header("Visuals")]
	public Sprite frontSprite;
	public Sprite backSprite;
	public Sprite slideSprite;

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

public static class CharacterDataExtensions
{
	public static CharacterData GetCharacter(this IEnumerable<CharacterData> characters, string id)
	{
		return characters.Where(c => c.id == id).FirstOrDefault();
	}
}