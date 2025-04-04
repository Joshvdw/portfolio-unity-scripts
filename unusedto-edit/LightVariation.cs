using UnityEngine;

public class RandomizedLightIntensity : MonoBehaviour
{
    public float minIntensity = 1.5f; // Minimum light intensity
    public float maxIntensity = 2.5f; // Maximum light intensity
    public float frequency = 1f;      // Frequency of the sine wave
    public float amplitude = 0.5f;    // Amplitude of the sine wave

    private Light _light;
    private float _initialIntensity;
    private float _randomOffset;

    void Start()
    {
        _light = GetComponent<Light>();
        _initialIntensity = _light.intensity;
        _randomOffset = Random.Range(0f, 2f * Mathf.PI); // Random offset for sine wave
    }

    void Update()
    {
        // Calculate the current intensity based on sine wave
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.Sin((Time.time * frequency) + _randomOffset) * amplitude);

        // Apply the intensity to the light
        _light.intensity = intensity;
    }
}
