using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public GameObject[] levelChunkDatas;
    public float[] lenght;
    public GameObject Blanksection;
    float z = 10;
    int rand = -1;
    int lastrand = -1;

    //megadja hogy mennyire elõre generáljon
    public int renderdistance = 30;

    public void Start()
    {
        
        //hosszúság bekérése
        lenght = new float[levelChunkDatas.Length];
            for (int i = 0; i < levelChunkDatas.Length; i++)
            {
                lenght[i] = levelChunkDatas[i].transform.localScale.z;
            }

        // elsõ blokkok generálása
        Instantiate(Blanksection, new Vector3(0, -0.1f, 5), Quaternion.identity);

        while (z < renderdistance)
        {
            int id = NotSame();
            Debug.Log("pregenerate");
            Instantiate(levelChunkDatas[id], new Vector3(0, -0.1f, z + lenght[id]/2), Quaternion.identity);
            z += lenght[id];
        }
    }

    public void Update()
    {
        // endless generálás
        if (TeamManager.instance.penguins[0].gameObject.transform.position.z > z - renderdistance)
        {
            int id = NotSame();
            Debug.Log("generate");
            Instantiate(levelChunkDatas[id], new Vector3(0, -0.1f, z + lenght[id] / 2), Quaternion.identity);
            z += lenght[id];
        }



    }

    //különbözõ generálásának a biztosítása
    public int NotSame() {

        while (lastrand == rand)
        {
        rand = UnityEngine.Random.Range(0, levelChunkDatas.Length);
        }
        lastrand = rand;
        
        return rand;
    }
}
