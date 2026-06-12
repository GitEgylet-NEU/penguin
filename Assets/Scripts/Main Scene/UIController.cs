using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public bool gameOn = false;
    public bool move = false;

    [Header("Main Menu")]
    public GameObject mainMenu;
    public GameObject tapToPlayText;
    public GameObject manualObject, creditsObject;
    [Space]
    public LocalizeStringEvent xpLevelLocalizer;
    public LocalizeStringEvent upgradePointsLocalizer;
    public TextMeshProUGUI levelText, upgradePointText;
    [Space]
    public GameObject popupObject;
    public TextMeshProUGUI popupTitle, popupText;
    public Button popupButton;

    [Header("Run")]
    public ProgressBar runProgressBar;

    [Header("Game Over")]
    public GameObject gameOverObject;
    public TextMeshProUGUI gameOverText;
    public LocalizeStringEvent restartButtonLocalizer;
    [SerializeField] LocalizedString hoorayString, restartString;
    public GameObject lostXpWindow;
    public TextMeshProUGUI lostXpText;
    public GameObject wonXpWindow;
    public TextMeshProUGUI wonXpText;

    void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        xpLevelLocalizer.StringReference.Arguments = new object[1];
        upgradePointsLocalizer.StringReference.Arguments = new object[1];
    }

    private void Start()
    {
        gameOverObject.SetActive(false);
        mainMenu.SetActive(true);

        runProgressBar.min = 0;

        xpLevelLocalizer.StringReference.Arguments[0] = SaveManager.instance.progressData.level + 1;
        xpLevelLocalizer.RefreshString();
        upgradePointsLocalizer.StringReference.Arguments[0] = SaveManager.instance.progressData.upgradePoints;
        upgradePointsLocalizer.RefreshString();
    }

    public void OnLayoutClicked()
    {
        SceneManager.LoadScene("LayoutPlanning");
    }
    public void OnPlayClicked()
    {
        move = true;
        gameOn = true;
        mainMenu.SetActive(false);
        TeamManager.instance.StartGame();
    }
    public void OnManualClicked()
    {
        ActivateLayer(manualObject, creditsObject, tapToPlayText);
    }
    public void OnCreditsClicked()
    {
        ActivateLayer(creditsObject, manualObject, tapToPlayText);
    }

    /// <summary>Activates a UI layer and deactivates others.</summary>
    /// <param name="visual"><see cref="GameObject"/> to enable.</param>
    /// <param name="antivisual"><see cref="GameObject"/>s to disable.</param>
    public void ActivateLayer(GameObject visual, params GameObject[] antivisual)
    {
        if (visual.activeInHierarchy)
        {
            visual.SetActive(false);
            tapToPlayText.SetActive(true);
            mainMenu.GetComponent<Button>().interactable = true;
        }
        else
        {
            tapToPlayText.SetActive(false);
            visual.SetActive(true);
            foreach (var av in antivisual)
            {
                av.SetActive(false);
            }
            mainMenu.GetComponent<Button>().interactable = false;
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void EndGame(bool won, float xp)
    {
        if (won)
        {
            gameOverText.text = "YOU won";
            restartButtonLocalizer.StringReference = hoorayString;
            restartButtonLocalizer.RefreshString();
            if (xp > 0f)
            {
                wonXpText.text = $"{xp} XP";
                wonXpWindow.SetActive(true);
            }
        }
        else
        {
            gameOverText.text = "YOU lost";
            restartButtonLocalizer.StringReference = restartString;
            restartButtonLocalizer.RefreshString();
            if (xp > 0f)
            {
                lostXpText.text = $"<s>{xp} XP</s>   ►   <b>{xp / 2f} XP";
                lostXpWindow.SetActive(true);
            }
        }

        gameOverObject.SetActive(true);
    }

    public void SetPopup(string titleText, string text)
    {
        //Debug.Log($"setPopup: {titleText}, {text}");
        popupTitle.text = titleText;
        popupText.text = text;
        popupButton.onClick.AddListener(() => ActivateLayer(tapToPlayText, popupObject));
        ActivateLayer(popupObject, manualObject, creditsObject);
    }
}
