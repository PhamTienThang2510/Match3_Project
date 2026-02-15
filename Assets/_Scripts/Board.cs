using UnityEngine;

public class Board : MonoBehaviour
{

    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] Dots;
    private TileBackground[,] gems;
    private GameObject[,] AllDotsInTheMatch;


    private void Start()
    {
        gems = new TileBackground[width, height];
        AllDotsInTheMatch = new GameObject[width, height];
        SetUpBoard();
        SetUpCamera();
    }

    private void SetUpBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 PosTemp = new Vector2(x, y);
                GameObject TileBackground = Instantiate(tilePrefab, PosTemp, Quaternion.identity, transform) as GameObject;
                TileBackground.transform.parent = this.transform;
                TileBackground.name = $"({x}, {y})";
                int randomDot = Random.Range(0, Dots.Length);
                GameObject dot = Instantiate(Dots[randomDot], PosTemp, Quaternion.identity, transform) as GameObject;
                dot.transform.parent = this.transform;
                dot.name = $"({x}, {y})";
                AllDotsInTheMatch[x, y] = dot;
            }
        }
    }

    private void SetUpCamera()
    {
        Camera.main.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10);
    }
}
