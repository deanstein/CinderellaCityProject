using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NewPath : MonoBehaviour {

	private List<Vector3> points = new List<Vector3>();
	public int pointLenght = 0;
	public Vector3 mousePos;
	public string pathName;
	public bool errors;
	public bool exit;
	public GameObject par;

    [HideInInspector]
    [SerializeField]
    public PathType PathType = PathType.PeoplePath; 

	public List<Vector3> PointsGet()
	{
		return points;
	}

	public void PointSet(int index, Vector3 pos)
	{
		points.Add(pos);

		if(par == null)
		{
			par = new GameObject();
			par.name = "New path points";
			par.transform.parent = gameObject.transform;
		}

		var prefab = GameObject.Find("Population System").GetComponent<PopulationSystemManager>().pointPrefab;
		var obj = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
		obj.name = "p" + index;
		obj.transform.parent = par.transform;
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
    	Selection.activeGameObject = gameObject;
    	ActiveEditorTracker.sharedTracker.isLocked = true;

    	Gizmos.color = Color.green;

    	if(pointLenght > 0 && !exit)
    	{
			Gizmos.DrawLine(points[pointLenght - 1], mousePos);
		}
		if(pointLenght > 1)
		{
			for(int i = 0; i < pointLenght - 1; i++)
				Gizmos.DrawLine(points[i], points[i + 1]);
		}
    }
#endif
}
