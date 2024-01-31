using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    private void Awake()
    {
        instance = this;
    }

    public UIDocument mainDoc;

    public Button layoutButton, manualButton, creditButton;
    public VisualElement playelement, manualelement, creditelement;
    public bool gameon = false;
    
    void Start()
    {
        var root = mainDoc.GetComponent<UIDocument>().rootVisualElement;

        //Visualelements
        creditelement = root.Q("creditelement");
        manualelement = root.Q("manualelement");
        playelement = root.Q("space");
        playelement.RegisterCallback<ClickEvent>(OnPlayClicked);

        //Buttons
        layoutButton = root.Q<Button>("layout");
        layoutButton.RegisterCallback<ClickEvent>(OnLayoutClicked);
        
        manualButton = root.Q<Button>("manual");
        manualButton.RegisterCallback<ClickEvent>((_) => ActivateLayer(manualelement, creditelement));

        creditButton = root.Q<Button>("credit");
        creditButton.RegisterCallback<ClickEvent>((_) => ActivateLayer(creditelement, manualelement));

        
    }

    void OnLayoutClicked(ClickEvent evt)
    {
        Debug.Log("layout");
        SceneManager.LoadScene("LayoutPlanning");
    }
    private void OnPlayClicked(ClickEvent evt)
    {
        gameon = true;
        mainDoc.enabled = false;
        TeamManager.instance.StartGame();
    }
    public void ActivateLayer(VisualElement visual, VisualElement antivisual) 
    {
        if (visual.style.display != DisplayStyle.None)
        {
            visual.style.display = DisplayStyle.None;
            playelement.style.display = DisplayStyle.Flex;
        }
        else
        {
            visual.style.display = DisplayStyle.Flex;
            playelement.style.display= DisplayStyle.None;
            antivisual.style.display = DisplayStyle.None;
        }
    }
}
