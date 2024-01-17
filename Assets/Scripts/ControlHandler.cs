using UnityEngine;

public class ControlHandler : MonoBehaviour
{
	public bool handheld;

	private void Awake()
	{
		Time.timeScale = 0f;
	}

	void Start()
	{
		handheld = SystemInfo.deviceType == DeviceType.Handheld;
		if (!handheld) Debug.LogWarning("Device is not handheld");
	}

	// Update is called once per frame
	void Update()
	{
		HandleMovement();
	}

	void HandleMovement()
	{
		if (Input.touchCount == 0)
		{
			TeamManager.instance.Move(Input.GetAxisRaw("Horizontal"));
		}
		else
		{
			Touch touch = Input.GetTouch(0);

			float horizontal = (touch.position.x - (Screen.width / 2f)) / (Screen.width / 2f);
			if (horizontal < 0 && horizontal > -.5f) horizontal = -.5f;
			else if (horizontal > 0 && horizontal < .5f) horizontal = .5f;

			TeamManager.instance.Move(horizontal);
		}
	}

	public void StartGame()
	{
		Time.timeScale = 1f;
	}
}
