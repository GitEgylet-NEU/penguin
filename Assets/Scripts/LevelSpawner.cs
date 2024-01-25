using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public GameObject[] levelChunkDatas;
    public float[] lenght;
    public GameObject Section;
    float z = 25;
    int rand = -1;
    int lastrand = -1;

    public void Start()
    {
        // elsõ blokkok generálása
        Instantiate(Section, new Vector3(0, -0.1f, 5), Quaternion.identity);
        Instantiate(Section, new Vector3(0, -0.1f, 15), Quaternion.identity);

        //hosszúság bekérése
        lenght = new float[levelChunkDatas.Length];
            for (int i = 0; i < levelChunkDatas.Length; i++)
            {
                lenght[i] = levelChunkDatas[i].transform.localScale.z;
            }
        
    }

    public void Update()
    {
        // endless generálás
        TeamManager tam = FindObjectOfType<TeamManager>();
        if (TeamManager.instance.penguins[0].gameObject.transform.position.z > z - 20)
        {
            int id = NotSame();
            Debug.Log("generate");
            Instantiate(levelChunkDatas[id], new Vector3(0, -0.1f, z), Quaternion.identity);
            z += lenght[id];
        }



    }

    //különbözõ generálásának a biztosítása
    public int NotSame() {

        while (lastrand == rand)
        {
        rand = UnityEngine.Random.Range(0, 4);
        }
        lastrand = rand;
        
        return rand;
    }
}
