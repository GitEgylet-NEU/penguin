using UnityEngine;

public class ControlHandler : MonoBehaviour
{
	public static ControlHandler instance;

	[SerializeField] bool showTutorial;
	[SerializeField] GameObject tutorialObject;
	public bool handheld;

	public bool canStrafe = true;

	private void Awake()
	{
		instance = this;
		if (showTutorial)
		{
			tutorialObject.SetActive(true);
			Time.timeScale = 0f;
		}
		else
		{
			tutorialObject.SetActive(false);
		}
	}

	void Start()
	{
		handheld = SystemInfo.deviceType == DeviceType.Handheld;
		if (!handheld) Debug.LogWarning("Device is not handheld");
	}

	void Update()
	{
		if (canStrafe) HandleStrafe();
	}

	void HandleStrafe()
	{
		if (Input.touchCount == 0)
		{
			TeamManager.instance.Move(Input.GetAxisRaw("Horizontal"));
		}
		else
		{
			Touch touch = Input.GetTouch(0);

			float horizontal = touch.position.x > 0f ? 1f : -1f;

			TeamManager.instance.Move(horizontal);
		}
	}

	public void StartGame()
	{
		Time.timeScale = 1f;
	}
}
