using NUnit.Framework;
using UnityEngine;

public class InitBoard : MonoBehaviour
{
    public void SetUpBoard(Board board)
    {
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Vector2 pos = new Vector2(x, y);

                // Avoid initial matches by re-rolling until safe
                int randomDot = 0; // Initialize to a valid value
                int attempts = 0;
                //do
                //{
                //    randomDot = Random.Range(0, board.dots.Length);
                //    attempts++;
                //    // safety to avoid infinite loop if dots.Length < required variety
                //    if (attempts > 50) break;
                //} while (WouldCreateInitialMatch(x, y, randomDot));

                GameObject dot = Instantiate(board.dots[randomDot], pos, Quaternion.identity, transform);
                Dot dotScript = dot.GetComponent<Dot>();
                if (dotScript != null)
                {
                    dotScript.Init(x, y, board, randomDot);
                }

                // ensure consistent base scale
                dot.transform.localScale = CONSTANT.baseScale;

                board.AllDotsInTheBoard[x, y] = dot;
            }
        }
    }
}
