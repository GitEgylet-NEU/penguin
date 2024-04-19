using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleSelector : MonoBehaviour
{
	bool running = false;

	private void Start()
	{
		try
		{
			ChangeLocale(PlayerPrefs.GetInt("localeID"));
		}
		catch
		{
			throw;
		}
	}

	public void ChangeLocale(int localeID)
	{
		if (running) return;
		StartCoroutine(SetLocale(localeID));
	}

	IEnumerator SetLocale(int localeID)
	{
		running = true;
		yield return LocalizationSettings.InitializationOperation;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
		PlayerPrefs.SetInt("localeID", localeID);
		running = false;
	}
}