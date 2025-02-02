using UnityEngine;
using UnityEditor;
using System.IO;

///<summary>
/// Adds a menu item in Window -> CubeSplitter, which allows for extracting the texture files from a CubeMap for editing
///</summary>

public class CubeSplitter : EditorWindow
{
    Cubemap splitCube;
    Color[] CubeMapColors;
    int splitSize;

    [MenuItem("Window/CubeSplitter")]

    static void Init()
    {
        CubeSplitter window = (CubeSplitter)EditorWindow.GetWindow(typeof(CubeSplitter), false);

        window.maxSize = new Vector2(512, 155);
        window.minSize = window.maxSize;
        //window.title = ("Cube Splitter!"); // obsolete, don't want to spend the time fixing
        window.Show();

    }

    void OnGUI()
    {

        GUILayout.Label("Choose the Cube Map you want to save as 6 images and click EXPORT!", EditorStyles.boldLabel);
        splitCube = EditorGUILayout.ObjectField("My Cubemap:", splitCube, typeof(Cubemap), false) as Cubemap;
        GUILayout.Label("Make sure to set the Size to the same as the Cubemap you are using", EditorStyles.boldLabel);
        splitSize = EditorGUILayout.IntField("CubeMap Size: ", splitSize);

        if (GUILayout.Button("EXPORT!"))
        {
            if (splitCube)
            {
                Export();
            }

            if (!splitCube)
            {
                Debug.Log("Forget Something?");
            }
        }
    }

    void Export()
    {
        var filePath = AssetDatabase.GetAssetPath(splitCube);

        Texture2D tex = new Texture2D(splitSize, splitSize, TextureFormat.RGB24, false);
        CubeMapColors = splitCube.GetPixels(CubemapFace.PositiveY);
        tex.SetPixels(CubeMapColors, 0);

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath + "_Bot.png", bytes);

        CubeMapColors = splitCube.GetPixels(CubemapFace.NegativeY);
        tex.SetPixels(CubeMapColors, 0);

        tex.Apply();

        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath + "_Top.png", bytes);


        CubeMapColors = splitCube.GetPixels(CubemapFace.PositiveX);
        tex.SetPixels(CubeMapColors, 0);

        tex.Apply();

        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath + "_Lef.png", bytes);


        CubeMapColors = splitCube.GetPixels(CubemapFace.NegativeX);
        tex.SetPixels(CubeMapColors, 0);

        tex.Apply();

        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath + "_Rig.png", bytes);

        CubeMapColors = splitCube.GetPixels(CubemapFace.PositiveZ);
        tex.SetPixels(CubeMapColors, 0);

        tex.Apply();

        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath + "_Fro.png", bytes);

        CubeMapColors = splitCube.GetPixels(CubemapFace.NegativeZ);
        tex.SetPixels(CubeMapColors, 0);

        tex.Apply();

        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath + "_Bak.png", bytes);

        this.Close();
    }

}