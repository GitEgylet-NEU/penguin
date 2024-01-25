using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class asd : MonoBehaviour
{
    public void Start()
    {
        if (this.gameObject.name != "Penguin 0")
        {
            GetComponent<asd>().enabled = false;
        }
    }
    public GameObject Section;
    int z = 25;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("trigger") && this.gameObject.name == "Penguin 0")
        {
            Debug.Log("generate");
                   Instantiate(Section, new Vector3(0, -0.1f, z), Quaternion.identity);
                  z += 10;
        }
    }
}
