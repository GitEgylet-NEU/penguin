using NohaSoftware.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CharacterInfoPanel : MonoBehaviour
{
	[HideInInspector] public PenguinData characterData;
	[HideInInspector] public PenguinData.Level level;
	int levelIdx;

	public GameData gameData;

	[Header("Character Selector")]
	[SerializeField] GameObject characterTemplate;
	[SerializeField] RectTransform scrollContent;

	[Header("Info Panel")]
	[SerializeField] RectTransform infoPanel;
	public LocalizeStringEvent upgradePointString;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI maxNumberText;
	public TextMeshProUGUI hpText;
	public TextMeshProUGUI hpsText;
	public TextMeshProUGUI damageText;
	public TextMeshProUGUI rangeText;
	public TextMeshProUGUI speedText;
	public TextMeshProUGUI kiteText;
	public TextMeshProUGUI descriptionText;
	public TextMeshProUGUI levelText;
	public TextMeshProUGUI upgradeCostText;
	public Button upgradeButton;
	public Image icon;

	private void OnEnable()
	{
		if (gameData == null) return;
		upgradePointString.StringReference.Arguments = new object[1] { SaveManager.instance.progressData.upgradePoints };
		upgradePointString.RefreshString();
		foreach (Transform t in scrollContent)
		{
			if (t != characterTemplate.transform) Destroy(t.gameObject);
		}
		int i = 0;
		foreach (PenguinData data in gameData.playerCharacters)
		{
			GameObject obj = Instantiate(characterTemplate, scrollContent);
			obj.name = data.name;
			obj.GetComponentInChildren<TextMeshProUGUI>().text = data.LocalizedName;
			//obj.transform.Find("Icon").GetComponent<Image>().color = data.color;
			obj.transform.Find("Icon").GetComponent<Image>().sprite = data.frontSprite;
			obj.GetComponent<Button>().onClick.AddListener(() => { SetCharacter(data); });
			obj.SetActive(true);
			i++;
		}
		float height = i * 210f + (i - 1) * 20f + 40f;
		scrollContent.SetHeight(height);
		SetCharacter(null);
	}

	public void SetCharacter(PenguinData newCharacter)
	{
		characterData = newCharacter;
		UpdateUI();
	}

	public void UpgradeCharacter()
	{
		if (levelIdx < characterData.levels.Length - 1 && SaveManager.instance.progressData.upgradePoints >= gameData.characterUpgradeCosts[levelIdx])
		{
			SaveManager.instance.progressData.upgradePoints -= gameData.characterUpgradeCosts[levelIdx];
			SaveManager.instance.progressData.characterLevels.GetElement(characterData.name).Value++;
			UpdateUI();
		}
	}

	public void UpdateUI()
	{
		upgradePointString.StringReference.Arguments[0] = SaveManager.instance.progressData.upgradePoints;
		upgradePointString.RefreshString();

		if (characterData == null)
		{
			infoPanel.gameObject.SetActive(false);
			return;
		}
		levelIdx = SaveManager.instance.progressData.characterLevels.GetElement(characterData.name).Value;
		level = characterData.levels[levelIdx];

		infoPanel.gameObject.SetActive(true);
		nameText.text = characterData.LocalizedName;
		maxNumberText.text = level.maxNumber + " db";
		hpText.text = level.maxHealth.ToString();
		hpsText.text = level.hitsPerSecond.ToString();
		damageText.text = level.damagePerHit.ToString();
		rangeText.text = level.range.ToString();
		speedText.text = level.speed.ToString();
		//kiteText.text = level.shouldMoveBack ? "igen" : "nem";

		levelText.text = (levelIdx+1).ToString();
		if (levelIdx < characterData.levels.Length - 1)
		{
			upgradeCostText.text = gameData.characterUpgradeCosts[levelIdx].ToString();
			upgradeCostText.gameObject.SetActive(true);
			upgradeButton.interactable = true;
		}
		else
		{
			upgradeCostText.gameObject.SetActive(false);
			upgradeButton.interactable = false;
		}

		descriptionText.text = characterData.LocalizedDescription;
		descriptionText.GetComponent<RectTransform>().SetHeight(descriptionText.preferredHeight);

		//icon.color = characterData.color;
		icon.sprite = characterData.frontSprite;

		//infoPanel.GetChild(0).GetChild(0).GetComponent<RectTransform>().SetHeight(Mathf.Abs(descriptionText.rectTransform.localPosition.y) + descriptionText.rectTransform.rect.height + 250f);
	}
}
