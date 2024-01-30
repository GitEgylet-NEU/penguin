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
        Instantiate(blankSection, new Vector3(0, -0.1f, 5), Quaternion.identity);

        while (z < renderdistance)
        {
            Debug.Log("pregenerate");
            var obj = Instantiate(blankSection, new Vector3(0, -0.1f, z + 5), Quaternion.identity);
            z += 10;
            sections.Add(obj);
        }

        trees = new();
    }

    public void Update()
    {
        // endless generálás
        if (TeamManager.instance.penguins[0].gameObject.transform.position.z > z - renderdistance)
        {
            
            //elindult e már a játék elenõrzése
            if (UIController.instance.gameon == false)
            {
                var obj = Instantiate(blankSection, new Vector3(0, -0.1f, z + 5), Quaternion.identity);
                z += 10;
                sections.Add(obj);
            }
            else
            {
                var next = GetNextLevelFragment();
                lastLevelId = next.id;
                 var obj = Instantiate(next.prefab, new Vector3(0, -0.1f, z + next.length / 2), Quaternion.identity);
                z += next.length;
                sections.Add(obj);
            }
            
        }
        if (trees.Last().transform.position.z < TeamManager.instance.penguins[0].transform.position.z + renderdistance)
        {

            int counter = 0;
            while (trees.Last().transform.position.z < TeamManager.instance.penguins[0].transform.position.z + renderdistance)
            {
                Instantiate(tree, new Vector3(6, 3, trees.Last().transform.position.z + counter), Quaternion.identity);
                Instantiate(tree, new Vector3(-6, 3, trees.Last().transform.position.z + counter), Quaternion.identity);
                counter+= treespace;
            }
        }
        if ((sections[0].transform.position.z + (sections[0].transform.Find("ground").localScale.z / 2) + 10) < TeamManager.instance.penguins.Last().transform.position.z )
        {
            DestroySection(0);
        }
    }

    //különbözõ generálásának a biztosítása
    public LevelData.LevelFragment GetNextLevelFragment()
    {
        return levelData.levelFragments.GetRandom(f => f.id != lastLevelId);
    }

    public void DestroySection(int id)
    {
        var obj = sections[id];
        sections.RemoveAt(id);
        Destroy(obj.gameObject);
    }
}
