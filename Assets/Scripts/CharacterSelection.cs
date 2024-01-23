using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
	[SerializeField] GraphicRaycaster raycaster;
	[SerializeField] BattleLayoutPlanner layoutPlanner;
	List<RaycastResult> results = new();

	private void Update()
	{
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
			Debug.Log("start");
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
				Transform marker = layoutPlanner.GetMarker(mousePos);
				if (marker != null)
				{
					var coord = marker.gameObject.name.Split(';').Select(int.Parse).ToArray();
					layoutPlanner.SetCharacter(coord[0], coord[1], string.Empty);
				}
				return;
			}
			else
			{
				Debug.Log(id);

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
			Debug.Log("end");
			Transform marker = layoutPlanner.GetMarker(mousePos);
			if (marker != null)
			{
				var coord = marker.gameObject.name.Split(';').Select(int.Parse).ToArray();
				layoutPlanner.SetCharacter(coord[0], coord[1], id);
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
