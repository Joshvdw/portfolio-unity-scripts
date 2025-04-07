using UnityEngine;
using DG.Tweening;

public class MobileVersion : MonoBehaviour
{
    private Gyroscope gyro;
    private bool gyroEnabled = false;
    private Tween rotationTween;
    private Quaternion previousRotation;
    public float rotationTime = 0.5f; // Match the mouse rotation time
    private bool hasUserInteracted = false;
    private bool isTouchDevice = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Check if device supports touch input
        isTouchDevice = Input.touchSupported;
        
        bool isMobile = screenIsMobile();
        
        if (isMobile && isTouchDevice) // Only enable gyro if mobile AND touch device
        {
            // On iOS, we'll wait for user interaction before enabling gyro
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // We'll enable gyro on first touch
                return;
            }
            
            EnableGyro();
            previousRotation = transform.rotation;
        }
    }

    void OnEnable()
    {
        // Subscribe to touch events
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
        }
    }

    void OnDisable()
    {
        // Unsubscribe from touch events
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only run gyro rotation if it IS a touch device
        if (!isTouchDevice)
        {
            return;
        }

        // Check for first touch on iOS
        if (Application.platform == RuntimePlatform.IPhonePlayer && !hasUserInteracted && Input.touchCount > 0)
        {
            hasUserInteracted = true;
            EnableGyro();
            previousRotation = transform.rotation;
        }

        if (gyroEnabled)
        {
            try
            {
                // Get gyroscope rotation
                Quaternion gyroRotation = gyro.attitude;
                
                // Convert gyroscope rotation to screen space
                Quaternion screenRotation = new Quaternion(gyroRotation.x, gyroRotation.y, -gyroRotation.z, -gyroRotation.w);
                
                // Convert to Euler angles for easier manipulation
                Vector3 eulerAngles = screenRotation.eulerAngles;
                
                // Normalize angles to -180 to 180 range
                eulerAngles.x = NormalizeAngle(eulerAngles.x);
                eulerAngles.y = NormalizeAngle(eulerAngles.y);
                
                // Apply very subtle scaling (0.1 = 10% of original movement)
                eulerAngles.x *= -0.8f; // Double vertical movement
                eulerAngles.y *= 0.8f;  // Double horizontal movement
                
                // Strictly limit rotation range to ±10 degrees
                eulerAngles.x = Mathf.Clamp(eulerAngles.x, -16f, 10f); // More allowance for downward rotation
                eulerAngles.y = Mathf.Clamp(eulerAngles.y, -10f, 10f);
                eulerAngles.z = 0f; // Prevent any z-axis rotation
                
                // Create target rotation relative to initial position
                Quaternion targetRotation = Quaternion.identity * Quaternion.Euler(eulerAngles);

                // Only update if the change is significant but not too large
                float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
                if (rotationTween == null || (angleDifference > 0.05f && angleDifference < 15f))
                {
                    rotationTween?.Kill();

                    // Slower rotation for smoother movement
                    rotationTween = transform.DORotateQuaternion(targetRotation, rotationTime * 1.5f)
                        .SetEase(Ease.OutCubic);

                    previousRotation = targetRotation;
                }
            }
            catch (System.Exception e)
            {
                gyroEnabled = false;
            }
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private void EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            try
            {
                gyro = Input.gyro;
                gyro.enabled = true;
                gyroEnabled = true;
            }
            catch (System.Exception e)
            {
                gyroEnabled = false;
            }
        }
    }

    public bool screenIsMobile()
    {
        bool isMobile = Screen.width < 1024;
        return isMobile;
    }
}
