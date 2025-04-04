using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;
using System.Collections;

public class VidPlayer : MonoBehaviour
{
    public LaptopAnimations animScript;
    public GradientAnimator gradientScript;
    public string[] projectColors = { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF" };
    [SerializeField] private string heroVideoUrl; // URL for the hero video
    [SerializeField] private List<string> videoUrls = new List<string>(); // List of regular video URLs

    private VideoPlayer videoPlayer;
    private int currentIndex = 0; // Index for regular videos

    private bool isPaused = false;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer)
        {
            SetupVideo(heroVideoUrl);
        }
        // Initialize the gradient color
        if (projectColors.Length > 0)
        {
            gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
        }
    }

    // Setup and prepare video by URL
    private void SetupVideo(string url)
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop(); // Stop current video if playing
        }

        videoPlayer.url = url;
        videoPlayer.Prepare();

        videoPlayer.prepareCompleted += PlayVideoPostSetup; // Play video when prepared
        
    }

    // Play video method callback
    private void PlayVideoPostSetup(VideoPlayer source)
    {        
        videoPlayer.Play();
        if (isPaused) 
        {
            videoPlayer.Pause(); // pause video if user paused
        }
        videoPlayer.prepareCompleted -= PlayVideoPostSetup; // Detach event after play to avoid multiple subscriptions
    }

    public void RestartVideo()
    {
        SetupVideo(videoUrls[0]);
    }

    public void ResetWorkProgressIndex()
    {
        currentIndex = 0;
    }

    public void VideoController(string func)
    {
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
                Debug.LogWarning("Unknown function: " + func);
                break;
        }
    }

    // Play next video
    public void NextVideo()
    {
        StartCoroutine(DelayedNextVideo());
        animScript.FlipLaptopUp();
    }
    private IEnumerator DelayedNextVideo()
    {
        yield return new WaitForSeconds(0.3f);

        currentIndex = (currentIndex + 1) % videoUrls.Count;
        SetupVideo(videoUrls[currentIndex]);
        gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
    }

    // Play previous video
    public void PreviousVideo()
    {
        StartCoroutine(DelayedPreviousVideo());
        animScript.FlipLaptopDown();
    }
        private IEnumerator DelayedPreviousVideo()
    {
        yield return new WaitForSeconds(0.3f);

        currentIndex = (currentIndex - 1 + videoUrls.Count) % videoUrls.Count;
        SetupVideo(videoUrls[currentIndex]);
        gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
    }

    // Parameterless public play video method
    public void PlayVideo()
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            isPaused = false;
        }
    }

    // Pause video
    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            isPaused = true;
        }
    }
}
