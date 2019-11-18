using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AnimateScreenSpaceObject : MonoBehaviour
{
    // the background slideshow sequence is passed in from the calling script
    public string[] mainMenuBackgroundSlideShowSequence;

    // define the speed of the image movement in x, y, z
    public Vector3 backgroundImageSlideshowSpeed = new Vector3(5, 0, 0);

    private void Start()
    {
        // the main menu
        if (this.name.Contains("BackgroundSlideShow"))
        {
            StartCoroutine(PlayFullScreenSpriteSequence(this.GetComponent<Image>(), mainMenuBackgroundSlideShowSequence, 5));
        }
    }

    private void Update()
    {
        if (this.name.Contains("BackgroundSlideShow"))
        {
            moveObjectContinuously(this.gameObject, backgroundImageSlideshowSpeed);
            scaleObjectContinuously(this.gameObject, 1.0003f);
        }
    }

    // move the screenspace object continuously
    public static void moveObjectContinuously(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.localPosition += vector * Time.deltaTime;
    }

    // scale the screenspace object continuously
    public static void scaleObjectContinuously(GameObject gameObject, float scaleFactor)
    {
        Vector3 currentScale = gameObject.GetComponent<Image>().rectTransform.localScale;
        Vector3 newScale = new Vector3(currentScale.x * scaleFactor, currentScale.y * scaleFactor, currentScale.z * scaleFactor);
        //Debug.Log("New scale: " + newScale);
        gameObject.GetComponent<Image>().rectTransform.localScale = newScale;
    }

    // play a sequence of sprites on an image component, switching images periodically
    IEnumerator PlayFullScreenSpriteSequence(Image imageComponent, string[] imageSequence, float displayTime)
    {
        // for each image in the array, display it for a certain amount of time, then show the next one
        Debug.Log("Playing image sequence...");
        int counter = 0;
        while (true)
        {
            // get the current sprite in the list and set it as the sprite for this image component
            imageComponent.sprite = (Sprite)Resources.Load(imageSequence[counter], typeof(Sprite));
            imageComponent.preserveAspect = true;
            imageComponent.SetNativeSize();

            // reset the scale before centering and full-screening
            imageComponent.rectTransform.localScale = new Vector3(1, 1, 1);

            // center and full-screen the current sprite
            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(imageComponent.gameObject);
            TransformScreenSpaceObject.ScaleObjectToFillCamera(imageComponent.gameObject);

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


