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

	bool init = false;
	public void Init(IEnumerable<CharacterData> characters)
	{
		GameObject prefab = transform.Find("CharacterTemplate").gameObject;
		prefab.SetActive(false);
		if (prefab == null) return;
		foreach (var c in characters)
		{
			var obj = Instantiate(prefab, transform);
			obj.name = c.id;
			obj.transform.Find("Icon").GetComponent<Image>().color = c.color;
			obj.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = c.name;
			obj.SetActive(true);
		}
		init = true;
	}

	private void Update()
	{
		if (!init) return;
		HandleDrags();
	}

	bool dragging = false;
	bool mouse;
	GameObject dragObj;
	string id;
	void HandleDrags()
	{
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
				CharacterData character = layoutPlanner.GetCharacter(id);
				if (character == null) return;

				dragObj = Instantiate(layoutPlanner.penguinPrefab);
				dragObj.name = "drag " + character.id;
				dragObj.GetComponent<SpriteRenderer>().color = character.color;
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
