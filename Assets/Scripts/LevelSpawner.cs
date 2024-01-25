using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public GameObject Section;
    public void Start()
    {
        Instantiate(Section, new Vector3(0, -0.1f, 5), Quaternion.identity);
        Instantiate(Section, new Vector3(0, -0.1f, 15), Quaternion.identity);
    }
}
