using System;
using System.Linq;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public static class ArrayUtilities
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
}

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

    public class GeometryUtils
    {
        // gets distance between two points, allegedly faster than Unity's built-in distance method
        // (sourced from the Unity forums)
        public static float GetFastDistance(Vector3 v1, Vector3 v2)
        {
            float f;
            float f2;
            f = v1.x - v2.x;
            if (f < 0) { f = f * -1; }
            f2 = v1.z - v2.z;
            if (f2 < 0) { f2 = f2 * -1; }

            if (f > f2) { f2 = f; }
            // simulates a box-shaped distance calculation

            return f2;
        }

        public static List<Transform> GetAllChildrenTransforms(Transform parent)
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
            if (GetFastDistance(gameObjectToMeasure.transform.position, GetNearestPointOnNavMesh(gameObjectToMeasure.transform.position, tolerance)) < tolerance)
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
                float distance = GetFastDistance(currentPosition, randomPosition);
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
        public static Vector3 GetRandomNPoinOnNavMesh(Vector3 startingPoint, float radius, bool stayOnLevel)
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
            MeshRenderer[] gameObjectMeshRendererArray = gameObjectToMeasure.GetComponentsInChildren<MeshRenderer>();

            // store the game object's maximum bounding box dimension
            float gameObjectMaxDimension = 1;

            // for each MeshRenderer found, get the height and add it to the list
            for (int i = 0; i < gameObjectMeshRendererArray.Length; i++)
            {
                Utils.DebugUtils.DebugLog("Found a MeshChunk to get bounds info from: " + gameObjectMeshRendererArray[i]);

                Bounds bounds = gameObjectMeshRendererArray[i].bounds;

                //Debug.Log("Bounds: " + bounds);
                float dimX = bounds.extents.x;
                float dimY = bounds.extents.y;
                float dimZ = bounds.extents.z;
                Utils.DebugUtils.DebugLog("Mesh dimensions for " + gameObjectMeshRendererArray[i] + dimX + "," + dimY + "," + dimZ);

                List<float> XYZList = new List<float>();
                XYZList.Add(dimX);
                XYZList.Add(dimY);
                XYZList.Add(dimZ);

                float maxXYZ = XYZList.Max();
                Utils.DebugUtils.DebugLog("Max XYZ dimension for " + gameObjectMeshRendererArray[i] + ": " + maxXYZ);

                // set the max dimension to the max XYZ value
                gameObjectMaxDimension = maxXYZ;
            }

            float gameObjectMaxHeight = gameObjectMaxDimension;

            return gameObjectMaxHeight;
        }

        // define how to scale one GameObject to match the height of another
        public static void ScaleToMatchHeight(GameObject gameObjectToScale, GameObject gameObjectToMatch)
        {
            // get the height of the object to be replaced
            float targetHeight = GetMaxGOBoundingBoxDimension(gameObjectToMatch);
            //Utils.DebugUtils.DebugLog("Target height: " + targetHeight);
            float currentHeight = GetMaxGOBoundingBoxDimension(gameObjectToScale);
            //Utils.DebugUtils.DebugLog("Current height: " + currentHeight);

            Utils.GeometryUtils.ScaleGameObjectToMaxHeight(gameObjectToScale, targetHeight);
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
    }
}

