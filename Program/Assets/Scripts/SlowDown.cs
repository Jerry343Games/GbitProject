using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDown : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Normal"))
        {
            other.transform.GetComponent<PlayerController>().speed = 0.4f * other.transform.GetComponent<PlayerController>().baseSpeed;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Normal"))
        {
            other.transform.GetComponent<PlayerController>().speed = other.transform.GetComponent<PlayerController>().baseSpeed;
        }
    }
}
