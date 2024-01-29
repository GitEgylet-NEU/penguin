using NohaSoftware.Utilities;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public LevelData levelData;
    public GameObject blankSection;
    float z = 10;
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
            Instantiate(blankSection, new Vector3(0, -0.1f, z + 5), Quaternion.identity);
            z += 10;
        }
    }

    public void Update()
    {
        // endless generálás
        if (TeamManager.instance.penguins[0].gameObject.transform.position.z > z - renderdistance)
        {
            
            
            if (UIController.instance.gameon == false)
            {
                Debug.Log("pregenerate");
                Instantiate(blankSection, new Vector3(0, -0.1f, z + 5), Quaternion.identity);
                z += 10;
            }
            else
            {
                Debug.Log("generate");
                var next = GetNextLevelFragment();
                lastLevelId = next.id;
                Instantiate(next.prefab, new Vector3(0, -0.1f, z + next.length / 2), Quaternion.identity);
                z += next.length;
            }
            
        }
    }

    //különbözõ generálásának a biztosítása
    public LevelData.LevelFragment GetNextLevelFragment()
    {
        return levelData.levelFragments.GetRandom(f => f.id != lastLevelId);
    }
}
