using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandingPeopleConcert : MonoBehaviour {

	[HideInInspector] public GameObject planePrefab;
	[HideInInspector] public GameObject circlePrefab;

	[HideInInspector] public GameObject surface;
	[HideInInspector] public Vector2 planeSize = new Vector2(1, 1);

    [Tooltip("People prefabs / Префабы людей")] public GameObject[] peoplePrefabs = new GameObject[0];
	[HideInInspector]
	List<Vector3> spawnPoints = new List<Vector3>();
	[HideInInspector] public GameObject target;
	[HideInInspector] public int peopleCount;


	[HideInInspector] public bool isCircle;
	[HideInInspector] public float circleDiametr = 1;
	[HideInInspector] public bool showSurface = true;

	public enum TestEnum{Rectangle, Circle};
    [Tooltip("Type of surface / Тип поверхности")] public TestEnum SurfaceType;

	[HideInInspector]
	public GameObject par;

    [HideInInspector] public bool looking;
    [HideInInspector] public float damping = 5;
    [HideInInspector] public float highToSpawn;

    public void OnDrawGizmos()
    {
        if (!isCircle)
            surface.transform.localScale = new Vector3(planeSize.x, 1, planeSize.y);
        else
            surface.transform.localScale = new Vector3(circleDiametr, 1, circleDiametr);
    }

    public void SpawnRectangleSurface()
    {
        if (surface != null) DestroyImmediate(surface);

        var plane = Instantiate(planePrefab, transform.position, Quaternion.identity) as GameObject;
        surface = plane;
        isCircle = false;

        plane.transform.eulerAngles = new Vector3(
            plane.transform.eulerAngles.x,
            plane.transform.eulerAngles.y,
                plane.transform.eulerAngles.z
        );

        plane.transform.position += new Vector3(0, 0.01f, 0);
        plane.transform.parent = transform;
        plane.name = "surface";
    }

    public void SpawnCircleSurface()
	{

		if(surface != null) DestroyImmediate(surface);

		var circle = Instantiate(circlePrefab, transform.position, Quaternion.identity) as GameObject;

		isCircle = true;

		circle.transform.eulerAngles = new Vector3(
    		circle.transform.eulerAngles.x,
    		circle.transform.eulerAngles.y,
   		 	circle.transform.eulerAngles.z
		);

		circle.transform.position += new Vector3(0, 0.01f, 0);
		circle.transform.parent = transform;
		circle.name = "surface";
		surface = circle;
	}

	public void RemoveButton()
	{
		if(par != null)
			DestroyImmediate(par);
	}

	public void PopulateButton()
	{
		RemoveButton();

		GameObject parGo = new GameObject();
		par = parGo;
		parGo.transform.parent = gameObject.transform;
		parGo.name = "people";

		spawnPoints.Clear();
		SpawnPeople(peopleCount);
	}

	void SpawnPeople (int _peopleCount)
	{
        int[] peoplePrefabIndexes = CommonUtils.GetRandomPrefabIndexes(_peopleCount, ref peoplePrefabs);

        for (int i = 0; i < _peopleCount; i++)
        {
            Vector3 randomPosition;

            if (!isCircle)
                randomPosition = RandomRectanglePosition();
            else
                randomPosition = RandomCirclePosition();

            if (randomPosition != Vector3.zero)
            {
                GameObject people = peoplePrefabs[peoplePrefabIndexes[i]];

                RaycastHit hit;

                GameObject obj = null;

                if (Physics.Raycast(randomPosition + Vector3.up * highToSpawn, Vector3.down, out hit, Mathf.Infinity))
                {
                    obj = Instantiate(people, new Vector3(randomPosition.x, hit.point.y, randomPosition.z), Quaternion.Euler(people.transform.rotation.x, transform.eulerAngles.y, people.transform.rotation.z)) as GameObject;
                }
                else
                {
                    continue;
                }

                PeopleController pc = obj.AddComponent<PeopleController>();

                spawnPoints.Add(obj.transform.position);

                if (target != null)
                {
                    pc.SetTarget(target.transform.position);

                    if (looking)
                    {
                        pc.target = target.transform;
                        pc.damping = damping;
                    }
                }

                pc.animNames = new string[4] { "idle1", "idle2", "cheer", "claphands" };
                obj.transform.parent = par.transform;
            }
		}
	}

	Vector3 RandomRectanglePosition ()
	{
		Vector3 randomPosition = new Vector3(0, 0, 0);

        for(int i = 0; i < 10; i++)
        {
			randomPosition.x = surface.transform.position.x - GetRealPlaneSize().x / 2 + Random.Range(0.0f, GetRealPlaneSize().x - 0.6f);
			randomPosition.z = surface.transform.position.z - GetRealPlaneSize().y / 2 + Random.Range(0.0f, GetRealPlaneSize().y - 0.6f);
			randomPosition.y = surface.transform.position.y;

        	if(IsRandomPositionFree(randomPosition))
        		return randomPosition;
        }

        return Vector3.zero;
	}

	Vector3 RandomCirclePosition ()
	{
		Vector3 center = surface.transform.position;
		float radius = GetRealPlaneSize().x / 2;

        for(int i = 0; i < 10; i++)
        {
            float randomRadius = Random.value * radius;
      		float ang = Random.value * 360;
        	Vector3 pos;
        	pos.x = center.x + randomRadius * Mathf.Sin(ang * Mathf.Deg2Rad);
        	pos.y = center.y;
        	pos.z = center.z + randomRadius * Mathf.Cos(ang * Mathf.Deg2Rad);

            if (IsRandomPositionFree(pos))
        		return pos;
        }

        return Vector3.zero;
	}

	bool IsRandomPositionFree (Vector3 pos)
	{
		for(int i = 0; i < spawnPoints.Count; i++)
		{
			if(spawnPoints[i].x - 0.6f < pos.x && spawnPoints[i].x + 1 > pos.x && spawnPoints[i].z - 0.5f < pos.z && spawnPoints[i].z + 0.6f > pos.z)
			return false;
		}
		return true;
	}

	Vector2 GetRealPlaneSize()
	{
		Vector3 meshSize = surface.GetComponent<MeshRenderer>().bounds.size;
		return new Vector2(meshSize.x, meshSize.z);
	}

	Vector2 GetRealPeopleModelSize()
	{
		Vector3 meshSize = peoplePrefabs[1].GetComponent<MeshRenderer>().bounds.size;
		return new Vector2(meshSize.x, meshSize.z);
	}

}
