using NohaSoftware.Utilities;
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
	public TextMeshProUGUI upgradePointText;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI maxNumberText;
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
			//obj.transform.Find("Icon").GetComponent<Image>().color = data.color;
			obj.transform.Find("Icon").GetComponent<Image>().sprite = data.frontSprite;
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
		upgradePointText.text = "Fejlesztési pontok: " + SaveManager.instance.progressData.upgradePoints;

		if (characterData == null)
		{
			infoPanel.gameObject.SetActive(false);
			return;
		}
		CharacterData.Level level = characterData.levels[SaveManager.instance.progressData.characterLevels.GetElement(characterData.id).Value];

		infoPanel.gameObject.SetActive(true);
		nameText.text = characterData.name;
		maxNumberText.text = level.maxNumber + " db";
		hpText.text = level.maxHealth.ToString();
		hpsText.text = level.hitsPerSecond.ToString();
		damageText.text = level.damagePerHit.ToString();
		rangeText.text = level.range.ToString();
		speedText.text = level.speed.ToString();
		kiteText.text = level.shouldMoveBack ? "igen" : "nem";
		descriptionText.text = characterData.description;
		descriptionText.GetComponent<RectTransform>().SetHeight(descriptionText.preferredHeight);

		//icon.color = characterData.color;
		icon.sprite = characterData.frontSprite;

		infoPanel.GetChild(0).GetChild(0).GetComponent<RectTransform>().SetHeight(Mathf.Abs(descriptionText.rectTransform.localPosition.y) + descriptionText.rectTransform.rect.height + 250f);
	}
}
