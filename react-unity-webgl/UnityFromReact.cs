using UnityEngine;

public class UnityFromReact : MonoBehaviour
{
    public LaptopAnimations animScript;
    public GradientAnimator gradientScript;
    public VidPlayer vidScript; 
    public GlobeWireframe globeScript; 
    public MouseRotateLaptop laptopRotateScript;

    // LOAD IN ANIMATION
    public void HasLoaded()
    {
        animScript.LoadInAnimation();
    }

    // START EXPERIENCE (HERO START BTN PRESS)
    public void StartExperience()
    {
        animScript.StartExperience();
        gradientScript.FadeOutRedAndFadeInRadialGradient();
    }

    // NAV HERO SCENE
    public void SetHeroScene()
    {
        vidScript.ResetWorkProgressIndex();
        animScript.BackToHero();
        gradientScript.ReverseFadeOutRedAndFadeInRadialGradient();
    }

    // NAV WORK SCENE
    public void SetWorkScene()
    {
        animScript.FlipLaptopIn();
        gradientScript.FadeInRadialGradient();
        vidScript.RestartVideo();
        IsLaptopInView(true);
        globeScript.SlideGlobeOut();
    }

    // NAV ABOUT SCENE
     public void SetAboutScene()
    {
       IsLaptopInView(false);
    }

    // NAV SERVICES SCENE
    public void SetServicesScene()
    {
        animScript.FlipLaptopOut();
        gradientScript.FadeInRadialGradient();
        IsLaptopInView(false);
        globeScript.SlideGlobeOut();
    }

    // NAV MUSIC SCENE
     public void SetMusicScene()
    {
       gradientScript.FadeOutRadialGradient(); // might not be needed
       IsLaptopInView(false);
       globeScript.SlideGlobeOut();
    }

    // NAV CONTACT SCENE
    public void SetContactScene()
    {
        animScript.FlipLaptopOut();
        globeScript.SlideGlobeIn();
        IsLaptopInView(false);
    }

    //  PROJECT VIDEO CONTROLLER
    public void VideoController(string func)
    {
        vidScript.VideoController(func);
    }

    // LOGIC FOR WHEN LAPTOP ISN'T IN VIEW
    private void IsLaptopInView(bool state)
    {
        laptopRotateScript.enabled = state;
        if (!state)
        {
            VideoController("PauseVideo");
        }
    }

    //  MOBILE SIZE SWITCHER
    public void isMobileSize(string arg)
    {
        if (arg == "true")
        {
            animScript.MoveLaptopToMobilePosition();
        }
        else
        {
            animScript.MoveLaptopToDesktopPosition();
        }
    }
}
