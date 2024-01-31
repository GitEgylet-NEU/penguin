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
	public string progressSavePath = "Data/progress.data";

	public ProgressData progressData;

	public void LoadProgress()
	{
		if (!LoadObject(progressSavePath, out progressData))
		{
			progressData = new ProgressData();
		}
	}
	public void SaveProgress()
	{
		SaveObject(progressSavePath, progressData);
	}

	public bool LoadObject(string path, out object result)
	{
		result = null;
		string filePath = Path.Combine(Application.persistentDataPath, path);
		if (!File.Exists(filePath))
		{
			Debug.LogWarning($"Couldn't load from {filePath}");
			return false;
		}

		FileStream file = File.OpenRead(filePath);

		BinaryFormatter bf = new BinaryFormatter();
		//TODO: surrogates

		try
		{
			result = bf.Deserialize(file);
			file.Close();
			return true;
		}
		catch (System.Exception)
		{
			Debug.LogError($"Couldn't deserialize {path}");
			file.Close();
			return false;
		}
	}
	public bool LoadObject<T>(string path, out T result) where T : class
	{
		result = null;
		string filePath = Path.Combine(Application.persistentDataPath, path);
		if (!File.Exists(filePath))
		{
			Debug.LogWarning($"Couldn't load from {filePath}");
			return false;
		}

		FileStream file = File.OpenRead(filePath);

		BinaryFormatter bf = new BinaryFormatter();
		//TODO: surrogates

		try
		{
			result = bf.Deserialize(file) as T;
			file.Close();
			return true;
		}
		catch (System.Exception)
		{
			Debug.LogError($"Couldn't deserialize {path}");
			file.Close();
			return false;
		}
	}
	public bool SaveObject(string path, object obj)
	{
		string directory = Path.GetDirectoryName(Path.Combine(Application.persistentDataPath, path));
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		string filePath = Path.Combine(Application.persistentDataPath, path);
		FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write);
		BinaryFormatter bf = new BinaryFormatter();

		try
		{
			bf.Serialize(file, obj);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Couldn't save object to {filePath}");
			Debug.LogException(e);
			file.Close();
			return false;
		}
		file.Close();
		return true;
	}
}