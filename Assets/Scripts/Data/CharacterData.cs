using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Character", fileName = "New Character")]
public class CharacterData : ScriptableObject
{
	public string id;
	public string description;
	public Color color;

	[Header("Battle Attributes")]
	[Tooltip("Whether there can be more instances of this character on the battle layout")] public bool unique;
	public float maxHealth;
	public float hitsPerSecond;
	public float damagePerHit;
	public float range;
	public float speed;
	public float rotationSpeed;
	[Tooltip("Whether the participant should move back to their desired range when their target gets too close")] public bool shouldMoveBack = false;

	[Header("Ability")]
	public bool hasAbility;
	public Ability ability;

	[System.Serializable]
	public class Ability
	{
		public string id;
		public string name;
		public string description;
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