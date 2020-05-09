using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandingPeopleStreet : MonoBehaviour {

	[HideInInspector] public GameObject planePrefab;
	[HideInInspector] public GameObject circlePrefab;

	[HideInInspector] public GameObject surface;
	[HideInInspector] public Vector2 planeSize = new Vector2(1, 1);

    [Tooltip("People prefabs / Префабы людей")] public GameObject[] peoplePrefabs = new GameObject[0];
	[HideInInspector]
	public List<Vector3> spawnPoints = new List<Vector3>();

	[HideInInspector] public int peopleCount;

	[HideInInspector] public bool isCircle;
	[HideInInspector] public float circleDiametr = 1;
	[HideInInspector] public bool showSurface = true;

	public enum TestEnum{Rectangle, Circle};
    [Tooltip("Type of surface / Тип поверхности")] public TestEnum SurfaceType;
	[HideInInspector]
	public GameObject par;

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
		if(surface != null) DestroyImmediate(surface);

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
		par = null;
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
		int three = Random.Range(0, (int) (_peopleCount) / 3) * 3;
		int two = Random.Range(0, (int) (_peopleCount - three) / 2) * 2;
		int one = _peopleCount - three - two;

        int[] peoplePrefabIndexes = CommonUtils.GetRandomPrefabIndexes(peopleCount, ref peoplePrefabs);
        int peoplePrefabCount = 0;

        for(int i = 0; i < one; i++)
        {
			Vector3 randomPosition;

			if(!isCircle)
				randomPosition = RandomRectanglePosition();
			else 
				randomPosition = RandomCirclePosition();

            if (randomPosition != Vector3.zero)
            {
                RaycastHit hit;

                GameObject obj = null;

                if (Physics.Raycast(randomPosition + Vector3.up * highToSpawn, Vector3.down, out hit, Mathf.Infinity))
                {
                    obj = Instantiate(peoplePrefabs[peoplePrefabIndexes[peoplePrefabCount]], new Vector3(randomPosition.x, hit.point.y, randomPosition.z), Quaternion.identity) as GameObject;
                    peoplePrefabCount++;
                }
                else
                {
                    continue;
                }

                obj.AddComponent<PeopleController>();

                spawnPoints.Add(obj.transform.position);
                obj.transform.localEulerAngles = new Vector3(obj.transform.rotation.x, Random.Range(1, 359), obj.transform.rotation.z);

                obj.GetComponent<PeopleController>().animNames = new string[2]{"idle1", "idle2"};
				obj.transform.parent = par.transform;
			}
		}
		
		for(int i = 0; i < two / 2; i++)
		{

			Vector3 randomPosition;

			if(!isCircle)
				randomPosition = RandomRectanglePosition();
			else 
				randomPosition = RandomCirclePosition();

			

			if(randomPosition != Vector3.zero)
			{

				Vector3 randomPos1 = Vector3.zero;
				Vector3 randomPos2 = Vector3.zero;

				for(int f = 0; f < 100; f++)
				{
					for(int z = 0; z < 10; z++)
					{
						randomPos1 = randomPosition + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
						if(IsRandomPositionFree(randomPos1, Vector3.zero, Vector3.zero))
							break;

						else randomPos1 = Vector3.zero;
					}

					for(int x = 0; x < 10; x++)
					{
						randomPos2 = randomPosition + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
						if(IsRandomPositionFree(randomPos2, randomPos1, Vector3.zero))
							break;

						else randomPos2 = Vector3.zero;
					}
				

					if(randomPos1 != Vector3.zero && randomPos2 != Vector3.zero)
					{
						spawnPoints.Add(randomPos1);
						spawnPoints.Add(randomPos2);
						break;
					}
					else
					{
						randomPos1 = Vector3.zero;
						randomPos2 = Vector3.zero;
					}
				}

				if(randomPos1 != Vector3.zero && randomPos2 != Vector3.zero)
				{
					int prefabNum = Random.Range(0, peoplePrefabs.Length);

                    RaycastHit hit;

                    GameObject obj = null;

                    if (Physics.Raycast(randomPos1 + Vector3.up * highToSpawn, Vector3.down, out hit, Mathf.Infinity))
                    {
                        obj = Instantiate(peoplePrefabs[prefabNum], new Vector3(randomPos1.x, hit.point.y, randomPos1.z), Quaternion.identity) as GameObject;
                        peoplePrefabCount++;
                    }
                    else
                    {
                        continue;
                    }

                    obj.AddComponent<PeopleController>();
					obj.GetComponent<PeopleController>().animNames = new string[3]{"talk1", "talk2", "listen"};
					obj.transform.parent = par.transform;

					prefabNum = Random.Range(0, peoplePrefabs.Length);

                    RaycastHit hit1;

                    GameObject obs = null;

                    if (Physics.Raycast(randomPos2 + Vector3.up * highToSpawn, Vector3.down, out hit1, Mathf.Infinity))
                    {
                        obs = Instantiate(peoplePrefabs[prefabNum], new Vector3(randomPos2.x, hit1.point.y, randomPos2.z), Quaternion.identity) as GameObject;
                    }
                    else
                    {
                        continue;
                    }

                    obs.AddComponent<PeopleController>();
					obs.GetComponent<PeopleController>().animNames = new string[3]{"talk1", "talk2", "listen"};
					obs.transform.parent = par.transform;


					obs.GetComponent<PeopleController>().SetTarget(obj.transform.position);
					obj.GetComponent<PeopleController>().SetTarget(obs.transform.position);					
				}
			}
		}
		
		for(int i = 0; i < three / 3; i++)
		{
			Vector3 randomPosition;

			if(!isCircle)
				randomPosition = RandomRectanglePosition();
			else 
				randomPosition = RandomCirclePosition();

			if(randomPosition != Vector3.zero)
			{
				int prefabNum = Random.Range(0, peoplePrefabs.Length);

				Vector3 randomPos0 = Vector3.zero;
				Vector3 randomPos1 = Vector3.zero;
				Vector3 randomPos2 = Vector3.zero;

				for(int f = 0; f < 100; f++)
				{
					int z;
					int x;
					int c;

					for(z = 0; z < 10; z++)
					{
						randomPos0 = randomPosition + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

						if(IsRandomPositionFree(randomPos0, Vector3.zero, Vector3.zero))
						{
							break;
						}
						else randomPos0 = Vector3.zero;
					}

					for(x = 0; x < 10; x++)
					{
						if(randomPos0 != Vector3.zero)
						{
							randomPos1 = randomPosition + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

							if(IsRandomPositionFree(randomPos1, randomPos0, Vector3.zero))
							{
								break;
							}
							else 
							{
								randomPos1 = Vector3.zero;
							}
						}
						else randomPos1 = Vector3.zero;

					}

					for(c = 0; c < 10; c++)
					{
						if(randomPos1 != Vector3.zero && randomPos0 != Vector3.zero)
						{
							randomPos2 = randomPosition + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
							if(IsRandomPositionFree(randomPos2, randomPos0, randomPos1))
								{
									break;
								}
							else 
							{
								randomPos2 = Vector3.zero;

							}
						}

						else randomPos2 = Vector3.zero;

					}

					if(randomPos0 != Vector3.zero && randomPos1 != Vector3.zero && randomPos2 != Vector3.zero)
					{
						spawnPoints.Add(randomPos0);
						spawnPoints.Add(randomPos1);
						spawnPoints.Add(randomPos2);
						break;
					}
					else
					{
						randomPos0 = Vector3.zero;
						randomPos1 = Vector3.zero;
						randomPos2 = Vector3.zero;
					}
				}

				if(randomPos0 != Vector3.zero)
				{
                    if (randomPos0 != Vector3.zero)
                    {
                        RaycastHit hit;

                        GameObject obj = null;

                        if (Physics.Raycast(randomPos0 + Vector3.up * highToSpawn, Vector3.down, out hit, Mathf.Infinity))
                        {
                            obj = Instantiate(peoplePrefabs[prefabNum], new Vector3(randomPos0.x, hit.point.y, randomPos0.z), Quaternion.identity) as GameObject;
                            peoplePrefabCount++;
                        }
                        else
                        {
                            continue;
                        }

                        obj.AddComponent<PeopleController>();

                        obj.GetComponent<PeopleController>().SetTarget(randomPosition);

                        obj.GetComponent<PeopleController>().animNames = new string[3] { "talk1", "talk2", "listen" };
                        obj.transform.parent = par.transform;
                    }


                    prefabNum = Random.Range(0, peoplePrefabs.Length);


                    if (randomPos0 != Vector3.zero)
                    {
                        RaycastHit hit1;

                        GameObject obs = null;

                        if (Physics.Raycast(randomPos1 + Vector3.up * highToSpawn, Vector3.down, out hit1, Mathf.Infinity))
                        {
                            obs = Instantiate(peoplePrefabs[prefabNum], new Vector3(randomPos1.x, hit1.point.y, randomPos1.z), Quaternion.identity) as GameObject;
                            peoplePrefabCount++;
                        }
                        else
                        {
                            continue;
                        }

                        obs.AddComponent<PeopleController>();

                        obs.GetComponent<PeopleController>().SetTarget(randomPosition);

                        obs.GetComponent<PeopleController>().animNames = new string[3] { "talk1", "talk2", "listen" };
                        obs.transform.parent = par.transform;
                    }

                    prefabNum = Random.Range(0, peoplePrefabs.Length);

                    if (randomPos0 != Vector3.zero)
                    {
                        RaycastHit hit2;

                        GameObject obl = null;

                        if (Physics.Raycast(randomPos2 + Vector3.up * highToSpawn, Vector3.down, out hit2, Mathf.Infinity))
                        {
                            obl = Instantiate(peoplePrefabs[prefabNum], new Vector3(randomPos2.x, hit2.point.y, randomPos2.z), Quaternion.identity) as GameObject;
                            peoplePrefabCount++;
                        }
                        else
                        {
                            continue;
                        }

                        obl.AddComponent<PeopleController>();

                        obl.GetComponent<PeopleController>().SetTarget(randomPosition);

                        obl.GetComponent<PeopleController>().animNames = new string[3] { "talk1", "talk2", "listen" };
                        obl.transform.parent = par.transform;
                    }
                }
            }
        }
    }

    Vector3 RandomRectanglePosition ()
	{
		Vector3 randomPosition = new Vector3(0, 0, 0);

        for(int i = 0; i < 10; i++)
        {
			randomPosition.x = surface.transform.position.x - GetRealPlaneSize().x / 2 + 0.3f + Random.Range(0.0f, GetRealPlaneSize().x - 0.6f);
			randomPosition.z = surface.transform.position.z - GetRealPlaneSize().y / 2 + 0.3f + Random.Range(0.0f, GetRealPlaneSize().y - 0.6f);
			randomPosition.y = surface.transform.position.y;

        	if(IsRandomPositionFree(randomPosition, Vector3.zero, Vector3.zero))
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

    		if(Vector3.Distance(pos, center) < GetRealPlaneSize().x / 2 - 0.3f && IsRandomPositionFree(pos, Vector3.zero, Vector3.zero))
        		return pos;
        }

        return Vector3.zero;
	}

	bool IsRandomPositionFree (Vector3 pos, Vector3 helpPoint1, Vector3 helpPoint2)
	{
		for(int i = 0; i < spawnPoints.Count; i++)
		{
			if(spawnPoints[i].x - 0.5f < pos.x && spawnPoints[i].x + 0.5f > pos.x && spawnPoints[i].z - 0.5f < pos.z && spawnPoints[i].z + 0.5f > pos.z)
			return false;
		}

		if(helpPoint1 != Vector3.zero)
		{
			if(helpPoint1.x - 0.6f < pos.x && helpPoint1.x + 0.6f > pos.x && helpPoint1.z - 0.6f < pos.z && helpPoint1.z + 0.6f > pos.z)
			{
				return false;
			}
				if(!isCircle)
				{
					if(
					!(helpPoint1.x + 0.3f > surface.transform.position.x - GetRealPlaneSize().x / 2) &&
					!(helpPoint1.x - 0.3f < surface.transform.position.x + GetRealPlaneSize().x / 2) &&
					!(helpPoint1.z + 0.3f > surface.transform.position.z - GetRealPlaneSize().y / 2) &&
					!(helpPoint1.z - 0.3f < surface.transform.position.z + GetRealPlaneSize().y / 2)
					)
						return false;
				}

				else
				{
					if(Vector3.Distance(helpPoint1, surface.transform.position) >= GetRealPlaneSize().x / 2 - 0.3f)
						return false;
				}

		}

		if(helpPoint2 != Vector3.zero)
		{
			if(helpPoint2.x - 0.6f < pos.x && helpPoint2.x + 0.6f > pos.x && helpPoint2.z - 0.6f < pos.z && helpPoint2.z + 0.6f > pos.z)
			{
				return false;
			}

				if(!isCircle)
				{
					if(
					!(helpPoint2.x + 0.3f > surface.transform.position.x - GetRealPlaneSize().x / 2) &&
					!(helpPoint2.x - 0.3f < surface.transform.position.x + GetRealPlaneSize().x / 2) &&
					!(helpPoint2.z + 0.3f > surface.transform.position.z - GetRealPlaneSize().y / 2) &&
					!(helpPoint2.z - 0.3f < surface.transform.position.z + GetRealPlaneSize().y / 2)
					)
						return false;
				}

				else
				{
					if(Vector3.Distance(helpPoint2, surface.transform.position) >= GetRealPlaneSize().x / 2 - 0.3f)
						return false;
				}
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
