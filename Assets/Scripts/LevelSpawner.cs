using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
	public LevelData levelData;
	public GameObject blankSection;
	public GameObject tree;
	public List<GameObject> sections;
	public List<GameObject> trees;
	float z = 0;
	string lastLevelId;

	//megadja hogy mennyire elõre generáljon
	public int renderdistance = 30;

	public void Start()
	{
		//hosszúság bekérése
		foreach (var fragment in levelData.levelFragments)
		{
			if (fragment.length == 0) fragment.length = fragment.prefab.transform.Find("ground").localScale.z;
		}

		// elsõ blokkok generálása
		//sections.Add(Instantiate(blankSection, new Vector3(0, -0.1f, 5), Quaternion.identity));
		trees.Add(Instantiate(tree, new Vector3(6, 3, 1), Quaternion.identity));
		trees.Add(Instantiate(tree, new Vector3(-6, 3, 1), Quaternion.identity));

		while (z < renderdistance)
		{
			Debug.Log("pregenerate");
			sections.Add(Instantiate(blankSection, new Vector3(0, -0.1f, z + 5), Quaternion.identity));
			z += 10;
		}
	}

	public void Update()
	{
		// endless generálás
		if (TeamManager.instance.penguins[0].gameObject.transform.position.z > z - renderdistance)
		{
			
			//elindult e már a játék elenõrzése
			if (UIController.instance.gameon == false)
			{
				sections.Add(Instantiate(blankSection, new Vector3(0, -0.1f, z + 5), Quaternion.identity));
				z += 10;
			}
			else
			{
				if ((TeamManager.instance.runStartZ + TeamManager.instance.runLength) - (sections.Last().transform.position.z + (sections.Last().transform.Find("ground").localScale.z / 2) ) <= 10)
				{
					Debug.Log("battle");
					UIController.instance.gameon = false;
				}
				else
				{
					var next = GetNextLevelFragment();
					Debug.Log(next.id);
					lastLevelId = next.id;
					sections.Add(Instantiate(next.prefab, new Vector3(0, -0.1f, z + next.length / 2), Quaternion.identity));
					z += next.length;
				}
			}
			
		}
		//fa generálása
			while (trees.Last().transform.position.z < TeamManager.instance.penguins[0].transform.position.z + renderdistance)
			{
				trees.Add(Instantiate(tree, new Vector3(6, 3, trees.Last().transform.position.z + Random.Range(2,5)), Quaternion.Euler(new Vector3(0,Random.Range(-30,30),0))));
				trees.Add(Instantiate(tree, new Vector3(-6, 3, trees.Last().transform.position.z + Random.Range(2, 5)), Quaternion.Euler(new Vector3(0, Random.Range(-30, 30), 0))));
			}

		//objectek törlése (fa + section)
		if (trees[0].transform.position.z < TeamManager.instance.penguins[0].transform.position.z - renderdistance/2)
		{
			DestroyObject(trees, 0);
		}
		if ((sections[0].transform.position.z + (sections[0].transform.Find("ground").localScale.z / 2) + 10) < TeamManager.instance.penguins.Last().transform.position.z )
		{
			DestroyObject(sections, 0);
		}
	}

	//különbözõ generálásának a biztosítása
	public LevelData.LevelFragment GetNextLevelFragment()
	{
		return levelData.levelFragments.GetRandom(f => f.id != lastLevelId && 
		f.length <= (TeamManager.instance.runStartZ + TeamManager.instance.runLength) - (sections.Last().transform.position.z + sections.Last().transform.Find("ground").localScale.z / 2));
	}

	//törlés
	public void DestroyObject(List<GameObject> list ,int id)
	{
		var obj = list[id];
		list.RemoveAt(id);
		Destroy(obj.gameObject);
	}
}
