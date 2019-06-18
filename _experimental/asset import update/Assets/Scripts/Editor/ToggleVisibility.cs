using UnityEngine;
using UnityEditor;

public static class ToggleVisibility
{
    public static void ToggleGameObjectOff(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                Debug.Log("Turning off GameObject: " + gameObject);
            }
        }
    }
}

