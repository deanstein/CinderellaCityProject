using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenSprite : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// called on awake
    void Awake() {
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
		//get camera information, in screen space pixels
        float cameraHeight = Camera.main.scaledPixelHeight;
		//Debug.Log("Main camera pixel height: " + cameraHeight);
		
		float cameraWidth = Camera.main.aspect * cameraHeight;
		//Debug.Log("Main camera pixel width: " + cameraWidth);
		Debug.Log("Main camera resolution: " + cameraWidth + ", " + cameraHeight);


		 /// get sprite information in world space
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float spriteWidth = spriteSize.x;
		//Debug.Log("Sprite width: " + spriteWidth);
		float spriteHeight = spriteSize.y;
		//Debug.Log("Sprite height: " + spriteHeight);
		Debug.Log("Sprite resolution: " + spriteWidth + ", " + spriteHeight);

		// define the two possible camera-to-sprite ratios: by width or by height
		float cameraToSpriteWidthRatio = cameraWidth / spriteWidth;
		float cameraToSpriteHeightRatio = cameraHeight / spriteHeight;

		// get the current sprite scale
        Vector2 scale = transform.localScale;
		Debug.Log("Current sprite scale: " + scale);

		// scale differently depending on whether the image is landscape or horizontal
        if (spriteSize.x >= spriteSize.y) { // Landscape (or equal)
			Debug.Log("Landscape image detected");

			Debug.Log("Camera-to-sprite width ratio: " + cameraToSpriteWidthRatio);
			scale *= cameraToSpriteHeightRatio;
			Debug.Log("New scale factor: " + scale);
        } else { // Portrait
			Debug.Log("Portrait image detected");	
            scale *= cameraToSpriteWidthRatio;
        }
        
        //transform.position = Vector2.zero; // Optional
        transform.localScale = scale;
    }
}
