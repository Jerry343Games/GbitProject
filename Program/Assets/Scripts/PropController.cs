using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PropController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isWeapon = false;
    public float duration = 3f;

    //音效
    public AudioSource audioSource1;

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
                //音效
                string prefabPath = "Assets/Sound/Role/Human/Hit.mp3";
                AudioClip mp3Audio = AssetDatabase.LoadAssetAtPath<AudioClip>(prefabPath);
                audioSource1.clip = mp3Audio;
                audioSource1.Play();

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
