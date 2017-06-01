using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour {

    // User set fields
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float maxIntensityVelocity = 0.1f;

    // Light fields
    private Light lightSource;

    public void Start()
    {
        lightSource = GetComponentInChildren<Light>();
        if (lightSource == null)
        {
            Debug.LogError("Light flicker script must have a Light Component on a child GameObject!");
            return;
        }
    }

    public void Update()
    {
        if (lightSource != null)
        {
            //lightSource.intensity = (Random.Range(minFlicker, maxFlicker));
            lightSource.intensity += Random.Range(-maxIntensityVelocity, maxIntensityVelocity);
            lightSource.intensity = Mathf.Clamp(lightSource.intensity, minIntensity, maxIntensity);
        }
    }

}
