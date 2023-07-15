using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UICheck : MonoBehaviour
{
    // Start is called before the first frame update
    public Image UICheckPanel;
    public Image UIHeadImage0;
    public Image UIHeadImage1;
    public Image UIHeadImage2;
    public Image UIHeadImage3;
    bool hasSet;
    public Vector3 Scale=new Vector3(1.2f,1.2f,1.2f);
    void Start()
    {

    }

    

    // Update is called once per frame
    void Update()
    {
        if (!hasSet)
        {
            int num = GameStart.Instance.GetCurPlayer();
            Debug.Log(num);
            switch (num)
            {
                case 0:
                    UIHeadImage0.transform.DOScale(Scale, 0.5f);
                    
                    break;
                case 1:
                    UIHeadImage0.transform.DOScale(1, 0.5f);
                    UIHeadImage0.transform.GetChild(0).gameObject.SetActive(true);
                    UIHeadImage1.transform.DOScale(Scale, 0.5f);
                    break;
                case 2:
                    UIHeadImage1.transform.DOScale(1, 0.5f);
                    UIHeadImage1.transform.GetChild(0).gameObject.SetActive(true);
                    UIHeadImage2.transform.DOScale(Scale, 0.5f);
                    break;
                case 3:
                    UIHeadImage2.transform.DOScale(1, 0.5f);
                    UIHeadImage2.transform.GetChild(0).gameObject.SetActive(true);
                    UIHeadImage3.transform.DOScale(Scale, 0.5f);
                    break;
                case 4:
                    UIHeadImage3.transform.DOScale(1, 0.5f);
                    UIHeadImage3.transform.GetChild(0).gameObject.SetActive(true);
                    UICheckPanel.transform.DOScale(0, 0.3f);
                    
                    hasSet = true;
                    break;
                default:
                    Debug.Log("´íÎó");
                    break;
            }
        }
    }
}
