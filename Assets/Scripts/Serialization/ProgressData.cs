using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ProgressData
{
	public int level;
	public float xp;
	public int upgradePoints;
	public int lastCheckedLevel;

	public List<SerializableTuple<string, int>> characterLevels;

	public void InitCharacterLevels(IEnumerable<CharacterData> characters)
	{
		Debug.Log("init charlevels");
		characterLevels = characters.Select(c => new SerializableTuple<string, int>(c.id, 0)).ToList();
	}

	public void UpdateCharacterLevels(IEnumerable<CharacterData> characters)
	{
		if (characterLevels == null || characterLevels.Count == 0)
		{
			InitCharacterLevels(characters);
		}
		Debug.Log("update charlevels");
		foreach (var c in characters)
		{
			if (c == null || string.IsNullOrEmpty(c.id)) continue;
			if (characterLevels == null) characterLevels = new();
			if (!characterLevels.ContainsKey(c.id))
			{
				characterLevels.Add(new(c.id, 0));
			}
		}
		foreach (var x in characterLevels.ToArray())
		{
			if (!characters.Any(c => c.id == x.Key))
			{
				characterLevels.RemoveAll(c => c.Key == x.Key);
			}
		}
	}
}
																																																									  