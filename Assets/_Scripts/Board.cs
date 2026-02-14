using UnityEngine;

public class Board : MonoBehaviour
{

    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject[] ListGemPrefabs;
    private GemBase[,] gems;


    private void Start()
    {
        gems = new GemBase[width, height];
        SetUpBoard();
        SetUpCamera();
    }

    private void SetUpBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create an empty cell GameObject named like "x,y" and place it on the board
                GameObject cell = new GameObject($"{x},{y}");
                cell.transform.SetParent(transform);
                cell.transform.position = new Vector3(x, y, 0f);

                // Instantiate chosen gem as a child of the cell, positioned at the cell's location
                int randomIndex = Random.Range(0, ListGemPrefabs.Length);
                GameObject gemObj = Instantiate(ListGemPrefabs[randomIndex], cell.transform.position, Quaternion.identity, cell.transform);

                // Ensure gem local position is exactly at the cell origin
                gemObj.transform.localPosition = Vector3.zero;

                // Initialize GemBase (sets type/position) and store in the grid
                GemBase gem = gemObj.GetComponent<GemBase>();
                if (gem != null)
                {
                    gem.Initalize(new Vector2(x, y));
                    gems[x, y] = gem;
                }
            }
        }
    }

    private void SetUpCamera()
    {
        Camera.main.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10);
    }
}
