using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCoin : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject != null)
        {
            Destroy(gameObject, 10);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate( Vector3.up*speed,Space.Self);
    }
}
