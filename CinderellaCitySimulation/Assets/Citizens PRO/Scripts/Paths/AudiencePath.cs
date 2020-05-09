using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudiencePath : WalkPath
{
    public enum Angle
    {
        zero,
        minus90,
        plus90
    };

    [Tooltip("Type of rotation / Вариант поворота")] [SerializeField] private Angle angle = Angle.plus90;

    [Range(-180, 180)]
    [Tooltip("Rotation of people / Поворот человека")] [SerializeField] private float peopleRotation;
    [Tooltip("Look for target / Слежение за таргетом")] [HideInInspector] [SerializeField] private bool looking;
    [Tooltip("Target / Цель")] [HideInInspector] [SerializeField] private Transform target;
    [Tooltip("Speed rotation (smooth) / Скорость поворота (смягчение)")] [HideInInspector] [SerializeField] private float damping = 5;

    public override void DrawCurved(bool withDraw)
    {
        if (numberOfWays < 1) numberOfWays = 1;
        if (lineSpacing < 0.6f) lineSpacing = 0.6f;
        _forward = new bool[numberOfWays];

        for (int w = 0; w < numberOfWays; w++)
        {
            _forward[w] = true;
        }

        if (pathPoint.Count < 2) return;
        points = new Vector3[numberOfWays, pathPoint.Count + 2];

        pointLength[0] = pathPoint.Count + 2;

        for (int i = 0; i < pathPointTransform.Count; i++)
        {
            Vector3 vectorStart;
            Vector3 vectorEnd;
            if (i == 0)
            {
                if (loopPath)
                {
                    vectorStart = pathPointTransform[pathPointTransform.Count - 1].transform.position - pathPointTransform[i].transform.position;
                }
                else
                {
                    vectorStart = Vector3.zero;
                }
                vectorEnd = pathPointTransform[i].transform.position - pathPointTransform[i + 1].transform.position;
            }
            else if (i == pathPointTransform.Count - 1)
            {
                vectorStart = pathPointTransform[i - 1].transform.position - pathPointTransform[i].transform.position;
                if (loopPath)
                {
                    vectorEnd = pathPointTransform[i].transform.position - pathPointTransform[0].transform.position;
                }
                else
                {
                    vectorEnd = Vector3.zero;
                }
            }
            else
            {
                vectorStart = pathPointTransform[i - 1].transform.position - pathPointTransform[i].transform.position;
                vectorEnd = pathPointTransform[i].transform.position - pathPointTransform[i + 1].transform.position;
            }
            //
            Vector3 vectorShift = Vector3.Normalize((Quaternion.Euler(0, 90, 0) * (vectorStart + vectorEnd)));
            //
            points[0, i + 1] = numberOfWays % 2 == 1 ? pathPointTransform[i].transform.position : pathPointTransform[i].transform.position + vectorShift * lineSpacing / 2;
            if (numberOfWays > 1) points[1, i + 1] = points[0, i + 1] - vectorShift * lineSpacing;

            for (int w = 1; w < numberOfWays; w++)
            {
                points[w, i + 1] = points[0, i + 1] + vectorShift * lineSpacing * (float)(System.Math.Pow(-1, w)) * ((w + 1) / 2);
            }
        }
        for (int w = 0; w < numberOfWays; w++)
        {
            points[w, 0] = points[w, 1];
            points[w, pointLength[0] - 1] = points[w, pointLength[0] - 2];
        }
        if (withDraw)
        {
            for (int w = 0; w < numberOfWays; w++)
            {
                if (loopPath)
                {
                    Gizmos.color = (_forward[w] ? Color.green : Color.red);
                    Gizmos.DrawLine(points[w, 0], points[w, pathPoint.Count]);
                }
                for (int i = 1; i < pathPoint.Count; i++)
                {
                    Gizmos.color = (_forward[w] ? Color.green : Color.red);
                    Gizmos.DrawLine(points[w, i + 1], points[w, i]);
                }
            }
        }
    }

    public override void SpawnPeople()
    {
        List<GameObject> pfb = new List<GameObject>(peoplePrefabs);

        for (int i = pfb.Count - 1; i >= 0; i--)
        {
            if (pfb[i] == null)
            {
                pfb.RemoveAt(i);
            }
        }

        peoplePrefabs = pfb.ToArray();

        if (points == null) DrawCurved(false);

        if (par == null)
        {
            par = new GameObject();
            par.transform.parent = gameObject.transform;
            par.name = "walkingObjects";
        }

        int pathPointCount;

        if (!loopPath)
        {
            pathPointCount = pointLength[0] - 2;
        }
        else
        {
            pathPointCount = pointLength[0] - 1;
        }

        if (pathPointCount < 2) return;

        var pCount = loopPath ? pointLength[0] - 1 : pointLength[0] - 2;

        for (int wayIndex = 0; wayIndex < numberOfWays; wayIndex++)
        {
            _distances = new float[pCount];

            float pathLength = 0f;

            for (int i = 1; i < pCount; i++)
            {
                Vector3 vector;
                if (loopPath && i == pCount - 1)
                {
                    vector = points[wayIndex, 1] - points[wayIndex, pCount];
                }
                else
                {
                    vector = points[wayIndex, i + 1] - points[wayIndex, i];
                }

                pathLength += vector.magnitude;
                _distances[i] = pathLength;
            }

            int peopleCount = Mathf.FloorToInt((Density * pathLength) / _minimalObjectLength);
            float segmentLen = _minimalObjectLength + (pathLength - (peopleCount * _minimalObjectLength)) / peopleCount;

            int[] pickList = CommonUtils.GetRandomPrefabIndexes(peopleCount, ref peoplePrefabs);

            Vector3[] pointArray = new Vector3[_distances.Length];

            for (int i = 1; i < _distances.Length; i++)
            {
                pointArray[i - 1] = points[wayIndex, i];
            }

            pointArray[_distances.Length - 1] = loopPath ? points[wayIndex, 1] : points[wayIndex, _distances.Length];

            for (int peopleIndex = 0; peopleIndex < peopleCount; peopleIndex++)
            {
                var people = gameObject;
                var randomShift = Random.Range(-segmentLen / 3f, segmentLen / 3f) + (wayIndex * segmentLen);
                var finalRandomDistance = (peopleIndex + 1) * segmentLen + randomShift;

                Vector3 routePosition = GetRoutePosition(pointArray, finalRandomDistance, pCount, loopPath);

                routePosition = new Vector3(routePosition.x, routePosition.y, routePosition.z);
                Vector3 or;

                RaycastHit hit;

                if (Physics.Raycast(new Vector3(routePosition.x, routePosition.y + highToSpawn, routePosition.z), Vector3.down, out hit, Mathf.Infinity))
                {
                    routePosition.y = hit.point.y;
                }
                else
                {
                    continue;
                }

                people = Instantiate(peoplePrefabs[pickList[peopleIndex]], routePosition, Quaternion.identity) as GameObject;

                people.transform.parent = par.transform;

                PeopleController pc = people.AddComponent<PeopleController>();
                pc.animNames = new string[4] { "idle1", "idle2", "cheer", "claphands" };

                if (looking)
                {
                    pc.target = target;
                    pc.damping = damping;
                }

                var movePath = people.AddComponent<MovePath>();

                movePath.walkPath = gameObject;

                movePath.MyStart(wayIndex, GetRoutePoint((peopleIndex + 1) * segmentLen + randomShift, wayIndex, pCount, true, loopPath), "", loopPath, true, 0, 0);

                Vector3 targetPos = new Vector3(movePath.finishPos.x, people.transform.position.y, movePath.finishPos.z);
                DestroyImmediate(movePath);
                people.transform.LookAt(targetPos);

                if (angle == Angle.zero)
                {
                    people.transform.eulerAngles = new Vector3(people.transform.eulerAngles.x, people.transform.eulerAngles.y + peopleRotation, people.transform.eulerAngles.z);
                }
                else
                {
                    people.transform.eulerAngles = new Vector3(people.transform.eulerAngles.x, people.transform.eulerAngles.y + ((angle == Angle.plus90) ? 90 : -90) + peopleRotation, people.transform.eulerAngles.z);
                }

                people.transform.position += people.transform.forward * Random.Range(-randZPos, randZPos);
                people.transform.position += people.transform.right * Random.Range(-randXPos, randXPos);

                if(Physics.Raycast(new Vector3(people.transform.position.x, people.transform.position.y + highToSpawn, people.transform.position.z), Vector3.down, out hit, Mathf.Infinity))
                {
                    people.transform.position = new Vector3(people.transform.position.x, hit.point.y, people.transform.position.z);
                }
            }
        }
    }
}