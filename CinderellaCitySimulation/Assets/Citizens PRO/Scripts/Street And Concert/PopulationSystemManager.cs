using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PopulationSystemManager : MonoBehaviour
{
	[SerializeField] private GameObject planePrefab;
	[SerializeField] private GameObject circlePrefab;
	public GameObject pointPrefab;
	[HideInInspector]
	public bool isConcert;
	[HideInInspector]
	public bool isStreet;
	[HideInInspector]
	public Vector3 mousePos;

	public void Concert(Vector3 pos)
	{
		isConcert = false;

		var emptyObject = new GameObject();
		emptyObject.transform.position = pos;
		emptyObject.name = "Audience";
#if UNITY_EDITOR
		Selection.activeGameObject = emptyObject;
#endif
		emptyObject.AddComponent<StandingPeopleConcert>();

		var _StandingPeopleConcert = emptyObject.GetComponent<StandingPeopleConcert>();
		_StandingPeopleConcert.planePrefab = planePrefab;
		_StandingPeopleConcert.circlePrefab = circlePrefab;
		_StandingPeopleConcert.SpawnRectangleSurface();
#if UNITY_EDITOR
    	Selection.activeGameObject = emptyObject;
    	ActiveEditorTracker.sharedTracker.isLocked = false;
#endif
	}

	public void Street(Vector3 pos)
	{
		isStreet = false;

		var emptyObject = new GameObject();
		emptyObject.transform.position = pos;
		emptyObject.name = "Talking people";
#if UNITY_EDITOR
		Selection.activeGameObject = emptyObject;
#endif
		emptyObject.AddComponent<StandingPeopleStreet>();

		var _StandingPeopleStreet = emptyObject.GetComponent<StandingPeopleStreet>();
		_StandingPeopleStreet.planePrefab = planePrefab;
		_StandingPeopleStreet.circlePrefab = circlePrefab;
		_StandingPeopleStreet.SpawnRectangleSurface();
#if UNITY_EDITOR		
    	Selection.activeGameObject = emptyObject;
    	ActiveEditorTracker.sharedTracker.isLocked = false;
#endif
	}
}
