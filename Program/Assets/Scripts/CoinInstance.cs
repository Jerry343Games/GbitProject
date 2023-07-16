using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinInstance : MonoBehaviour
{
    public Vector2 Size;
    float x;
    float y;
    Vector3 CoinPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetRange()
    {
        x = Random.Range(0, Size.x);
        y = Random.Range(0, Size.y);
        CoinPos = new Vector3();
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, new Vector3(x, 0, y));
    }
}
