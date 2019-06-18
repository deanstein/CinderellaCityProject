using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewScene : MonoBehaviour {
	//public void loadlevel(string level);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

 	if (Input.GetKeyDown("l")) {
		 SceneManager.LoadScene("CenterCourt");
	 }

		
		}

}