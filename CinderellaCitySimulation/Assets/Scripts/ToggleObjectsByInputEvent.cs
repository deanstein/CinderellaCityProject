using UnityEngine;

/// <summary>
/// Toggles geometry on/off via certain input events
/// </summary>

// this script should be attached to any object that needs to watch for shortcuts to toggle it, or its children, on/off
// this script also contains general object visibility utils used by other scripts
public class ToggleObjectsByInputEvent : MonoBehaviour {

     // watch for shortcuts every frame
    void Update()
    {
        /// object visibility shortcuts ///

        // people
        if (Input.GetKeyDown("p") && this.name.Contains(ObjectVisibilityGlobals.peopleObjectKeywords[0]))
        {
            ManageSceneObjects.ObjectState.ToggleTopLevelChildrenSceneObjects(this.gameObject);
        }
        // reset people to starting location
        if (Input.GetKeyDown("o") && this.name.Contains(ObjectVisibilityGlobals.peopleObjectKeywords[0]))
        {
            ManageNPCControllers.ResetAllNPCsToOriginalLocation();
        }

        // get all the photo objects in the scene
        if (Input.GetKeyDown("["))
        {
            //Debug.Log("These are the photo objects in the scene: " + ManageSceneObjects.GetAllHistoricPhotoCamerasInScene().Length);

            //foreach (Camera currentCamera in ManageSceneObjects.GetAllHistoricPhotoCamerasInScene())
            //{
            //    Debug.Log("Camera: " + currentCamera.transform.parent.name);
            //}

        }
    }
}