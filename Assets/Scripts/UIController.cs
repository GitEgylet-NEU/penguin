using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    private void Awake()
    {
        instance = this;
    }

    public UIDocument mainDoc;

    public Button layout;
    public VisualElement play;
    public bool gameon = false;
    
    void Start()
    {
        var root = mainDoc.GetComponent<UIDocument>().rootVisualElement;

        layout = root.Q("layout") as Button;
        layout.RegisterCallback<ClickEvent>(OnLayoutClicked);

        play = root.Q("space") as VisualElement;
        play.RegisterCallback<ClickEvent>(OnPlayClicked);
    }

    void OnLayoutClicked(ClickEvent evt)
    {
        Debug.Log("layout");
        SceneManager.LoadScene("LayoutPlanning");
    }
    private void OnPlayClicked(ClickEvent evt)
    {
        Debug.Log("play");
        gameon = true;
        mainDoc.enabled = false;
        TeamManager.instance.runMultiplier = 1.05F;
    }
}
