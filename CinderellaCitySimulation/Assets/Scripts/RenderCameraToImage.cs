using System.IO;
using UnityEngine;

public class RenderCameraToImage : MonoBehaviour
{
    bool captureRequested;
    bool captured;

    private void Start()
    {
        captured = false;
    }

    void OnPostRender()
    {
        CamCapture();
    }

    private void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            captureRequested = true;
        }
    }

    void CamCapture()
    {
        if (captured)
        {
            return;
        }
        if ((!captured) && (captureRequested))
        {
            // create a new texture with the width and height of the camera
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            //Texture2D texture = new Texture2D(this.GetComponent<Camera>().pixelWidth, this.GetComponent<Camera>().pixelHeight, TextureFormat.RGB24, false);

            // read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
            texture.Apply();

            // create the PNG bytes
            var Bytes = texture.EncodeToPNG();
            Destroy(texture);

            // write to the file system
            File.WriteAllBytes("Assets/Resources/UI/" + this.name + ".png", Bytes);

            captured = true;
            Debug.Log("Captured a thumbnail camera screenshot: " + this.name);

            //Camera.main.Render();
        }
    }

}