using UnityEngine;

public enum GemType
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple
}
public class GemBase : MonoBehaviour
{
    [SerializeField] protected GemType type;
    // expose logical position to Board
    public Vector2 LogicalPosition { get; private set; }

    // Initialize the instantiated gem: set its logical position and choose a random GemType.
    // Returns the same GameObject instance (does NOT Instantiate again).
    public GameObject Initalize(Vector2 Pos)
    {
        LogicalPosition = Pos;
        transform.position = Pos;

        // Optional: update name so it's easier to debug in the Hierarchy
        gameObject.name = $"Gem ({Pos.x}, {Pos.y}) - {type}";

        return gameObject;
    }
}