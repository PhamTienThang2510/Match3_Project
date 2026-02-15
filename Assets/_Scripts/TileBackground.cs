using UnityEngine;

public class TileBackground : MonoBehaviour
{

    [SerializeField] private GameObject[] Dots;
    private void Start()
    {
        Initialized();
    }
    private void Initialized()
    {
        int randomDot = Random.Range(0, Dots.Length);
        GameObject dot = Instantiate(Dots[randomDot], transform.position, Quaternion.identity, transform) as GameObject;
        dot.transform.parent = this.transform;
        dot.name = this.gameObject.name;
    }
}
