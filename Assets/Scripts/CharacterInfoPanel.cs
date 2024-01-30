using NohaSoftware.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoPanel : MonoBehaviour
{
	[HideInInspector] public CharacterData characterData;

	public GameData gameData;

	[Header("Character Selector")]
	[SerializeField] GameObject characterTemplate;
	[SerializeField] RectTransform scrollContent;

	[Header("Info Panel")]
	[SerializeField] RectTransform infoPanel;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI hpText;
	public TextMeshProUGUI hpsText;
	public TextMeshProUGUI damageText;
	public TextMeshProUGUI rangeText;
	public TextMeshProUGUI speedText;
	public TextMeshProUGUI kiteText;
	public TextMeshProUGUI descriptionText;
	public Image icon;

	private void OnEnable()
	{
		if (gameData == null) return;
		foreach (Transform t in scrollContent)
		{
			if (t != characterTemplate.transform) Destroy(t.gameObject);
		}
		foreach (CharacterData data in gameData.playerCharacters)
		{
			GameObject obj = Instantiate(characterTemplate, scrollContent);
			obj.name = data.name;
			obj.GetComponentInChildren<TextMeshProUGUI>().text = data.name;
			obj.transform.Find("Icon").GetComponent<Image>().color = data.color;
			obj.GetComponent<Button>().onClick.AddListener(() => { SetCharacter(data); });
			obj.SetActive(true);
		}
		SetCharacter(null);
	}

	public void SetCharacter(CharacterData newCharacter)
	{
		characterData = newCharacter;
		UpdateUI();
	}

	public void UpdateUI()
	{
		if (characterData == null)
		{
			infoPanel.gameObject.SetActive(false);
			return;
		}
		infoPanel.gameObject.SetActive(true);
		nameText.text = characterData.name;
		hpText.text = characterData.maxHealth.ToString();
		hpsText.text = characterData.hitsPerSecond.ToString();
		damageText.text = characterData.damagePerHit.ToString();
		rangeText.text = characterData.range.ToString();
		speedText.text = characterData.speed.ToString();
		kiteText.text = characterData.shouldMoveBack ? "igen" : "nem";
		descriptionText.text = characterData.description;
		descriptionText.GetComponent<RectTransform>().SetHeight(descriptionText.GetPreferredValues(characterData.description).y);
		icon.color = characterData.color;
	}
}
