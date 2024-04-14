using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
	[SerializeField] GraphicRaycaster raycaster;
	[SerializeField] BattleLayoutPlanner layoutPlanner;
	List<RaycastResult> results = new();
	Dictionary<string, bool> canSelect;
	Dictionary<string, GameObject> buttons;

	bool init = false;
	public void Init(IEnumerable<PenguinData> characters)
	{
		canSelect = new Dictionary<string, bool>();

		GameObject prefab = transform.Find("CharacterTemplate").gameObject;
		prefab.SetActive(false);
		if (prefab == null) return;

		buttons = new();
		foreach (var c in characters)
		{
			if (SaveManager.instance.progressData.characterLevels.GetElement(c.name).Value == -1) continue; //not unlocked
			var obj = Instantiate(prefab, transform);
			obj.name = c.name;
			//obj.transform.Find("Icon").GetComponent<Image>().color = c.color;
			obj.transform.Find("Icon").GetComponent<Image>().sprite = c.frontSprite;
			obj.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = c.name;
			obj.SetActive(true);
			canSelect.Add(c.name, true);
			buttons.Add(c.name, obj);
		}

		GetComponent<RectTransform>().SetWidth(50f + (transform.childCount-1) * 190f + (transform.childCount - 2) * 25f);

		init = true;
	}

	private void Update()
	{
		if (!init) return;
		HandleDrags();

		foreach (Transform t in transform)
		{
			if (t.gameObject.name == "CharacterTemplate") continue;
			var c = layoutPlanner.gameData.GetPenguinData(t.gameObject.name);
			if (c == null) continue;
			canSelect[c.name] = layoutPlanner.Count(c.name) < c.levels[SaveManager.instance.progressData.characterLevels.GetElement(c.name).Value].maxNumber;
			buttons[c.name].GetComponent<Image>().color = canSelect[c.name] ? Color.white : Color.gray;
		}
	}

	bool dragging = false;
	bool mouse;
	GameObject dragObj;
	string id;

	public void IgnoreTouch(bool set) => ignoreTouch = set;
	public bool ignoreTouch;
	void HandleDrags()
	{
		if (ignoreTouch) return;
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetMouseButtonDown(0) || (!dragging && Input.touchCount == 1))
		{
			mouse = Input.touchCount == 0;
			//start drag
			id = string.Empty;
			GraphicRaycast();
			var a = results.Where(r => r.gameObject.layer == LayerMask.NameToLayer("Receive Graphic Raycast"));
			if (a.Any())
			{
				id = a.FirstOrDefault().gameObject.name;
			}

			if (string.IsNullOrEmpty(id))
			{
				//remove

				try
				{
					Transform marker = layoutPlanner.GetMarker(mousePos);
					if (marker != null)
					{
						var coord = marker.gameObject.name.Split(';').Select(int.Parse).ToArray();
						layoutPlanner.SetCharacter(coord[0], coord[1], string.Empty);
					}
				}
				catch { }
				return;
			}
			else
			{
				PenguinData character = layoutPlanner.gameData.GetPenguinData(id);
				if (character == null) return;
				if (!canSelect[character.name]) return;

				dragObj = Instantiate(layoutPlanner.penguinPrefab);
				dragObj.name = "drag " + character.name;
				dragObj.GetComponent<SpriteRenderer>().sprite = character.frontSprite;
				dragObj.SetActive(true);

				dragging = true;
			}
		}
		else if (!string.IsNullOrEmpty(id) && ((mouse && Input.GetMouseButtonUp(0)) || (dragging && !mouse && Input.touchCount == 0)))
		{
			try
			{
				Transform marker = layoutPlanner.GetMarker(mousePos);
				if (marker != null)
				{
					var coord = marker.gameObject.name.Split(';').Select(int.Parse).ToArray();
					layoutPlanner.SetCharacter(coord[0], coord[1], id);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}

			Destroy(dragObj);

			dragging = false;
			id = string.Empty;
		}

		if (dragging)
		{
			dragObj.transform.position = mousePos;
		}
	}

	void GraphicRaycast()
	{
		PointerEventData ped = new PointerEventData(EventSystem.current);
		ped.position = Input.mousePosition;
		results.Clear();
		raycaster.Raycast(ped, results);
	}
}
