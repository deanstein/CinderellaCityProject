using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Refreshes an image sprite: rebuild a sprite's pixels from the definition on-disk or in-memory
/// </summary>

public class RefreshImageSprite : MonoBehaviour
{
    // set a flag as to whether this script should refresh automatically on enable, or only when requested
    public bool refreshOnEnable = false;

    void refreshSprite()
    {
        // this object should have an image component to update
        Image imageToUpdate = this.GetComponent<Image>();

        // determine the texture we should use based on this object's name
        Texture2D useTexture = CreateScreenSpaceUIElements.AssociateCameraTextureByName(this.gameObject.name);

        // if the refresh texture exists, set it to this object's image sprite
        if (useTexture)
        {
            imageToUpdate.sprite = Sprite.Create(useTexture, new Rect(0.0f, 0.0f, useTexture.width, useTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }

    private void OnEnable()
    {
        if (refreshOnEnable)
        {
            refreshSprite();
        }
    }
}