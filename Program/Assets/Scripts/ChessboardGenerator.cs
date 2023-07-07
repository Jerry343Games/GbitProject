using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ChessboardGenerator : MonoBehaviour
{
    /// <summary>
    /// 棋盘的网格数
    /// </summary>
    public Vector2Int gridSize;
    /// <summary>
    /// 正方形网格边长
    /// </summary>
    public float squareSize;
    /// <summary>
    /// 网格预制体
    /// </summary>
    public GameObject tilePrefab; 

    private void Start()
    {
        if (!Application.isPlaying)
        {
            GenerateChessboard();
        }
    }

    public void GenerateChessboard()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 position = transform.position + new Vector3(x * squareSize, 0f, y * squareSize);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.localScale = new Vector3(squareSize, 0.1f, squareSize);
                tile.tag = "Tile";
                tile.transform.parent = transform;
            }
        }
    }

    public void DeleteChessboard()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

[CustomEditor(typeof(ChessboardGenerator))]
public class ChessboardGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ChessboardGenerator generator = (ChessboardGenerator)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("生成棋盘"))
        {
            generator.GenerateChessboard();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("删除棋盘"))
        {
            generator.DeleteChessboard();
        }
    }
}
