using UnityEngine;
using System.Collections;
using UnityEngine.Scripting;
[Preserve]
public class GradientAnimator : MonoBehaviour
{
    public Material material;
    public Light targetLight; // Reference to the Light component you want to control
    public float fadeOutDuration = 1.0f; // Duration of the fade-out animation
    public float fadeInDuration = 2.0f; // Duration of the fade-in animation for the radial gradient

    // Radial gradient settings
    public Color radialGradientColor = Color.red; // Color of the radial gradient
    public float radialGradientRadius = 0.5f; // Radius of the radial gradient
    public Vector2 radialGradientCenter = new Vector2(0.25f, 0.5f); // Center of the radial gradient in UV coordinates
    public Color defaultColorRight = new Color(0.6275f, 0.0902f, 0.0118f, 1.0f);
    public string gradientShiftProperty = "_GradientShift";
    public string radialGradientColorProperty = "_RadialGradientColor";

    public float minShift = 0.1f; // Minimum value of the gradient shift
    public float maxShift = 0.35f; // Maximum value of the gradient shift
    public float changeInterval = 2.0f; // Time interval between changes
    public float transitionSpeed = 1.0f; // Speed of transition to the new value
    private float currentGradientShift;
    private float targetGradientShift;
    private float timeSinceLastChange;

    // Gradient shift for radial
    public string circleRadiusProperty = "_CircleRadius";
    private float currentRadialShift;
    private float targetRadialShift;
    public float minShiftRadial = 0.1f;
    public float maxShiftRadial = 0.2f;

    private MaterialPropertyBlock propertyBlock;
    private Renderer targetRenderer;
    public float intensityMultiplier = 2.0f;
    public float intensityMultiplierWork = 3.0f;

    private bool hasStarted = false;
    public float minLightHero = 0f;
    public float maxLightHero = 1.5f;
    public float minLightWork = 0f;
    public float maxLightWork = 1.5f;
    public MobileVersion mobileVersion;
    private Vector3 desktopPosition;


    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        // Initialize material properties
        ResetToDefaultState();

        // store default position as variable before possible mobile changes 
        desktopPosition = transform.position;

        // Reposition the plane for non-mobile screens
        if (mobileVersion.screenIsMobile())
        {
            MoveRadialGradientToMobileHero();
        }

        // Gradient shifter code
        currentGradientShift = material.GetFloat(gradientShiftProperty);
        targetRadialShift = material.GetFloat(circleRadiusProperty);
        targetGradientShift = GetRandomGradientShift(minShift, maxShift);
        targetRadialShift = GetRandomGradientShift(minShiftRadial, maxShiftRadial);
        timeSinceLastChange = 0f;
    }

    void Update()
    {
        // Gradient shifter code
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= changeInterval)
        {
            targetGradientShift = GetRandomGradientShift(minShift, maxShift);
            targetRadialShift = GetRandomGradientShift(minShiftRadial, maxShiftRadial);
            timeSinceLastChange = 0f;
        }

        // Update current gradient shift values
        currentGradientShift = Mathf.Lerp(currentGradientShift, targetGradientShift, Time.deltaTime * transitionSpeed);
        currentRadialShift = Mathf.Lerp(currentRadialShift, targetRadialShift, Time.deltaTime * transitionSpeed);

        // Update MaterialPropertyBlock
        propertyBlock.SetFloat(gradientShiftProperty, currentGradientShift);
        propertyBlock.SetFloat(circleRadiusProperty, currentRadialShift);
        targetRenderer.SetPropertyBlock(propertyBlock);

        // Determine which shift value to use based on hasStarted
        float currentShift = hasStarted ? currentRadialShift : currentGradientShift;
        float targetShift = hasStarted ? targetRadialShift : targetGradientShift;

        // Calculate intensity based on current shift value
        float intensity;
        if (hasStarted)
        {
            // When hasStarted is true, keep the direction as is (increase as gradient shift increases)
            intensity = Mathf.Lerp(minLightWork, maxLightWork, currentShift / maxShiftRadial) * intensityMultiplierWork;
        }
        else
        {
            // When hasStarted is false, invert the direction (decrease as gradient shift increases)
            intensity = Mathf.Lerp(maxLightHero, minLightHero, currentShift / maxShift) * intensityMultiplier;
        }

        // Update light intensity
        targetLight.intensity = intensity;
    }

    float GetRandomGradientShift(float min, float max)
    {
        return Random.Range(min, max);
    }

    void ResetToDefaultState()
    {
        material.SetColor("_ColorRight", defaultColorRight);
        material.SetFloat("_RadialGradientOpacity", 0.0f); // Ensure radial gradient is initially invisible
        material.SetVector("_RadialGradientCenter", radialGradientCenter); // Apply the radial gradient center position
    }

    // Function to start fading out the red intensity and fading in the radial gradient with delay
    public void FadeOutRedAndFadeInRadialGradient()
    {
        // Reposition the plane for non-mobile screens
        if (mobileVersion.screenIsMobile())
        {
            MoveRadialGradientToMobileWork();
        }
        hasStarted = true;
        StartCoroutine(FadeOutRedAndFadeInRadialGradientCoroutine());
    }

    private IEnumerator FadeOutRedAndFadeInRadialGradientCoroutine()
    {
        yield return StartCoroutine(FadeInRadialGradientCoroutine(fadeInDuration));
        StartCoroutine(FadeOutRedCoroutine());
    }

    private IEnumerator FadeOutRedCoroutine()
    {
        float elapsedTime = 0.0f;
        Color initialColor = material.GetColor("_ColorRight");
        Color targetColor = material.GetColor("_ColorLeft");

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            Color newColor = Color.Lerp(initialColor, targetColor, elapsedTime / fadeOutDuration);
            material.SetColor("_ColorRight", newColor);
            yield return null;
        }

        material.SetColor("_ColorRight", targetColor);
    }

    private IEnumerator FadeInRadialGradientCoroutine(float duration)
    {
        float longerDuration = duration * 2;
        float elapsedTime = 0.0f;
        float initialOpacity = material.GetFloat("_RadialGradientOpacity");

        while (elapsedTime < longerDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate new opacity using interpolation with quadratic easing for faster fade-in
            float t = Mathf.Pow(elapsedTime / longerDuration, 2.0f); // Adjust this exponent for faster acceleration
            float newOpacity = Mathf.Lerp(initialOpacity, 0.6f, t);
            material.SetFloat("_RadialGradientOpacity", newOpacity);

            yield return null; // Wait for the next frame
        }

        // Ensure final opacity is set precisely
        material.SetFloat("_RadialGradientOpacity", 0.6f);
    }

    // Function to fade in radial gradient independently with delay
    public void FadeInRadialGradient(float delay = 0.0f)
    {
        StartCoroutine(FadeInRadialGradientCoroutineWithDelay(fadeInDuration, delay));
    }

    private IEnumerator FadeInRadialGradientCoroutineWithDelay(float duration, float delay)
    {
        if (delay > 0.0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsedTime = 0.0f;
        float initialOpacity = material.GetFloat("_RadialGradientOpacity");

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate new opacity using interpolation with quadratic easing for faster fade-in
            float t = Mathf.Pow(elapsedTime / duration, 2.0f); // Adjust this exponent for faster acceleration
            float newOpacity = Mathf.Lerp(initialOpacity, 0.6f, t);
            material.SetFloat("_RadialGradientOpacity", newOpacity);

            yield return null; // Wait for the next frame
        }

        // Ensure final opacity is set precisely
        material.SetFloat("_RadialGradientOpacity", 0.6f);
    }

    // Function to fade out the radial gradient
    public void FadeOutRadialGradient()
    {
        StartCoroutine(FadeOutRadialGradientCoroutine(fadeOutDuration));
    }

    private IEnumerator FadeOutRadialGradientCoroutine(float duration)
    {
        float elapsedTime = 0.0f;
        float initialOpacity = material.GetFloat("_RadialGradientOpacity");

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate new opacity using interpolation
            float t = elapsedTime / duration;
            float newOpacity = Mathf.Lerp(initialOpacity, 0.0f, t);
            material.SetFloat("_RadialGradientOpacity", newOpacity);

            yield return null; // Wait for the next frame
        }

        // Ensure final opacity is set to zero
        material.SetFloat("_RadialGradientOpacity", 0.0f);
    }

    // REVERSE BACK TO HERO
    // Function to reverse the fading of the red and radial gradient
public void ReverseFadeOutRedAndFadeInRadialGradient()
{
     // Reposition the plane for non-mobile screens
    if (mobileVersion.screenIsMobile())
    {
        MoveRadialGradientToMobileHero();
    }
    StartCoroutine(ReverseFadeOutRedAndFadeInRadialGradientCoroutine());
}

private IEnumerator ReverseFadeOutRedAndFadeInRadialGradientCoroutine()
{
    yield return StartCoroutine(FadeOutRadialGradientCoroutine(fadeOutDuration));  // Fade out the radial gradient
    StartCoroutine(FadeInRedCoroutine());  // Fade in the red color
}

private IEnumerator FadeInRedCoroutine()
{
    float elapsedTime = 0.0f;
    Color initialColor = material.GetColor("_ColorRight");
    Color targetColor = defaultColorRight;  // Use the original red color (or whatever red is appropriate)

    while (elapsedTime < fadeOutDuration)
    {
        elapsedTime += Time.deltaTime;
        Color newColor = Color.Lerp(initialColor, targetColor, elapsedTime / fadeOutDuration);
        material.SetColor("_ColorRight", newColor);
        yield return null;
    }

    material.SetColor("_ColorRight", targetColor);  // Ensure final color is set to the red color
}


    public void SetRadialGradientColor(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color targetColor))
        {
            StartCoroutine(SmoothColorTransition(targetColor));
        }
        else
        {
            Debug.LogError($"Invalid hex color: {hexColor}");
        }
    }

    private IEnumerator SmoothColorTransition(Color targetColor)
    {
        // Get the current color from the material
        Color initialColor = material.GetColor(radialGradientColorProperty);
        float elapsedTime = 0f;
        float duration = 0.75f; // Adjust duration for the smoothness of transition

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Interpolate the color
            Color currentColor = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
            material.SetColor(radialGradientColorProperty, currentColor);

            yield return null; // Wait for the next frame
        }

        // Ensure the final color is set precisely
        material.SetColor(radialGradientColorProperty, targetColor);
    }

    public void MoveRadialGradientToMobileHero()
    {
        transform.position = new Vector3(6f, 8f, transform.position.z);
    }
    public void MoveRadialGradientToMobileWork()
    {
        transform.position = new Vector3(-12.5f, 8f, transform.position.z);
    }
    public void MoveRadialGradientToDesktop()
    {
        transform.position = desktopPosition;
    }

}
