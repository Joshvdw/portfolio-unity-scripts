using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;
using System.Collections;

public class LaptopAnimations : MonoBehaviour
{
    public float duration = 0.5f; // Duration of the animation
    public float xOffset = 7.5f;  // Public property to adjust the X movement offset
    public float scaleMultiplier = 2.0f; // Public property to adjust the scale multiplier
    public VideoPlayer videoPlayer; // Reference to the VideoPlayer component
    public string newVideoUrl = "http://example.com/newvideo.mp4"; // New video URL to set during animation
    public string heroVideoUrl = "http://example.com/newvideo.mp4";    
    public Transform transformParent;
    private Sequence mySequence;
    private Sequence resetSequence;
    private bool isOffScreen = false;
    public bool hasStarted = false; 
    public MobileVersion mobileVersion;

    private void Start()
    {
        InitializeSequence();
        // Initialize the reset sequence once
        resetSequence = DOTween.Sequence().Pause().SetAutoKill(false);
        // start laptop off-screen if not mobile
        if (!mobileVersion.screenIsMobile()) {
            Vector3 currentPosition = transform.position;
            currentPosition.x -= xOffset * 2;
            transform.position = currentPosition;
        } else {
            // Position laptop above viewport for mobile
            if (transformParent != null) {
                Vector3 screenPosition = new Vector3(Screen.width / 2f, 0f, 0f);
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, transformParent.position.z));
                worldPosition.y = 14f; // Position 10 units above final position
                transformParent.position = worldPosition;
            }
            // Reset local position of the laptop relative to its parent
            transform.localPosition = Vector3.zero;
            // Reset Y rotation to 0 for mobile
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(10f, 0f, currentRotation.z);
            // Set scale to 1.0 for mobile
            transform.localScale = Vector3.one * 1.0f;
        }
    }

    private void InitializeSequence()
    {
        // Kill the existing sequence if it exists
        if (mySequence != null && mySequence.IsActive())
        {
            mySequence.Kill();
        }
        
        // Kill any existing reset sequence
        if (resetSequence != null && resetSequence.IsActive())
        {
            resetSequence.Kill();
        }

        // Create a new sequence and set to pause and not auto-kill
        mySequence = DOTween.Sequence().Pause().SetAutoKill(false);
    }

    private void OnDisable()
    {
        // Clean up sequences when object is disabled
        if (mySequence != null)
        {
            mySequence.Kill();
        }
        if (resetSequence != null)
        {
            resetSequence.Kill();
        }
    }

    public void RepositionLaptopOnMobile()
    {
        // Reset scale
        transform.localScale = Vector3.one * 1.0f;
        
        // Center the laptop on screen
        if (transformParent != null) {
            // Calculate position: centered on X, 3 units from top on Y
            Vector3 screenPosition = new Vector3(Screen.width / 2f, 0f, 0f);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, transformParent.position.z));
            worldPosition.y = 3.5f;
            transformParent.position = worldPosition;
            // Reset Y rotation
            transform.rotation = Quaternion.Euler(10f, 0f, transform.rotation.eulerAngles.z);
            
            // Reset local position of the laptop relative to its parent
            transform.localPosition = Vector3.zero;
        }
    }

    private void SmoothResetOrientation()
    {
        // Kill any active reset sequence
        if (resetSequence.IsActive())
        {
            resetSequence.Kill();
        }
        
        // Clear and reuse the sequence
        resetSequence.SetAutoKill(false);
        resetSequence.Complete(); // Ensure any previous animation is finished
        resetSequence.Kill(true); // Kill and reset the sequence
        
        // Get current rotation and create target rotation
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(10f, 0f, currentRotation.z);
        
        // Match the mouse movement's floaty feel but twice as slow
        resetSequence.Append(transform.DORotateQuaternion(targetRotation, 1f) // Twice the original duration
            .SetEase(Ease.OutQuad)); // Keep same easing for smooth feel
            
        resetSequence.Play();
    }

    // LOAD IN ANIMATION
    public void LoadInAnimation()
    {
        InitializeSequence();
        
        if (!mobileVersion.screenIsMobile()) {
            float longerDuration = duration * 2;
            mySequence.Append(transform.DORotate(new Vector3(0, -360, 0), longerDuration / 2, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
            mySequence.Join(transform.DOMoveX(transform.position.x + xOffset * 2, longerDuration / 2).SetEase(Ease.OutExpo));
        } else {
            Vector3 startPosition = transformParent.position;
            startPosition.y += 10f;
            transformParent.position = startPosition;
            
            mySequence.Append(transformParent.DOMoveY(3.5f, duration * 2).SetEase(Ease.OutExpo));
            
            // Set initial X rotation
            transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            // Only rotate on Y axis
            mySequence.Join(transform.DORotate(new Vector3(0, -360, 0), duration * 2, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
        }

        mySequence.Restart();
    }

    public void StartExperience()
    {
        StartCoroutine(DelayVideoStart());
        InitializeSequence();

        if (mobileVersion.screenIsMobile()) {
            // Set initial X rotation
            transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            // Only rotate on Y axis
            mySequence.Append(transform.DORotate(new Vector3(0, -360, 0), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutQuad));
        } else {
            mySequence.Append(transform.DORotate(new Vector3(0, -360, 0), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
        }
        
        if (!mobileVersion.screenIsMobile()) {
            mySequence.Join(transformParent.DOMoveX(transform.position.x + xOffset, duration).SetEase(Ease.OutExpo));
            Vector3 currentRotation = transform.rotation.eulerAngles;
            currentRotation.y -= 24f;
            transform.rotation = Quaternion.Euler(currentRotation);
        }

        hasStarted = true;
        mySequence.Restart();
    }

    private IEnumerator DelayVideoStart()
    {
        yield return new WaitForSeconds(0.1f);
        // Change the video URL at the desired point in the animation
        ChangeVideoUrl();
    }

    public void FlipLaptopUp()
    {
        InitializeSequence();

        if (mobileVersion.screenIsMobile()) {
            // Set initial X rotation
            transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            // Only rotate on Y axis
            mySequence.Append(transform.DORotate(new Vector3(0, -360, 0), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutQuad));
        } else {
            mySequence.Append(transform.DORotate(new Vector3(0, -360, 0), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutQuad));
        }

        mySequence.Restart();
    }

    public void FlipLaptopDown()
    {
        InitializeSequence();

        if (mobileVersion.screenIsMobile()) {
            // Set initial X rotation
            transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            // Only rotate on Y axis
            mySequence.Append(transform.DORotate(new Vector3(0, 360, 0), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutQuad));
        } else {
            mySequence.Append(transform.DORotate(new Vector3(0, 360, 0), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutQuad));
        }

        mySequence.Restart();
    }

    public void FlipLaptopOut()
    {
        if (!isOffScreen)
        {
            InitializeSequence();
            float xOffsetNavChange = xOffset * 3;

            if (mobileVersion.screenIsMobile()) {
                // Set initial X rotation
                transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                // Only rotate on Y axis
                mySequence.Append(transform.DORotate(new Vector3(0, -360, 0), duration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.InOutQuad));
            } else {
                mySequence.Append(transform.DORotate(new Vector3(0, -360, 0), duration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.InOutQuad));
            }
            
            mySequence.Join(transform.DOMoveX(transform.position.x + xOffsetNavChange, duration * 4).SetEase(Ease.OutExpo));

            mySequence.Restart();
            isOffScreen = true;
        }
    }

    public void FlipLaptopIn()
    {
        if (isOffScreen)
        {
            InitializeSequence();
            float xOffsetNavChange = xOffset * 3;

            if (mobileVersion.screenIsMobile()) {
                // Set initial X rotation
                transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                // Only rotate on Y axis
                mySequence.Append(transform.DORotate(new Vector3(0, 360, 0), duration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.InOutQuad));
            } else {
                mySequence.Append(transform.DORotate(new Vector3(0, 360, 0), duration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.InOutQuad));
            }

            mySequence.Join(transform.DOMoveX(transform.position.x - xOffsetNavChange, duration).SetEase(Ease.OutExpo));

            mySequence.Restart();
            isOffScreen = false;
        }
    }

    public void BackToHero()
    {
        videoPlayer.url = heroVideoUrl;
        videoPlayer.Prepare();

        InitializeSequence();

        if (mobileVersion.screenIsMobile()) {
            // Set initial X rotation
            transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            // Only rotate on Y axis
            mySequence.Append(transform.DORotate(new Vector3(0, 360, 0), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutQuad));
        } else {
            mySequence.Append(transform.DORotate(new Vector3(0, 360, 0), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutQuad));
        }

        if (!mobileVersion.screenIsMobile()) 
        {
            mySequence.Join(transformParent.DOMoveX(transform.position.x - xOffset, duration).SetEase(Ease.OutExpo));
            Vector3 currentRotation = transform.rotation.eulerAngles;
            currentRotation.y += 24f;
            transform.rotation = Quaternion.Euler(currentRotation);
        }
        
        mySequence.Restart();
    }

    private void ChangeVideoUrl()
    {
        videoPlayer.url = newVideoUrl;
        videoPlayer.Prepare(); // Prepare the video to play
    }

    public void ScaleLaptopToSmall()
    {
        if (transform != null)
        {
            transform.localScale = Vector3.one * 0.6f;
        }
    }

    public void ScaleLaptopToNormal()
    {
        if (transform != null)
        {
            transform.localScale = Vector3.one * 1.0f;
        }
    }

    public void MoveLaptopToMobilePosition()
    {
        if (transformParent != null)
        {
            Vector3 screenPosition = new Vector3(Screen.width / 2f, 0f, 0f);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, transformParent.position.z));
            worldPosition.y = 14f; // Position higher up for mobile
            transformParent.position = worldPosition;
            // Reset local position of the laptop relative to its parent
            transform.localPosition = Vector3.zero;
            // Reset Y rotation to 0 for mobile
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(10f, 0f, currentRotation.z);
        }
    }

    public void MoveLaptopToNormalPosition()
    {
        if (transformParent != null)
        {
            Vector3 screenPosition = new Vector3(Screen.width / 2f, 0f, 0f);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, transformParent.position.z));
            worldPosition.y = 3.5f; // Normal position
            transformParent.position = worldPosition;
            // Reset local position of the laptop relative to its parent
            transform.localPosition = Vector3.zero;
        }
    }
}
