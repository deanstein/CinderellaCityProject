using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneByName : MonoBehaviour {

    public bool buttonClicked;
    public bool isSceneLoaded = false;
    AsyncOperation asyncOperation;

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
        // Press the space key to load the scene
        if (isSceneLoaded && Input.GetKeyDown(KeyCode.Space))
        {
            buttonClicked = true;
            asyncOperation.allowSceneActivation = true;
            Debug.Log("Is scene activated? " + asyncOperation.allowSceneActivation);
        }
    }
    

    IEnumerator LoadScene(string sceneName)
    {
      //  yield return null;

        //Begin to load the Scene you specify
        asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;
        Debug.Log("Pro :" + asyncOperation.progress);
        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            //Output the current progress
            Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");

            //// Check if the load has finished
            //if (asyncOperation.progress >= 0.9f)
            //{
            //    isSceneLoaded = true;
            //}

            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");
        Debug.Log("Scene activated? " + (asyncOperation.allowSceneActivation));
        isSceneLoaded = asyncOperation.isDone;
    }

}
