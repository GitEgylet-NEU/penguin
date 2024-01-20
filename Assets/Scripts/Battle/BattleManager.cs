using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	public static BattleManager instance;
	private void Awake()
	{
		instance = this;
		participants = new();
	}

	[SerializeField] Transform battlefield;
	[SerializeField][Min(0)] int columns;
	[SerializeField][Min(0)] int rows;
	[SerializeField][Min(0)] float padding;

	[Header("Battle")]
	public List<BattleParticipant> participants;

	private void OnDrawGizmos()
	{
		if (battlefield == null || columns < 1 || rows < 1) return;
		float distX = (battlefield.localScale.x - padding * 2) / (columns - 1);
		float distY = (battlefield.localScale.y - padding * 2) / (rows - 1);
		Gizmos.color = Color.yellow;
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				float x = battlefield.position.x - battlefield.localScale.x / 2f + padding + j * distX;
				float y = battlefield.position.y - battlefield.localScale.y / 2f + padding + i * distY;
				Gizmos.DrawSphere(new Vector2(x, y), .2f);
			}
		}
	}
}
