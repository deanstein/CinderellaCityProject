using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManageFPSControllers : MonoBehaviour {

    // this script needs to be attached to all FPSController objects
    public class FPSControllerGlobals
    {
        // the globally-available current FPSController
        public static GameObject activeFPSController;
    }

    // set the active controller to this object
    public void SetActiveFPSController()
    {
        FPSControllerGlobals.activeFPSController = this.gameObject;
    }

    void Start()
    {
        // set the active controller to this object
        SetActiveFPSController();
    }

    private void OnEnable()
    {
        // set the active controller to this object
        SetActiveFPSController();
    }
}

