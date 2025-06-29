using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class VidPlayer : MonoBehaviour
{
    public LaptopAnimations animScript;
    public GradientAnimator gradientScript;
    public string[] projectColors = { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF" };

    public RawImage videoDisplay;          // Reference to RawImage UI element
    public Texture fallbackTexture;        // fallback static image
    public Texture[] fallbackTextures;     // fallback animation frames for stop-motion
    public float fallbackFrameInterval = 1f; // seconds per frame for stop-motion

    private Coroutine fallbackAnimationCoroutine = null;
    private bool usingFallbackAnimation = false;
    private bool isIOS = false;

    [SerializeField] private string heroVideoUrl;
    [SerializeField] private List<string> videoUrls = new List<string>();

    private VideoPlayer videoPlayer;
    private int currentIndex = 0;
    private bool isPaused = false;

    private float lastTime = 0f;
    private float stuckTime = 0f;

    [SerializeField] private RenderTexture newRenderTexture;

    private IEnumerator Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            PreserveCoroutines();
        #endif

        if (projectColors.Length > 0)
        {
            gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
        }

        videoPlayer = GetComponent<VideoPlayer>();


       // Wait a bit to ensure WebGL runtime is ready
        yield return new WaitForSeconds(0.5f);

        isIOS = Application.platform == RuntimePlatform.IPhonePlayer || IsIosWebGL();
        bool hasVisited = PlayerPrefs.GetInt("VisitedBefore", 0) == 1;

           if (isIOS && !hasVisited)
           {
//                Debug.Log("iOS first-time visitor — using fallback.");

               PlayerPrefs.SetInt("VisitedBefore", 1);

               if (videoDisplay != null)
               {
                   if (fallbackTextures != null && fallbackTextures.Length > 0)
                   {
                       // Start stop-motion animation
                       usingFallbackAnimation = true;
                       fallbackAnimationCoroutine = StartCoroutine(PlayFallbackAnimation());
                   }
                   else if (fallbackTexture != null)
                   {
                       // Static fallback image
                       videoDisplay.texture = fallbackTexture;
                   }
               }

               // Skip video init entirely for fallback on iOS first visit
               yield break;
           }

        if (videoPlayer)
        {
//             Debug.Log("[VidPlayer] Initialized VideoPlayer component.");
            videoPlayer.errorReceived += OnVideoError;
            // rws
           // videoPlayer.prepareCompleted += PlayVideoPostSetup; // est
            StartCoroutine(CheckVideoHealth());

            videoPlayer.SetDirectAudioMute(0, true);
//             videoPlayer.playOnAwake = false;

            SetupVideo(heroVideoUrl);
            videoPlayer.Pause(); // FOR ANDROID
        }
    }

    public void SetupVideo(string url)
    {
//         Debug.Log("[VidPlayer] Setting up video: " + url);

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        videoPlayer.prepareCompleted -= PlayVideoPostSetup;
        videoPlayer.prepareCompleted += PlayVideoPostSetup;
        videoPlayer.targetTexture = null;
        videoPlayer.clip = null; // optional but can help
        videoPlayer.url = "";    // force-clear previous UR

        // Reassign immediately
        if (videoPlayer.targetTexture == null)
        {
            videoPlayer.targetTexture = newRenderTexture;
        }

        videoPlayer.url = url;

//         float startTime = Time.realtimeSinceStartup;
        videoPlayer.Prepare();

//         StartCoroutine(WaitForPrepare(startTime));
    }
//
//     private IEnumerator WaitForPrepare(float startTime)
//     {
//         while (!videoPlayer.isPrepared)
//             yield return null;
//
//         float elapsed = Time.realtimeSinceStartup - startTime;
//         Debug.Log($"[VidPlayer] Video prepared in {elapsed:F3} seconds.");
//     }


    public void PlayVideoPostSetup(VideoPlayer source)
    {
//         Debug.Log("[VidPlayer] Video prepared. Attempting to play.");

        // Stop fallback animation if running
        if (usingFallbackAnimation)
        {
            usingFallbackAnimation = false;
            if (fallbackAnimationCoroutine != null)
            {
                StopCoroutine(fallbackAnimationCoroutine);
                fallbackAnimationCoroutine = null;
            }
        }

        if (videoDisplay != null)
        {
            videoDisplay.texture = videoPlayer.targetTexture;
        }

        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
//             Debug.Log("[VidPlayer] Video playback started.");

            if (isPaused)
            {
//                 Debug.Log("[VidPlayer] User had previously paused video. Pausing after play.");
                videoPlayer.Pause();
            }
        }
        else
        {
//             Debug.LogWarning("[VidPlayer] Tried to play video, but it's not prepared.");
        }

        videoPlayer.prepareCompleted -= PlayVideoPostSetup;
    }

    public void RestartVideo()
    {
//         Debug.Log("[VidPlayer] Restarting first video.");
        SetupVideo(videoUrls[0]);
    }

    public void RestartCurrentVideo()
    {
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Stop(); // optional, but ensures clean state
            videoPlayer.time = 0;
            videoPlayer.Play();
            isPaused = false;
//             Debug.Log("Restarted video at current index: " + currentIndex);
        }
        else
        {
//             Debug.LogWarning("Video not yet prepared; can't restart.");
        }
    }


    public void ResetWorkProgressIndex()
    {
//         Debug.Log("[VidPlayer] Resetting currentIndex to 0.");
        currentIndex = 0;
        isPaused = false;
    }

    public void VideoController(string func)
    {
//         Debug.Log("[VidPlayer] Received VideoController action: " + func);

        switch (func)
        {
            case "NextVideo":
                NextVideo();
                break;
            case "PreviousVideo":
                PreviousVideo();
                break;
            case "PauseVideo":
                PauseVideo();
                break;
            case "PlayVideo":
                PlayVideo();
                break;
            default:
                Debug.LogWarning("[VidPlayer] Unknown VideoController action: " + func);
                break;
        }
    }

// ****************** DELAYED VERSIONS *********************

    public void NextVideo()
    {
//         Debug.Log("[VidPlayer] Moving to next video.");
        if (isIOS)
        {
            currentIndex = (currentIndex + 1) % videoUrls.Count;
//             Debug.Log("[VidPlayer] Playing next video at index: " + currentIndex);
            SetupVideo(videoUrls[currentIndex]);
            gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
        }
        else
        {
        StartCoroutine(DelayedNextVideo());
        }
        animScript.FlipLaptopUp();
    }

    private IEnumerator DelayedNextVideo()
    {
        yield return new WaitForSeconds(0.3f);

        currentIndex = (currentIndex + 1) % videoUrls.Count;
//         Debug.Log("[VidPlayer] Playing next video at index: " + currentIndex);
        SetupVideo(videoUrls[currentIndex]);
        gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
    }

    public void PreviousVideo()
    {
        Debug.Log("[VidPlayer] Moving to previous video.");
        if (isIOS)
        {
            currentIndex = (currentIndex - 1 + videoUrls.Count) % videoUrls.Count;
//             Debug.Log("[VidPlayer] Playing previous video at index: " + currentIndex);
            SetupVideo(videoUrls[currentIndex]);
            gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
         }
        else
        {
        StartCoroutine(DelayedPreviousVideo());
        }
        animScript.FlipLaptopDown();
    }

    private IEnumerator DelayedPreviousVideo()
    {
        yield return new WaitForSeconds(0.3f);

        currentIndex = (currentIndex - 1 + videoUrls.Count) % videoUrls.Count;
//         Debug.Log("[VidPlayer] Playing previous video at index: " + currentIndex);
        SetupVideo(videoUrls[currentIndex]);
        gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
    }

// ****************** NON DELAYED VERSIONS *********************
//  public void NextVideo()
//     {
//         Debug.Log("[VidPlayer] Moving to next video.");
//         currentIndex = (currentIndex + 1) % videoUrls.Count;
//         Debug.Log("[VidPlayer] Playing next video at index: " + currentIndex);
//         SetupVideo(videoUrls[currentIndex]);
//         gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
//         animScript.FlipLaptopUp();
//     }
//
//     public void PreviousVideo()
//     {
//         Debug.Log("[VidPlayer] Moving to previous video.");
//         currentIndex = (currentIndex - 1 + videoUrls.Count) % videoUrls.Count;
//         Debug.Log("[VidPlayer] Playing previous video at index: " + currentIndex);
//         SetupVideo(videoUrls[currentIndex]);
//         gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
//         animScript.FlipLaptopDown();
//     }


    public void PlayVideo()
    {
//         Debug.Log("[VidPlayer] PlayVideo called. Is playing? " + videoPlayer.isPlaying);
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
//             Debug.Log("[VidPlayer] Video manually started.");
            isPaused = false;
        }
    }

    public void PauseVideo()
    {
//         Debug.Log("[VidPlayer] PauseVideo called. Is playing? " + videoPlayer.isPlaying);
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
//             Debug.Log("[VidPlayer] Video manually paused.");
            isPaused = true;
        }
    }

    private IEnumerator CheckVideoHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (!videoPlayer.isPrepared || string.IsNullOrEmpty(videoPlayer.url))
            {
//                 Debug.Log("[VidPlayer] Skipping health check — video not prepared or no URL.");
                continue;
            }

            float currentTime = (float)videoPlayer.time;

            if (Mathf.Approximately(currentTime, lastTime) && videoPlayer.isPlaying)
            {
                stuckTime += 1f;
//                 Debug.LogWarning($"[VidPlayer] Video possibly stuck — time: {currentTime}, stuck for: {stuckTime} sec.");
            }
            else
            {
                stuckTime = 0f;
            }

            lastTime = currentTime;

            if (stuckTime >= 3f)
            {
//                 Debug.LogError("[VidPlayer] Video appears stuck for 3+ seconds. Attempting recovery.");
                TryRecoverVideo();
                stuckTime = 0f;
            }
        }
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
//         Debug.LogError("[VidPlayer] VideoPlayer error: " + message);
        TryRecoverVideo();
    }

    private void TryRecoverVideo()
    {
        if (currentIndex < videoUrls.Count)
        {
//             Debug.Log("[VidPlayer] Attempting to recover current video at index " + currentIndex);
            SetupVideo(videoUrls[currentIndex]);
        }
        else
        {
//             Debug.LogWarning("[VidPlayer] Recovery failed — invalid currentIndex: " + currentIndex);
        }
    }

    private IEnumerator PlayFallbackAnimation()
    {
        int index = 0;
        while (usingFallbackAnimation)
        {
            if (fallbackTextures != null && fallbackTextures.Length > 0 && videoDisplay != null)
            {
                videoDisplay.texture = fallbackTextures[index];
                index = (index + 1) % fallbackTextures.Length;
            }
            yield return new WaitForSeconds(fallbackFrameInterval);
        }
    }

    // is IOS CHECKER
    [DllImport("__Internal")]
    private static extern int IsIosUserAgent();

    private bool IsIosWebGL()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        return IsIosUserAgent() == 1;
    #else
        return false;
    #endif
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
        // Prevent IL2CPP stripping of coroutines
        private void PreserveCoroutines()
        {
            IEnumerator a = CheckVideoHealth();
            IEnumerator b = DelayedNextVideo();
            IEnumerator c = DelayedPreviousVideo();
            IEnumerator d = PlayFallbackAnimation();

            // Prevent compiler warning
            if (a == null || b == null || c == null || d == null) Debug.Log("Preserved coroutines");
        }
    #endif

}
