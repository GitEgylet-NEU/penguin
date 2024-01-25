using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public GameObject Section;
    int z = 25;

    public void Start()
    {
        // elsõ blokkok generálása
        Instantiate(Section, new Vector3(0, -0.1f, 5), Quaternion.identity);
        Instantiate(Section, new Vector3(0, -0.1f, 15), Quaternion.identity);
    }

   
    public void Update()
    {
        // generálás
        TeamManager tam = FindObjectOfType<TeamManager>();
        if (TeamManager.instance.penguins[0].gameObject.transform.position.z > z - 20)
        {
            Debug.Log("generate");
            Instantiate(Section, new Vector3(0, -0.1f, z), Quaternion.identity);
            z += 10;
        }
    }
}
