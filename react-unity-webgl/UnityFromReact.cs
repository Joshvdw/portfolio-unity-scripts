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
        animScript.BackToHero();
        vidScript.ResetWorkProgressIndex();
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

    //  TABLET SIZE CHECK
    public void isTabletSize(string isTablet)
    {
        if (isTablet == "true")
        {
            animScript.ScaleLaptopToSmall();
        }
        else
        {
            animScript.ScaleLaptopToNormal();
        }
    }
    //  MOBILE SIZE CHECK
    public void isMobileSize(string isMobile)
    {
        if (isMobile == "true")
        {
            // animScript.MoveLaptopToMobilePosition();
        }
        else
        {
            // animScript.MoveLaptopToNormalPosition();
        }
    }
}
