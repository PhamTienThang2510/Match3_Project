using UnityEngine;

public class Dot : MonoBehaviour
{
    private Vector2 FirstTouch;
    private Vector2 LastTouch;
    public float SwipeAngle = 0;
    private void OnMouseDown()
    {
        FirstTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log("First Touch: " + FirstTouch);
    }
    private void OnMouseUp()
    {
        LastTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngles();
    }
    void CalculateAngles()
    {
        SwipeAngle = Mathf.Atan2(LastTouch.y - FirstTouch.y, LastTouch.x - FirstTouch.x) * 180 / Mathf.PI;
        Debug.Log("Swipe Angle: " + SwipeAngle);
    }
}
