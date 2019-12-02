using System.IO;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]

public class RenderCameraToImageSelfDestruct : MonoBehaviour
{
    // attach this script to a camera to write an image of the camera's view to the local file system
    // in the editor or in play mode, this script runs immediately, then self-destructs

    // path of the resulting image is passed in from AssetImportPipeline
    public string path;

    void OnPostRender()
    {
        // the full path of the file to be created
        // ensure this extension matches the encoding below
        string fullPath = path + this.name + ".png";

        // create a new texture with the width and height of the camera
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //Texture2D texture = new Texture2D(this.GetComponent<Camera>().pixelWidth, this.GetComponent<Camera>().pixelHeight, TextureFormat.RGB24, false);

        // read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        texture.Apply();

        // create the texture bytes
        var Bytes = texture.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(texture);

        // write bytes to file system
        File.WriteAllBytes(fullPath, Bytes);

        Debug.Log("<b>Rendered a camera to an image:</b> " + fullPath);

        // must delete this script component so it doesn't run every frame
        UnityEngine.Object.DestroyImmediate(this);
    }
}

