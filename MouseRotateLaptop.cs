using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MouseRotateLaptop : MonoBehaviour
{
    public float rotationAmount = 30f; // Maximum rotation angle
    public float rotationTime = 0.5f;  // Time taken to rotate
    public float floatStrength = 1f; // The height of the float effect
    public float floatSpeed = 0.5f; // Speed of the float effect
    public GameObject laptopChild;
    public bool debugSimulateTouchDevice = false; // Debug flag to simulate touch device in editor
    private Tween rotationTween;
    private Quaternion previousRotation;
    private Vector3 basePosition;
    public LaptopAnimations laptopAnimations;
    public MobileVersion mobileVersion;
    public const float targetWidth = 1300f; // Target width threshold in pixels
    private bool isTouchDevice = false;
    private const int minScreenWidth = 1024; // Minimum screen width to enable mouse rotation
    private const float minXRotation = -5f; // Minimum X rotation limit
    public bool resetChildRotation = true;

    private void Start()
    {
        basePosition = transform.position; // Store the current position of the object
        previousRotation = transform.rotation;

        // More specific touch detection
        #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            isTouchDevice = true;
        #else
            isTouchDevice = debugSimulateTouchDevice;
        #endif

        // Laptop responsiveness (moving further from right of vierport on small screens)
        // CheckViewportWidth();
        // StartCoroutine(MonitorViewportSize());
    }

    private void Update()
    {
        // Only run mouse rotation if it's NOT a touch device AND screen width is above minimum
        if (!isTouchDevice && Screen.width > minScreenWidth)
        {
            // Get mouse position as a value between -1 and 1
            float mouseXNormalized = (Input.mousePosition.x / Screen.width) * 2 - 1;
            float mouseYNormalized = (Input.mousePosition.y / Screen.height) * 2 - 1;

            // Calculate rotation angles based on mouse position
            float targetRotationX = Mathf.Clamp(mouseYNormalized * rotationAmount * -1, minXRotation, rotationAmount);
            float targetRotationY = Mathf.Clamp(mouseXNormalized * rotationAmount * -1, -rotationAmount, rotationAmount);

            Quaternion targetRotation = Quaternion.Euler(targetRotationX, targetRotationY, 0);

            // Only create or update the tween if there is a significant change
            if (rotationTween == null || targetRotation != previousRotation)
            {
                // If a tween already exists, kill it
                rotationTween?.Kill();

                // Create a new tween
                rotationTween = transform.DORotateQuaternion(targetRotation, rotationTime)
                    .SetEase(Ease.OutQuad);

                // Update previous rotation
                previousRotation = targetRotation;
            }
        }

        // Ensure laptopChild's x and z rotations are 0
        if (resetChildRotation && !mobileVersion.screenIsMobile())
        {
            SmoothRotateLaptopChild();
        }

        FloatEffect();
    }

    void SmoothRotateLaptopChild()
    {
        if (laptopChild != null && !isTouchDevice && Screen.width > minScreenWidth)  // Only run on desktop with sufficient screen width
        {
            Quaternion childRotation = laptopChild.transform.localRotation;

            // Smoothly correct laptopChild's rotation if its x or z rotation is not zero
            if (childRotation.eulerAngles.x != 0 || childRotation.eulerAngles.z != 0)
            {
                Quaternion targetRotation = Quaternion.Euler(0, childRotation.eulerAngles.y, 0);

                // Adjust rotationAmount to control the speed of rotation correction
                float rotationSpeed = 2f; // Adjust this value to control speed (lower value for slower animation)
                laptopChild.transform.localRotation = Quaternion.RotateTowards(childRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    void FloatEffect()
    {
        float newZ = Mathf.Cos(Time.time * floatSpeed) * floatStrength; // Depth motion
        transform.position = new Vector3(transform.position.x, transform.position.y, basePosition.z + newZ);
    }

    private void CheckViewportWidth()
    {
        if (Screen.width < targetWidth)
        {
            // Adjust the X position if viewport is less than the target width
            Vector3 newPosition = basePosition;
            newPosition.x += 4f; // Move by 2 units on the X-axis
            transform.position = newPosition;
        }
        else
        {
            // Reset to initial position if viewport width is greater
            transform.position = basePosition;
        }
    }

    private IEnumerator MonitorViewportSize()
    {
        Vector2 lastScreenSize = new Vector2(Screen.width, Screen.height);

        while (true)
        {
            // Check if the screen size has changed
            if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height && !laptopAnimations.hasStarted)
            {
                lastScreenSize = new Vector2(Screen.width, Screen.height);
                CheckViewportWidth();
            }

            yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds
        }
    }
}
