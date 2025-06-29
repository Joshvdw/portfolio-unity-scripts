// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Video;
// using System;
// using System.Collections;
//
// public class VidPlayer : MonoBehaviour
// {
//     public LaptopAnimations animScript;
//     public GradientAnimator gradientScript;
//     public string[] projectColors = { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF" };
//     [SerializeField] private string heroVideoUrl; // Just the filename or relative path
//     [SerializeField] private List<string> videoUrls = new List<string>(); // Just filenames or relative paths
//
//     private VideoPlayer videoPlayer;
//     private int currentIndex = 0;
//     private bool isPaused = false;
//
//     void Awake()
//     {
//         videoPlayer = GetComponent<VideoPlayer>();
//
//         if (videoPlayer)
//         {
//             SetupVideo(GetFullVideoUrl(heroVideoUrl));
//         }
//
//         if (projectColors.Length > 0)
//         {
//             gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
//         }
//     }
//
//     /// <summary>
//     /// Builds a full, absolute video URL based on a relative filename or path.
//     /// </summary>
//     private string GetFullVideoUrl(string filename)
//     {
//         string baseUrl = Application.absoluteURL;
//
//         // Trim after /Build/ or any hash/query to get clean domain root
//         int idx = baseUrl.IndexOf("/Build/");
//         if (idx > 0) baseUrl = baseUrl.Substring(0, idx);
//
//         string fullUrl = baseUrl + "/Assets/Videos/" + filename;
//
//         Debug.Log("Resolved video URL: " + fullUrl);
//         return fullUrl;
//     }
//
//     // Setup and prepare video by URL
//     private void SetupVideo(string url)
//     {
//         if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
//         {
//             Debug.LogWarning("Attempted to setup invalid video URL: " + url);
//             return;
//         }
//
//         if (videoPlayer.isPlaying)
//         {
//             videoPlayer.Stop();
//         }
//
//         videoPlayer.url = url;
//         videoPlayer.Prepare();
//         videoPlayer.prepareCompleted += PlayVideoPostSetup;
//     }
//
//     private void PlayVideoPostSetup(VideoPlayer source)
//     {
//         videoPlayer.Play();
//
//         if (isPaused)
//         {
//             videoPlayer.Pause();
//         }
//
//         videoPlayer.prepareCompleted -= PlayVideoPostSetup;
//     }
//
//     public void RestartVideo()
//     {
//         SetupVideo(GetFullVideoUrl(videoUrls[0]));
//     }
//
//     public void ResetWorkProgressIndex()
//     {
//         currentIndex = 0;
//     }
//
//     public void VideoController(string func)
//     {
//         switch (func)
//         {
//             case "NextVideo":
//                 NextVideo();
//                 break;
//             case "PreviousVideo":
//                 PreviousVideo();
//                 break;
//             case "PauseVideo":
//                 PauseVideo();
//                 break;
//             case "PlayVideo":
//                 PlayVideo();
//                 break;
//             default:
//                 Debug.LogWarning("Unknown function: " + func);
//                 break;
//         }
//     }
//
//     public void NextVideo()
//     {
//         StartCoroutine(DelayedNextVideo());
//         animScript.FlipLaptopUp();
//     }
//
//     private IEnumerator DelayedNextVideo()
//     {
//         yield return new WaitForSeconds(0.3f);
//
//         currentIndex = (currentIndex + 1) % videoUrls.Count;
//         SetupVideo(GetFullVideoUrl(videoUrls[currentIndex]));
//         gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
//     }
//
//     public void PreviousVideo()
//     {
//         StartCoroutine(DelayedPreviousVideo());
//         animScript.FlipLaptopDown();
//     }
//
//     private IEnumerator DelayedPreviousVideo()
//     {
//         yield return new WaitForSeconds(0.3f);
//
//         currentIndex = (currentIndex - 1 + videoUrls.Count) % videoUrls.Count;
//         SetupVideo(GetFullVideoUrl(videoUrls[currentIndex]));
//         gradientScript.SetRadialGradientColor(projectColors[currentIndex]);
//     }
//
//     public void PlayVideo()
//     {
//         if (!videoPlayer.isPlaying)
//         {
//             videoPlayer.Play();
//             isPaused = false;
//         }
//     }
//
//     public void PauseVideo()
//     {
//         if (videoPlayer.isPlaying)
//         {
//             videoPlayer.Pause();
//             isPaused = true;
//         }
//     }
// }

