using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class Utils
{
    public class ListUtils
    {

    }

    public class GeometryUtils
    {
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
                Debug.Log(meshPos);
                gameObjectMeshPosList.Add(meshPos);
            }

            return gameObjectMeshPosList;
        }

        // get a random point on the scene's current navmesh within some radius from a starting point
        public static Vector3 GetRandomNavMeshPointWithinRadius(Vector3 startingPoint, float radius, bool stayOnLevel)
        {
            // get a random direction within the radius
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;

            // apply the random direction from the starting point
            randomDirection += startingPoint;

            // set up the hit and final position
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;

            // if we get a hit
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                // if stayOnLevel is true, replace the y-value with the original position's y-value
                if (stayOnLevel)
                {
                    finalPosition = new Vector3(hit.position.x, startingPoint.y, hit.position.z);
                }

                else
                {
                    // the hit position could wind up on the roof
                    // so clamp the y (up) value to ~the 2nd floor height (~10 meters) at all times
                    if (hit.position.y > 10)
                    {
                        finalPosition = new Vector3(hit.position.x, 10, hit.position.z);
                    }
                    else
                    {
                        finalPosition = hit.position;
                    }
                }

                // optional: visualize the location and a connecting line
                //Debug.DrawLine(this.gameObject.transform.position, finalPosition, Color.red, 100f);
                //Debug.DrawLine(finalPosition, new Vector3(finalPosition.x, finalPosition.y + 1, finalPosition.z), Color.red, 100f);
            }

            return finalPosition;
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
                Debug.Log("Found a MeshChunk to get bounds info from: " + gameObjectMeshRendererArray[i]);

                Bounds bounds = gameObjectMeshRendererArray[i].bounds;

                //Debug.Log("Bounds: " + bounds);
                float dimX = bounds.extents.x;
                float dimY = bounds.extents.y;
                float dimZ = bounds.extents.z;
                Debug.Log("Mesh dimensions for " + gameObjectMeshRendererArray[i] + dimX + "," + dimY + "," + dimZ);

                List<float> XYZList = new List<float>();
                XYZList.Add(dimX);
                XYZList.Add(dimY);
                XYZList.Add(dimZ);

                float maxXYZ = XYZList.Max();
                Debug.Log("Max XYZ dimension for " + gameObjectMeshRendererArray[i] + ": " + maxXYZ);

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
            //Debug.Log("Target height: " + targetHeight);
            float currentHeight = GetMaxGOBoundingBoxDimension(gameObjectToScale);
            //Debug.Log("Current height: " + currentHeight);

            float scaleFactor = (targetHeight / currentHeight) * ((gameObjectToScale.transform.localScale.x + gameObjectToScale.transform.localScale.y + gameObjectToScale.transform.localScale.z) / 3);

            // scale the prefab to match the height of its replacement
            gameObjectToScale.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            Debug.Log("<b>Scaled </b>" + gameObjectToScale + " <b>to match</b> " + gameObjectToMatch + " <b>(" + scaleFactor + " scale factor)</b>");
        }
    }
}

