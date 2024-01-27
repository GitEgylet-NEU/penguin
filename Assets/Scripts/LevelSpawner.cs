using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public GameObject[] levelChunkData;
    public float[] length;
    public GameObject blankSection;
    float z = 10;
    int rand = -1;
    int lastrand = -1;

    //megadja hogy mennyire elõre generáljon
    public int renderdistance = 30;

    public void Start()
    {
        
        //hosszúság bekérése
        //length = new float[levelChunkData.Length];
        //for (int i = 0; i < levelChunkData.Length; i++)
        //{
        //    length[i] = levelChunkData[i].transform.localScale.z;
        //}

        // elsõ blokkok generálása
        Instantiate(blankSection, new Vector3(0, -0.1f, 5), Quaternion.identity);

        while (z < renderdistance)
        {
            int id = NotSame();
            Debug.Log("pregenerate");
            Instantiate(levelChunkData[id], new Vector3(0, -0.1f, z + length[id]/2), Quaternion.identity);
            z += length[id];
        }
    }

    public void Update()
    {
        // endless generálás
        if (TeamManager.instance.penguins[0].gameObject.transform.position.z > z - renderdistance)
        {
            int id = NotSame();
            Debug.Log("generate");
            Instantiate(levelChunkData[id], new Vector3(0, -0.1f, z + length[id] / 2), Quaternion.identity);
            z += length[id];
        }



    }

    //különbözõ generálásának a biztosítása
    public int NotSame() {

        while (lastrand == rand)
        {
        rand = UnityEngine.Random.Range(0, levelChunkData.Length);
        }
        lastrand = rand;
        
        return rand;
    }
}
