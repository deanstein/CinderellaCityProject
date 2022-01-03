using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates screen-space objects like UI and background images
/// Uses auto-resuming coroutines
/// </summary>

public class AnimateScreenSpaceObject : AutoResumeCoroutines
{
    // the background slideshow image sequence is passed in from the calling script
    public Sprite[] backgroundSlideShowSequence;

    /// define screen space object animation speeds

    // speed of background image movement in x, y, z
    public Vector3 backgroundImageMovementSpeed = new Vector3(5, 0, 0);

    // amount of background image scaling
    public float backgroundImageScalingSpeed = 1.0003f;

    private void Start()
    {
        // start the auto-resume coroutine
        if (this.name.Contains("BackgroundSlideShow"))
        {
            StartAutoResumeCoroutine(PlayFullScreenSpriteSequence(this.GetComponent<Image>(), backgroundSlideShowSequence, 5));
        }
    }

    private void Update()
    {
        if (this.name.Contains("BackgroundSlideShow"))
        {
            MoveObjectContinuously(this.gameObject, backgroundImageMovementSpeed);
            ScaleObjectContinuously(this.gameObject, backgroundImageScalingSpeed);
        }
    }

    // move the screenspace object continuously
    public static void MoveObjectContinuously(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.localPosition += vector * Time.smoothDeltaTime;
    }

    // scale the screenspace object continuously
    public static void ScaleObjectContinuously(GameObject gameObject, float scaleFactor)
    {
        Vector3 currentScale = gameObject.GetComponent<Image>().rectTransform.localScale;
        Vector3 newScale = new Vector3(currentScale.x * scaleFactor, currentScale.y * scaleFactor, currentScale.z * scaleFactor);
        //Utils.DebugUtils.DebugLog("New scale: " + newScale);
        gameObject.GetComponent<Image>().rectTransform.localScale = newScale;
    }

    // play a sequence of sprites on an image component, switching images periodically
    public IEnumerator PlayFullScreenSpriteSequence(Image imageComponent, Sprite[] imageSequence, float displayTime)
    {
        // for each image in the array, display it for a certain amount of time, then show the next one
        Utils.DebugUtils.DebugLog("Playing image sequence...");
        int counter = 0;
        while (true)
        {
            // get the current sprite in the list and set it as the sprite for this image component
            imageComponent.sprite = imageSequence[counter];
            imageComponent.preserveAspect = true;
            imageComponent.SetNativeSize();

            // reset the scale before centering and full-screening
            imageComponent.rectTransform.localScale = new Vector3(1, 1, 1);

            // center and full-screen the current sprite
            TransformScreenSpaceObject.PositionObjectAtCenterofScreen(imageComponent.gameObject);
            TransformScreenSpaceObject.ScaleImageToFillScreen(imageComponent);

            // reset at the end of the list to repeat the sequence
            counter = (counter + 1) % imageSequence.Length;

            yield return new WaitForSeconds(displayTime);
        }
    }

    // fade an image to black
    IEnumerator FadeTo(Material material, float targetOpacity, float duration)
    {

        // Cache the current color of the material, and its initiql opacity.
        Color color = material.color;
        float startOpacity = color.a;

        // Track how many seconds we've been fading.
        float t = 0;

        while (t < duration)
        {
            // Step the fade forward one frame.
            t += Time.deltaTime;
            // Turn the time into an interpolation factor between 0 and 1.
            float blend = Mathf.Clamp01(t / duration);

            // Blend to the corresponding opacity between start & target.
            color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);

            // Apply the resulting color to the material.
            material.color = color;

            // Wait one frame, and repeat.
            yield return null;
        }
    }
}


