using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Battle Layout", fileName = "New Battle Layout")]
public class PremadeBattleLayout : ScriptableObject
{
	public List<SerializableTuple<Vector2Int, EnemyData>> characters; // Tuple<Tuple<column, row>, characterID>
	public int Difficulty
	{
		get
		{
			int diff = 0;
			foreach (var character in characters)
			{
				diff += character.Value.difficulty;
			}
			return diff;
		}
	}

	public string[,] GetMatrix()
	{
		if (characters == null || characters.Count == 0) return null;
		int x = characters.Select(c => c.Key.x).Max();
		int y = characters.Select(c => c.Key.y).Max();
		string[,] matrix = new string[x + 1, y + 1];
		foreach (var c in characters.ToArray())
		{
			if (c.Key.x < 0 || c.Key.y < 0) continue;
			matrix[c.Key.x, c.Key.y] = c.Value.name;
		}
		return matrix;
	}
	//public void LoadMatrix(string[,] matrix)
	//{
	//	characters = new();
	//	for (int col = 0; col < matrix.GetLength(0); col++)
	//	{
	//		for (int row = 0; row < matrix.GetLength(1); row++)
	//		{
	//			if (!string.IsNullOrEmpty(matrix[col, row])) characters.Add(new(new(col, row), matrix[col, row]));
	//		}
	//	}
	//}
}
