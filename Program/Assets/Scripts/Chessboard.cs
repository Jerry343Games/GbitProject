using UnityEngine;

public class Chessboard : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject highlightedTile;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HighlightGrid();
    }

    /// <summary>
    /// 实时检测鼠标位置并高亮对应网格，需要在Update中执行
    /// </summary>
    private void HighlightGrid()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject hitObject = hitInfo.collider.gameObject;

            if (hitObject.CompareTag("Tile"))
            {
                if (highlightedTile != null && highlightedTile != hitObject)
                {
                    highlightedTile.GetComponent<Renderer>().material.color = Color.white;
                }

                highlightedTile = hitObject;
                highlightedTile.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
        else
        {
            if (highlightedTile != null)
            {
                highlightedTile.GetComponent<Renderer>().material.color = Color.white;
                highlightedTile = null;
            }
        }
    }
}
