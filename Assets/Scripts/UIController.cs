using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Data;

public class UIController : MonoBehaviour
{
	public static UIController instance;
	private void Awake()
	{
		instance = this;
	}

	public UIDocument mainDoc, gameOverDoc;

	public Button layoutButton, manualButton, creditButton, restartButton, okButton;
	public VisualElement playElement, manualElement, creditElement, popupElement;
	public Image icon;
	public Label popupText, popupTitle, gameoverTitle;
	public bool gameOn = false;
	
	void Start()
	{
		mainDoc.enabled = true;

		var gameRoot = gameOverDoc.GetComponent<UIDocument>().rootVisualElement;
		var root = mainDoc.GetComponent<UIDocument>().rootVisualElement;
		
		gameOverDoc.enabled = false;

		//Visualelements
		creditElement = root.Q("creditelement");
		manualElement = root.Q("manualelement");
		playElement = root.Q("space");
		playElement.RegisterCallback<ClickEvent>(OnPlayClicked);

		//Buttons
			layoutButton = root.Q<Button>("layout");
			layoutButton.RegisterCallback<ClickEvent>(OnLayoutClicked);
		
			manualButton = root.Q<Button>("manual");
			manualButton.RegisterCallback<ClickEvent>((_) => ActivateLayer(manualElement, creditElement));

			creditButton = root.Q<Button>("credit");
			creditButton.RegisterCallback<ClickEvent>((_) => ActivateLayer(creditElement, manualElement));

				//gameover/win
				restartButton = gameRoot.Q<Button>("restart");
				restartButton.RegisterCallback<ClickEvent>(RestartGame);

				//popup
				okButton = root.Q<Button>("ok");
				okButton.RegisterCallback<ClickEvent>((_) => ActivateLayer(playElement, popupElement));

		//egyéb
		icon = root.Q<Image>("icon");
		popupText = root.Q<Label>("text");
		popupTitle = root.Q<Label>("title");
		gameoverTitle = gameRoot.Q<Label>("title");

	}

	void OnLayoutClicked(ClickEvent evt)
	{
		Debug.Log("layout");
		SceneManager.LoadScene("LayoutPlanning");
	}
	private void OnPlayClicked(ClickEvent evt)
	{
		gameOn = true;
		mainDoc.enabled = false;
		TeamManager.instance.StartGame();
	}
	public void ActivateLayer(VisualElement visual, VisualElement antivisual) 
	{
		Debug.Log("activatelayer");
		Debug.Log(visual.style.display);
		if (visual.style.display == DisplayStyle.Flex)
		{
			visual.style.display = DisplayStyle.None;
			playElement.style.display = DisplayStyle.Flex;
		}
		else
		{
			visual.style.display = DisplayStyle.Flex;
			playElement.style.display= DisplayStyle.None;
			antivisual.style.display = DisplayStyle.None;
		}
	}
	public void RestartGame(ClickEvent evt)
	{
		SceneManager.LoadScene("Samplescene");
	}

	//dokumentumok ki/be kapcsolása
	public void onOff(UIDocument document,bool state)
	{
		document.enabled = state;
	}
	public void setWin() 
	{
		restartButton.text = ("Hurráá");
		gameoverTitle.text = ("YOUwin");
		onOff(gameOverDoc, true);
	}
	public void setPopup(string titleText, string text, bool iconActive)
	{
		icon.style.display = iconActive ? DisplayStyle.Flex : DisplayStyle.None;
		popupTitle.text = titleText;
		popupText.text = text;
	}
}
