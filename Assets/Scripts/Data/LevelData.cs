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
	}
}
