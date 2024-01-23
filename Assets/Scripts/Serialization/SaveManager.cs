using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	public static SaveManager instance;
	private void Awake()
	{
		instance = this;
	}

	public string layoutSavePath = "Data/Layouts";

	public bool LoadSaveData(string name, out BattleLayout layout)
	{
		layout = null;
		string filePath = Path.Combine(Application.persistentDataPath, layoutSavePath, name + ".layout");
		if (!File.Exists(filePath))
		{
			Debug.LogWarning($"Couldn't load BattleLayout at {filePath}");
			return false;
		}

		FileStream file = File.OpenRead(filePath);

		BinaryFormatter bf = new BinaryFormatter();
		//TODO: surrogates

		try
		{
			layout = bf.Deserialize(file) as BattleLayout;
			file.Close();
			return true;
		}
		catch (System.Exception)
		{
			Debug.LogError($"Couldn't deserialize BattleLayout ({name})");
			file.Close();
			return false;
		}
	}
	public bool SaveLayout(string name, BattleLayout layout)
	{
		string directory = Path.Combine(Application.persistentDataPath, layoutSavePath);
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		string filePath = Path.Combine(directory, name + ".layout");
		FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write);
		BinaryFormatter bf = new BinaryFormatter();

		try
		{
			bf.Serialize(file, layout);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Couldn't save layout to {filePath}");
			Debug.LogException(e);
			file.Close();
			return false;
		}
		file.Close();
		return true;
	}
}