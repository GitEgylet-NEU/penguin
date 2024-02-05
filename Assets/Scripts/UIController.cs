using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
	public static UIController instance;

	public UIDocument mainDoc, gameOverDoc;

	public Button layoutButton, manualButton, creditButton, restartButton;
	public VisualElement playElement, manualElement, creditElement, popupElement;
	public Label gameoverTitle;
	public bool gameOn = false;
	public bool move = false;

	[Space]
	public UnityEngine.UI.Button unityRestartButton;
	public TextMeshProUGUI unityGameOverText;

	void Awake()
	{
		instance = this;

		mainDoc.enabled = true;

		var gameRoot = gameOverDoc.GetComponent<UIDocument>().rootVisualElement;
		var root = mainDoc.GetComponent<UIDocument>().rootVisualElement;

		gameOverDoc.enabled = false;

		//Visualelements
		creditElement = root.Q("creditelement");
		manualElement = root.Q("manualelement");
		playElement = root.Q("space");
		playElement.RegisterCallback<ClickEvent>(OnPlayClicked);
		popupElement = root.Q("popup");

		//Buttons
		layoutButton = root.Q<Button>("layout");
		layoutButton.RegisterCallback<ClickEvent>(OnLayoutClicked);

		manualButton = root.Q<Button>("manual");
		manualButton.RegisterCallback<ClickEvent>((_) => ActivateLayer(manualElement, creditElement, popupElement));

		creditButton = root.Q<Button>("credit");
		creditButton.RegisterCallback<ClickEvent>((_) => ActivateLayer(creditElement, manualElement, popupElement));

		//gameover/win
		restartButton = gameRoot.Q<Button>("restart");
		restartButton.RegisterCallback<ClickEvent>((_) => RestartGame());

		//egyéb
		gameoverTitle = gameRoot.Q<Label>("title");

		//make sure all menus are hidden
		ActivateLayer(playElement, popupElement, creditElement, manualElement);
	}

	void OnLayoutClicked(ClickEvent evt)
	{
		//Debug.Log("layout");
		SceneManager.LoadScene("LayoutPlanning");
	}
	private void OnPlayClicked(ClickEvent evt)
	{
		move = true;
		gameOn = true;
		mainDoc.enabled = false;
		TeamManager.instance.StartGame();
	}

	/// <param name="visual"><see cref="VisualElement"/> to enable</param>
	/// <param name="antivisual"><see cref="VisualElement"/>s to disable</param>
	public void ActivateLayer(VisualElement visual, params VisualElement[] antivisual)
	{
		if (visual.style.display == DisplayStyle.Flex)
		{
			visual.style.display = DisplayStyle.None;
			playElement.style.display = DisplayStyle.Flex;
		}
		else
		{
			playElement.style.display = DisplayStyle.None;
			visual.style.display = DisplayStyle.Flex;
			foreach (var av in antivisual)
			{
				av.style.display = DisplayStyle.None;
			}

		}
	}
	public void RestartGame()
	{
		SceneManager.LoadScene("Samplescene");
	}
	public void EndGame(bool won)
	{
		unityGameOverText.text = won ? "YOU won" : "YOU lost";
		unityGameOverText.gameObject.SetActive(true);
		unityRestartButton.GetComponentInChildren<TextMeshProUGUI>().text = won ? "Hurrá!" : "Újrakezdés";
		unityRestartButton.gameObject.SetActive(true);
	}

	//dokumentumok ki/be kapcsolása
	public void ToggleDocument(UIDocument document, bool state)
	{
		document.enabled = state;
	}
	public void SetWin()
	{
		restartButton.text = ("Hurráá");
		gameoverTitle.text = ("YOUwin");
		ToggleDocument(gameOverDoc, true);
	}
	public void SetPopup(string titleText, string text)
	{
		//Debug.Log($"setPopup: {titleText}, {text}");
		mainDoc.GetComponent<UIDocument>().rootVisualElement.Q<Label>("title").text = titleText;
		mainDoc.GetComponent<UIDocument>().rootVisualElement.Q<Label>("text").text = text;
		mainDoc.GetComponent<UIDocument>().rootVisualElement.Q<Button>("ok").RegisterCallback<ClickEvent>((_) => ActivateLayer(playElement, popupElement));
	}
}
