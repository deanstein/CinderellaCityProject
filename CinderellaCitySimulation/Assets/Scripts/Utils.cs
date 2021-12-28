using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Offers a variety of low-level, common operations shared among other scripts
/// </summary>

public static class ArrayUtils
{
    // create a subset from a range of indices
    public static T[] RangeSubset<T>(this T[] array, int startIndex, int length)
    {
        T[] subset = new T[length];
        Array.Copy(array, startIndex, subset, 0, length);
        return subset;
    }

    // create a subset from a specific list of indices
    public static T[] Subset<T>(this T[] array, params int[] indices)
    {
        T[] subset = new T[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            subset[i] = array[indices[i]];
        }
        return subset;
    }

    public static string[] ShuffleArray(string[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia
        for (int t = 0; t < texts.Length; t++)
        {
            string tmp = texts[t];
            int r = UnityEngine.Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }

        return texts;
    }
}

public class FileDirUtils
{
    public static string[] GetAllFilesInDirOfTypes(string dirPath, string[] fileTypes)
    {
        List<string> filePathsList = new List<string>();

        foreach (string fileType in fileTypes)
        {
            string formattedFileType = "*." + fileType;
            string[] filePaths = System.IO.Directory.GetFiles(dirPath, formattedFileType);
            foreach (string filePath in filePaths)
            {
                filePathsList.Add(filePath);
            }
        }

        return filePathsList.ToArray();
    }

    // remove a 3-letter extension and the dot from a name or path
    public static string RemoveExtension(string nameOrPathWithExtension)
    {
        string nameOrPathWithoutExtension = nameOrPathWithExtension.Substring(0, nameOrPathWithExtension.Length - 4);

        return nameOrPathWithoutExtension;
    }

    // eliminate "Assets/Resources/" from a path
    public static string ConvertProjectPathToRelativePath(string projectPath)
    {
        string relativePath = projectPath.Substring(UIGlobals.projectResourcesPath.Length, ((projectPath.Length) - UIGlobals.projectResourcesPath.Length));

        return relativePath;
    }

    public static string[] ConvertPathsToRelativeAndRemoveExtensions(string[] filePaths)
    {
        List<string> finalPathsList = new List<string>();

        foreach (string filePath in filePaths)
        {
            finalPathsList.Add(FileDirUtils.ConvertProjectPathToRelativePath(FileDirUtils.RemoveExtension(filePath)));
        }

        return finalPathsList.ToArray();
    }
}

// TODO: move these out of Utils to top-level classes
public class Utils
{
    public class DebugUtils
    {
        // print debug messages in the Editor console?
        static bool printDebugMessages = true;

        public static void DebugLog(string message)
        {
            // only print messages if the flag is set, and if we're in the Editor - otherwise, don't
            if (printDebugMessages && Application.isEditor)
            {
                Debug.Log(message);
            }
        }
    }

    public class StringUtils
    {
        // remove spaces, punctuation, and other characters from a string
        public static string CleanString(string messyString)
        {
            // remove spaces
            string cleanString = messyString.Replace(" ", "");

            // remove colons
            cleanString = cleanString.Replace(":", "");

            // remove dashes
            cleanString = cleanString.Replace("-", "");

            // remove the "19" if used in year syntax
            cleanString = cleanString.Replace("19", "");

            return cleanString;
        }

        // converts an array of friendly UI names into Scene names
        public static List<string> ConvertFriendlyNamesToSceneNames(List<string> friendlyNames)
        {
            List<string> convertedNames = new List<string>();

            foreach (string friendlyName in friendlyNames)
            {
                string convertedName = CleanString(friendlyName);
                convertedNames.Add(convertedName);
            }

            return convertedNames;
        }

        // gets the index of a friendly name given a scene name
        public static string ConvertSceneNameToFriendlyName(string sceneName)
        {
            // get the index of the scene we're in
            int sceneIndex = SceneGlobals.availableTimePeriodSceneNamesList.IndexOf(sceneName);

            if (sceneIndex != -1)
            {
                // now get the associated friendly name
                string friendlyName = SceneGlobals.availableTimePeriodFriendlyNames[sceneIndex];

                return friendlyName;
            }
            else
            {
                return "0000 Experimental";
            }
        }

        // return true if this string is found at all in the given array
        public static bool TestIfAnyListItemContainedInString(List<string> listOfStringsToSearchFor, string stringToSearchIn)
        {
            foreach (string searchForString in listOfStringsToSearchFor)
            {
                if (stringToSearchIn.Contains(searchForString))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class MathUtils
    {
        public static float RemapRange(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }

    public class GeometryUtils
    {
        // gets all children in the root object
        public static GameObject[] GetAllTopLevelChildrenInObject(GameObject parentObject)
        {
            List<GameObject> childrenList = new List<GameObject>();

            foreach (Transform trans in parentObject.transform)
            {
                childrenList.Add(trans.gameObject);
            }
            GameObject[] childrenObjects = childrenList.ToArray();

            return childrenObjects;
        }

        // get all children transforms given a parent transform
        public static List<Transform> GetAllChildrenTransformsInTransform(Transform parent)
        {
            List<Transform> children = new List<Transform>();

            int count = parent.childCount;

            for (int i = 0; i < count; i++)
            {
                children.Add(parent.GetChild(i));
            }

            return children;
        }

        // get a point on a mesh
        public static List<Vector3> GetPointOnMeshAtIndex(GameObject meshParent, int vertexIndex)
        {

            MeshFilter[] meshes = meshParent.GetComponentsInChildren<MeshFilter>();

            Vector3 meshPos = new Vector3(0, 0, 0);
            List<Vector3> gameObjectMeshPosList = new List<Vector3>();

            foreach (MeshFilter mesh in meshes)
            {
                Vector3[] vertices = mesh.mesh.vertices;
                meshPos = meshParent.transform.TransformPoint(vertices[vertexIndex]);
                //Utils.DebugUtils.DebugLog(meshPos);
                gameObjectMeshPosList.Add(meshPos);
            }

            return gameObjectMeshPosList;
        }

        // determine if an object is close enough to a valid nav mesh point to be considered on the nav mesh
        public static bool GetIsOnNavMeshWithinTolerance(GameObject gameObjectToMeasure, float tolerance)
        {
            if (Vector3.Distance(gameObjectToMeasure.transform.position, GetNearestPointOnNavMesh(gameObjectToMeasure.transform.position, tolerance)) < tolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // get a point on the scene's current navmesh within some radius from a starting point
        public static Vector3 GetNearestPointOnNavMesh(Vector3 startingPoint, float radius)
        {
            // set up the hit and final position
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;

            // if we get a hit
            if (NavMesh.SamplePosition(startingPoint, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            else
            {
                Utils.DebugUtils.DebugLog("Failed to find a point on the NavMesh.");
            }

            return finalPosition;
        }

        // get a random point on the scene's current navmesh within some radius from a starting point
        public static Vector3 GetRandomPointOnNavMeshFromPool(Vector3 currentPosition, Vector3[] positionPool, float minDistance, float maxDistance, bool stayOnLevel)
        {
            // get a random selection from the pool
            Vector3 randomPosition = positionPool[UnityEngine.Random.Range(0, positionPool.Length)];

            // when picking from the random position pool,
            // we should try to adhere to the specified radius and stayOnLevel requests
            // so try several times, before giving up and falling back to a random point
            int maxTries = 1000;
            for (var i = 0; i < maxTries;  i++)
            {
                float distance = Vector3.Distance(currentPosition, randomPosition);
                bool isIdealDistance = distance > minDistance && distance < maxDistance;
                bool isOnLevel = Mathf.Abs(currentPosition.y - randomPosition.y) < NPCControllerGlobals.maxStepHeight;

                // test if we meet the specified criteria
                if (isIdealDistance && isOnLevel)
                {
                    // if so, return out of this for loop
                    return randomPosition;
                }
                else
                {
                    // get a different random position and try again
                    randomPosition = positionPool[UnityEngine.Random.Range(0, positionPool.Length)];
                }
            }

            return randomPosition;
        }

        // get a random point on the scene's current navmesh within some radius from a starting point
        public static Vector3 GetRandomPointOnNavMesh(Vector3 startingPoint, float radius, bool stayOnLevel)
        {
            // get a random direction within the radius
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;

            // apply the random direction from the starting point
            randomDirection += startingPoint;

            // set up a potential hit
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;

            // how many times to try
            int tries = 30;

            // if stayOnLevel is true, try several times to get a new hit that
            // maintains the y-value/height
            if (stayOnLevel)
            {
                // try up to 30 times to get a new hit point 
                for (var i = 0; i < tries; i++)
                {
                    // if we get a hit
                    if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
                    {
                        // check if the hit point is at a different height than the starting point
                        // otherwise, try another hit
                        if (Mathf.Abs(hit.position.y - startingPoint.y) < NPCControllerGlobals.maxStepHeight)
                        {
                            finalPosition = hit.position;
                            return finalPosition;
                        }
                    }
                }

                return finalPosition;
            }
            else
            {
                // if we get a hit
                if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
                {
                    // the hit position could wind up on the roof
                    // so clamp the y (up) value to ~the 2nd floor height (~10 meters) at all times
                    if (hit.position.y > 10)
                    {
                        finalPosition = new Vector3(hit.position.x, 10, hit.position.z);
                        return finalPosition;
                    }
                    else
                    {
                        finalPosition = hit.position;
                        return finalPosition;
                    }
                }

                if (finalPosition == Vector3.zero)
                {
                    Utils.DebugUtils.DebugLog("ERROR: Failed to find a random point on NavMesh, so used the world origin instead.");
                }

                return finalPosition;
            }

            // optional: visualize the location and a connecting line
            //Debug.DrawLine(this.gameObject.transform.position, finalPosition, Color.red, 100f);
            //Debug.DrawLine(finalPosition, new Vector3(finalPosition.x, finalPosition.y + 1, finalPosition.z), Color.red, 100f);
        }

        // used to correct nav mesh points which are just slightly above the floor
        // ensures the player will always be on the floor when relocating to a camera
        // returns zero if the raycast didn't hit anything
        public static Vector3 GetNearestRaycastPointBelowPos(Vector3 initialPosition, float maxDistance)
        {
            NavMeshHit navhit;
            if (NavMesh.SamplePosition(initialPosition, out navhit, maxDistance, NavMesh.AllAreas))
            {
                Ray r = new Ray(navhit.position, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit, maxDistance, LayerMask.GetMask("Default")))
                {
                    //Debug.Log("Hit distance: " + hit.distance);
                    Vector3 adjustedPosition = initialPosition;
                    adjustedPosition.y = initialPosition.y - hit.distance;

                    return adjustedPosition;
                }
            }

            return Vector3.zero;
        }

        // define how to get the max height of a gameObject
        public static float GetMaxGOBoundingBoxDimension(GameObject gameObjectToMeasure)
        {
            // create an array of MeshChunks found within this GameObject so we can get the bounding box size
            MeshRenderer[] gameObjectMeshRendererArray = gameObjectToMeasure.GetComponentsInChildren<MeshRenderer>(true);
            MeshFilter[] gameObjectMeshFilterArray = gameObjectToMeasure.GetComponentsInChildren<MeshFilter>(true);
            SkinnedMeshRenderer[] gameObjectSkinnedMeshRendererArray = gameObjectToMeasure.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            // instantiate bounds - we don't now what kind of bounds yet
            Bounds bounds;

            // assume, for now, that the object to measure has a single mesh of some sort
            // this could be a mesh renderer, or a skinned mesh renderer
            if (gameObjectMeshFilterArray.Length > 0)
            {
                bounds = gameObjectMeshFilterArray[0].sharedMesh.bounds;
                //DebugUtils.DebugLog("Mesh renderer found for " + gameObjectToMeasure + ". Bounds: " + bounds);

                // ensure the object is enabled so it can be measured
                gameObjectMeshFilterArray[0].gameObject.SetActive(true);
            }
            else if (gameObjectSkinnedMeshRendererArray.Length > 0)
            {
                bounds = gameObjectSkinnedMeshRendererArray[0].bounds;
                //DebugUtils.DebugLog("Skinned mesh renderer found for " + gameObjectToMeasure + ". Bounds: " + bounds);
            }
            else
            {
                DebugUtils.DebugLog("ERROR: Failed to get max bounding box dimensions because no meshes of any type were found to measure within " + gameObjectToMeasure.name);
                return 1;
            }

            // initialize the game object's maximum bounding box dimension
            float gameObjectMaxDimension = 0;
            
            //Utils.DebugUtils.DebugLog("Found a MeshChunk to get bounds info from: " + gameObjectMeshRendererArray[i]);

            //Debug.Log("Bounds: " + bounds);
            float dimX = bounds.extents.x;
            float dimY = bounds.extents.y;
            float dimZ = bounds.extents.z;
            //Utils.DebugUtils.DebugLog("Mesh dimensions for " + gameObjectMeshFilterArray[i] + dimX + "," + dimY + "," + dimZ);

            List<float> XYZList = new List<float>();
            XYZList.Add(dimX);
            XYZList.Add(dimY);
            XYZList.Add(dimZ);

            float maxXYZ = XYZList.Max();
            //Utils.DebugUtils.DebugLog("Max XYZ dimension for " + gameObjectMeshFilterArray[i] + ": " + maxXYZ);

            // set the max dimension to the max XYZ value
            gameObjectMaxDimension = maxXYZ;

            float gameObjectMaxHeight = gameObjectMaxDimension;

            //DebugUtils.DebugLog("Max height of " + gameObjectToMeasure.name + ": " + gameObjectMaxHeight);
            return gameObjectMaxHeight;
        }

        // define how to scale one GameObject to match the height of another
        public static void ScaleGameObjectToMatchOther(GameObject gameObjectToScale, GameObject gameObjectToMatch)
        {
            // get the height of the object to be replaced
            float targetHeight = GetMaxGOBoundingBoxDimension(gameObjectToMatch);
            //Utils.DebugUtils.DebugLog("Target height: " + targetHeight);
            float currentHeight = GetMaxGOBoundingBoxDimension(gameObjectToScale);
            //Utils.DebugUtils.DebugLog("Current height: " + currentHeight);

            // run NPCs (people) through an extra set of scaling, to reduce their size if required
            if (gameObjectToMatch.name.Contains("people"))
            {
                float scaleDown = ManageNPCControllers.GetRandomNPCScaleDownFactor(gameObjectToScale);
                DebugUtils.DebugLog("Applying additional NPC scale down factor: " + scaleDown + " for object: " + gameObjectToScale.name);
                targetHeight = targetHeight * scaleDown;
            }

            // only scale if the target value is non-zero
            if (targetHeight != 0)
            {
                Utils.GeometryUtils.ScaleGameObjectToMaxHeight(gameObjectToScale, targetHeight);
            }
        }

        // define how to scale a GameObject to match a height
        public static void ScaleGameObjectToMaxHeight(GameObject gameObjectToScale, float targetHeight)
        {
            //Utils.DebugUtils.DebugLog("Target height: " + targetHeight);
            float currentHeight = GetMaxGOBoundingBoxDimension(gameObjectToScale);
            //Utils.DebugUtils.DebugLog("Current height: " + currentHeight);

            float scaleFactor = (targetHeight / currentHeight) * ((gameObjectToScale.transform.localScale.x + gameObjectToScale.transform.localScale.y + gameObjectToScale.transform.localScale.z) / 3);

            // scale the prefab to match the height of its replacement
            gameObjectToScale.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            Utils.DebugUtils.DebugLog("<b>Scaled </b>" + gameObjectToScale + " <b>to max height: </b> " + targetHeight + " <b>(" + scaleFactor + " scale factor)</b>");
        }

        // randomly rotate a GameObject about its vertical axis (Y)
        public static void RandomRotateGameObjectAboutY(GameObject gameObjectToRotate)
        {
            Quaternion currentRotation = gameObjectToRotate.transform.rotation;
            float randomYRotation = UnityEngine.Random.Range(-1.0f, 1.0f);
            Quaternion adjustedRotation = new Quaternion(currentRotation.x, randomYRotation, currentRotation.z, currentRotation.w);
            gameObjectToRotate.transform.rotation = adjustedRotation;
        }

        // define how to scale a GameObject randomly within a range
        public static void ScaleGameObjectRandomlyWithinRange(GameObject gameObjectToScale, float minScale, float maxScale)
        {

            float scaleFactor = UnityEngine.Random.Range(minScale, maxScale);

            // scale the prefab to match the height of its replacement
            gameObjectToScale.transform.localScale = new Vector3(gameObjectToScale.transform.localScale.x * scaleFactor, gameObjectToScale.transform.localScale.y * scaleFactor, gameObjectToScale.transform.localScale.z * scaleFactor);
        }
    }
}

