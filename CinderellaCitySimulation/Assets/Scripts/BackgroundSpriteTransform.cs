using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSpriteTransform : MonoBehaviour {

	// define the speed of the image movement in x, y, z
	public Vector3 speed = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		transform.localPosition += speed * Time.deltaTime;
		
	}

}