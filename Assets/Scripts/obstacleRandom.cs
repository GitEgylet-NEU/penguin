using NohaSoftware.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obstacleRandom : MonoBehaviour
{
    public Sprite[] obstacles;
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = obstacles.GetRandom();
    }
}
