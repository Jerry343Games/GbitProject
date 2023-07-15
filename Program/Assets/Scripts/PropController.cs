using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isWeapon = false;
    public float duration = 3f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isWeapon)
        {
            duration -= Time.deltaTime;
            if (duration <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isWeapon)
        {
            if (collision.gameObject.CompareTag("Normal") || collision.gameObject.CompareTag("Ghost"))
            {
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.IsOpposite();
                }

                // 道具砸中后销毁
                Destroy(gameObject);
            }
        }
    }
}
