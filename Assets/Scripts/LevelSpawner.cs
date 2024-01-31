using NohaSoftware.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class LevelSpawner : MonoBehaviour
{
    public LevelData levelData;
    public GameObject blankSection;
    public GameObject tree;
    public List<GameObject> sections;
    public List<GameObject> trees;
    float z = 10;
    public int treespace = 10;
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
        sections.Add(Instantiate(blankSection, new Vector3(0, -0.1f, 5), Quaternion.identity));
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
                var next = GetNextLevelFragment();
                lastLevelId = next.id;
                sections.Add(Instantiate(next.prefab, new Vector3(0, -0.1f, z + next.length / 2), Quaternion.identity));
                z += next.length;
            }
            
        }
        if (trees.Last().transform.position.z < TeamManager.instance.penguins[0].transform.position.z + renderdistance)
        {

            //int counter = treespace;
            while (trees.Last().transform.position.z < TeamManager.instance.penguins[0].transform.position.z + renderdistance)
            {
                trees.Add(Instantiate(tree, new Vector3(6, 3, trees.Last().transform.position.z + Random.Range(2,5)), Quaternion.Euler(new Vector3(0,Random.Range(-30,30),0))));
                trees.Add(Instantiate(tree, new Vector3(-6, 3, trees.Last().transform.position.z + Random.Range(2, 5)), Quaternion.Euler(new Vector3(0, Random.Range(-30, 30), 0))));
                //counter+= treespace;
            }
        }
        if (trees[0].transform.position.z < TeamManager.instance.penguins.Last().transform.position.z - renderdistance)
        {
            DestroySection(trees, 0);
        }
        if ((sections[0].transform.position.z + (sections[0].transform.Find("ground").localScale.z / 2) + 10) < TeamManager.instance.penguins.Last().transform.position.z )
        {
            DestroySection(sections, 0);
        }
    }

    //különbözõ generálásának a biztosítása
    public LevelData.LevelFragment GetNextLevelFragment()
    {
        return levelData.levelFragments.GetRandom(f => f.id != lastLevelId);
    }

    public void DestroySection(List<GameObject> list ,int id)
    {
        var obj = list[id];
        list.RemoveAt(id);
        Destroy(obj.gameObject);
    }
}
