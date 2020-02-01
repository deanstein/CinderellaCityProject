using System;
using UnityEngine;
using UnityEngine.UI;

public class RefreshImageSprite : MonoBehaviour
{
    private void OnEnable()
    {
        // this object should have an image component to update
        Image imageToUpdate = this.GetComponent<Image>();

        // determine the texture we should use based on this object's name
        Texture2D refreshTexture = CreateScreenSpaceUIElements.AssociateCameraTextureByName(this.gameObject);

        // if the refresh texture exists, set it to this object's image sprite
        if (refreshTexture)
        {
            imageToUpdate.sprite =  Sprite.Create(refreshTexture, new Rect(0.0f, 0.0f, refreshTexture.width, refreshTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }
}