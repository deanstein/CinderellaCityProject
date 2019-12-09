using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]

public class RenderCameraToImageSelfDestruct : MonoBehaviour
{
    // attach this script to a camera to write an image of the camera's view to the local file system
    // to initiate, reimport an asset in the Editor (will not work in Play mode)
    // this script runs immediately, then self-destructs (it only needs to run once)

    // path of the resulting image is passed in from AssetImportPipeline
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
        // only execute if we're *not* in Play mode
        if (!Application.isPlaying)
        {
            // define the width and height of the image that will be created from this camera
            int width = Screen.width;
            int height = Screen.height;

            // the full path of the file to be created
            // ensure this extension matches the encoding below
            string fullPath = filePath + this.name + ".png";

            // create a new texture with the width and height of the camera
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

            // read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, true);
            texture.Apply();

            // create the texture bytes
            var Bytes = texture.EncodeToPNG();
            DestroyImmediate(texture);

            // write bytes to file system
            File.WriteAllBytes(fullPath, Bytes);

            Debug.Log("<b>Rendered a camera to an image:</b> " + fullPath);

            // must delete this script component so it doesn't run every frame
            DestroyImmediate(GetComponent<RenderCameraToImageSelfDestruct>());
        }
    }
}

