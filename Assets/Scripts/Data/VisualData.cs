using NohaSoftware.Utilities;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Visual Data")]
public class VisualData : ScriptableObject
{
	public List<SerializableTuple<string, GameObject>> abilityParticles;
}