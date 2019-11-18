using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneByName : MonoBehaviour {

    public bool buttonClicked;

	// Use this for initialization
	void Start ()
    {
        // the main menu needs to async load the larger mall scene
		if (this.name == "MainMenu")
        {
            StartCoroutine(LoadScene("80s-90s"));
            //StartCoroutine(LoadSceneAsync("80s-90s"));
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        /*
        // Press the space key to load the scene
        if (Input.GetKeyDown(KeyCode.Space))
        {
            buttonClicked = true;
        }
        */
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        Debug.Log("Async loading..." + sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone && !buttonClicked)
        {
            yield return null;
        }
    }

    IEnumerator LoadScene(string sceneName)
    {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;
        Debug.Log("Pro :" + asyncOperation.progress);
        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            //Output the current progress
            Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                //Wait to you press the space key to activate the Scene
                if (Input.GetKeyDown(KeyCode.Space))
                    //Activate the Scene
                    asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

}
