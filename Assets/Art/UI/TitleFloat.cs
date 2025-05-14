using UnityEngine;

public class TitleFloat : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float floatHeight = 6f;
    private Vector2 startPos;
    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector2(transform.position.x, newY);
    }
}
