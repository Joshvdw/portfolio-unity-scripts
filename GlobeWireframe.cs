using UnityEngine;

public class GlobeWireframe : MonoBehaviour
{
    public float xOffset = 10f;
    public float slideSpeed = 5f;
    public float spinSpeedX = 50f;
    public float spinSpeedY = 30f;
    public float spinSpeedZ = 20f;
    private bool globeIsOffScreen = true;
    private Vector3 offScreenPosition;
    private Vector3 onScreenPosition;

    void Start()
    {
        onScreenPosition = transform.position;
        offScreenPosition = onScreenPosition + new Vector3(xOffset, 0f, 0f);
        transform.position = offScreenPosition;
    }

    void Update()
    {
        if (!globeIsOffScreen)
        {
            transform.Rotate(Vector3.right, spinSpeedX * Time.deltaTime);
            transform.Rotate(Vector3.up, spinSpeedY * Time.deltaTime);
            transform.Rotate(Vector3.forward, spinSpeedZ * Time.deltaTime);

            float step = slideSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, onScreenPosition, step);
            if (Vector3.Distance(transform.position, onScreenPosition) < 0.01f)
            {
                transform.position = onScreenPosition;
                globeIsOffScreen = false;
            }
        }
    }

    public void SlideGlobeOut()
    {
        if (!globeIsOffScreen)
        {
            globeIsOffScreen = true;
            transform.position = offScreenPosition;
        }
    }

    public void SlideGlobeIn()
    {
        if (globeIsOffScreen)
        {
            gameObject.SetActive(true);
            globeIsOffScreen = false;
        }
    }
}
