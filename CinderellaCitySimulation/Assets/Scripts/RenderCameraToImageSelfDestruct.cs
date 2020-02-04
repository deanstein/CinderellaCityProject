using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]

public class RenderCameraToImageSelfDestruct : MonoBehaviour
{
    // attach this script to a camera to write an image of the camera's view to the local file system
    // to initiate, reimport an asset in the Editor, or enable the flag to run in game mode
    // this script runs on OnPostREnder, then self-destructs (it only needs to run once)

    // defaults to false, so will only run in Editor
    // however some scripts may call this to execute once, while the game is running
    public bool runInGameMode = false;

    // if specified by another script, we'll write this camera's texture to a file
    public string filePath;

    private void Start()
    {
        // only execute if we're *not* in Play mode
        if (!Application.isPlaying)
        {
            // render the camera (required to kick off OnPostRender)
            this.GetComponent<Camera>().Render();
        }
    }

    void OnPostRender()
    {
        // only execute if we're *not* in Play mode, or if the override is true
        if (!Application.isPlaying || runInGameMode)
        {
            // define the width and height of the image that will be created from this camera
            int width = Screen.width;
            int height = Screen.height;

            // create a new texture with the width and height of the camera
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, true);

            // read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            texture.Apply();

            // create the texture bytes
            var Bytes = texture.EncodeToPNG();

            //DestroyImmediate(texture);

            // if a file path is specified, write the image there
            if (filePath != null)
            {
                // the full path of the file to be created
                // ensure this extension matches the encoding below
                string fullPath = filePath + this.name + ".png";

                // write bytes to file system
                File.WriteAllBytes(fullPath, Bytes);

                Debug.Log("<b>Rendered a camera to an image:</b> " + fullPath);
            }
            //otherwise store the texture globally for other scripts to access
            else
            {
                // capture the image as a texture
                UIGlobals.outgoingFPSControllerCameraTexture = texture;
                // assign the texture by name
                CreateScreenSpaceUIElements.AssignCameraTextureToVariableByName();

                Debug.Log("<b>Rendered a camera to an internal variable.</b> ");
            }

            // must delete this script component so it doesn't run every frame
            DestroyImmediate(GetComponent<RenderCameraToImageSelfDestruct>());
        }
    }
}

