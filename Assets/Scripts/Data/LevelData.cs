using UnityEngine;

[CreateAssetMenu(menuName = "Data/Level Data")]
public class LevelData : ScriptableObject
{
	public LevelFragment[] levelFragments;

	[System.Serializable]
	public class LevelFragment
	{
		public string id;
		public GameObject prefab;
		public float length;

		[Header("Sockets")]
		public Socket frontSocket;
		public Socket rearSocket;

		public LevelFragment(string id, GameObject prefab, float length = 0f)
		{
			this.id = id;
			this.prefab = prefab;
			if (length == 0f) FindLength();
			else this.length = length;
		}

		void FindLength()
		{
			if (prefab == null) return;
			try
			{
				length = prefab.transform.Find("ground").localScale.z;
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				Debug.LogWarning("Can't determine length for LevelFragment " + id + ", setting to zero");
				length = 0f;
			}
		}

		[System.Serializable]
		public struct Socket
		{
			public bool left, center, right;

			public Socket(bool left, bool center, bool right)
			{
				this.left = left;
				this.center = center;
				this.right = right;
			}

			public bool IsCompatible(Socket other)
			{
				if (left && other.left) return true;
				if (center && other.center) return true;
				if (right && other.right) return true;
				return false;
			}
		}
	}
}
