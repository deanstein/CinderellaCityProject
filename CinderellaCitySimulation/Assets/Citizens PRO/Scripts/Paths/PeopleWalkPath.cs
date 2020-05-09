using System;
using UnityEngine;
using System.Collections.Generic;

public class PeopleWalkPath : WalkPath
{
    public enum EnumMove { Walk, Run };
    public enum EnumDir { Forward, Backward, HugLeft, HugRight, WeaveLeft, WeaveRight };

    [HideInInspector] [Tooltip("Type of movement / Тип движения")] [SerializeField] private EnumMove _moveType;
    [Tooltip("Direction of movement / Направление движения. Левостороннее, правостороннее, итд.")] [SerializeField] private EnumDir direction;
    [HideInInspector] [Tooltip("Speed of walk / Скорость ходьбы")] [SerializeField] private float walkSpeed = 1;
    [HideInInspector] [Tooltip("Speed of run / Скорость бега")] [SerializeField] private float runSpeed = 4; 

    [HideInInspector] public bool isWalk;

    [HideInInspector] [SerializeField] [Tooltip("Set your animation speed? / Установить свою скорость анимации?")] private bool _overrideDefaultAnimationMultiplier = true;
    [HideInInspector] [SerializeField] [Tooltip("Speed animation of walking / Скорость анимации ходьбы")] private float _customWalkAnimationMultiplier = 1.1f;
    [HideInInspector] [SerializeField] [Tooltip("Running animation speed / Скорость анимации бега")] private float _customRunAnimationMultiplier = 0.3f;

    public override void DrawCurved(bool withDraw)
    {
        if (numberOfWays< 1) numberOfWays = 1;
        if (lineSpacing< 0.6f) lineSpacing = 0.6f;
        _forward = new bool[numberOfWays];

        isWalk = (_moveType.ToString() == "Walk") ? true : false;

        for (int w = 0; w<numberOfWays; w++)
        {

            if (direction.ToString() == "Forward")
            {
                _forward[w] = true;
            }

            else if (direction.ToString() == "Backward")
            {
                _forward[w] = false;
            }

            else if (direction.ToString() == "HugLeft")
            {
                if ((w + 2) % 2 == 0)
                    _forward[w] = true;
                else
                    _forward[w] = false;
            }

            else if (direction.ToString() == "HugRight")
            {
                if ((w + 2) % 2 == 0)
                    _forward[w] = false;
                else
                    _forward[w] = true;
            }

            else if (direction.ToString() == "WeaveLeft")
            {
                if (w == 1 || w == 2 || (w - 1) % 4 == 0 || (w - 2) % 4 == 0)
                    _forward[w] = false;
                else _forward[w] = true;
            }

            else if (direction.ToString() == "WeaveRight")
            {
                if (w == 1 || w == 2 || (w - 1) % 4 == 0 || (w - 2) % 4 == 0)
                    _forward[w] = true;
                else _forward[w] = false;
            }

        }


        if (pathPoint.Count< 2) return;
        points = new Vector3[numberOfWays, pathPoint.Count + 2];

        pointLength[0] = pathPoint.Count + 2;

        for (int i = 0; i<pathPointTransform.Count; i++)
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

            Vector3 vectorShift = Vector3.Normalize((Quaternion.Euler(0, 90, 0) * (vectorStart + vectorEnd)));

            points[0, i + 1] = numberOfWays % 2 == 1 ? pathPointTransform[i].transform.position : pathPointTransform[i].transform.position + vectorShift* lineSpacing / 2;
            if (numberOfWays > 1) points[1, i + 1] = points[0, i + 1] - vectorShift* lineSpacing;

            for (int w = 1; w<numberOfWays; w++)
            {
                points[w, i + 1] = points[0, i + 1] + vectorShift* lineSpacing * (float) (Math.Pow(-1, w)) * ((w + 1) / 2);
            }
        }
        for (int w = 0; w<numberOfWays; w++)
        {
            points[w, 0] = points[w, 1];
            points[w, pointLength[0] - 1] = points[w, pointLength[0] - 2];
        }
        if (withDraw)
        {
            for (int w = 0; w<numberOfWays; w++)
            {
                if (loopPath)
                {
                    Gizmos.color = (_forward[w] ? Color.green : Color.red);
                    Gizmos.DrawLine(points[w, 0], points[w, pathPoint.Count]);
                }
                for (int i = 1; i<pathPoint.Count; i++)
                {
                    Gizmos.color = (_forward[w] ? Color.green : Color.red);
                    Gizmos.DrawLine(points[w, i + 1], points[w, i]);
                }
            }
        }
    }
    public override void SpawnOnePeople(int w, bool forward, float walkSpeed, float runSpeed)
    {
        int prefabNum = UnityEngine.Random.Range(0, peoplePrefabs.Length);
        var people = gameObject;

        if (!forward)
            people = Instantiate(peoplePrefabs[prefabNum], points[w, pointLength[0] - 2], Quaternion.identity) as GameObject;
        else
            people = Instantiate(peoplePrefabs[prefabNum], points[w, 1], Quaternion.identity) as GameObject;
        var _movePath = people.AddComponent<MovePath>();

        _movePath.randXFinish = UnityEngine.Random.Range(-randXPos, randXPos);
        _movePath.randZFinish = UnityEngine.Random.Range(-randZPos, randZPos);

        people.transform.parent = par.transform;
        _movePath.walkPath = gameObject;

        string animName;
        if (isWalk)
            animName = "walk";
        else
            animName = "run";

        _movePath.InitializeAnimation(_overrideDefaultAnimationMultiplier, _customWalkAnimationMultiplier, _customRunAnimationMultiplier);

        if (!forward)
        {
            _movePath.MyStart(w, pointLength[0] - 2, animName, loopPath, forward, walkSpeed, runSpeed);
            people.transform.LookAt(points[w, pointLength[0] - 3]);
        }
        else
        {
            _movePath.MyStart(w, 1, animName, loopPath, forward, walkSpeed, runSpeed);
            people.transform.LookAt(points[w, 2]);
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
            par.name = "people";
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

            bool forward = false;

            switch (direction.ToString())
            {
                case "Forward":
                    forward = true;
                    break;
                case "Backward":
                    forward = false;
                    break;
                case "HugLeft":
                    forward = (wayIndex + 2) % 2 == 0;
                    break;
                case "HugRight":
                    forward = (wayIndex + 2) % 2 != 0;
                    break;
                case "WeaveLeft":
                    forward = wayIndex != 1 && wayIndex != 2 && (wayIndex - 1) % 4 != 0 && (wayIndex - 2) % 4 != 0;
                    break;
                case "WeaveRight":
                    forward = wayIndex == 1 || wayIndex == 2 || (wayIndex - 1) % 4 == 0 || (wayIndex - 2) % 4 == 0;
                    break;
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
                var randomShift = UnityEngine.Random.Range(-segmentLen / 3f, segmentLen / 3f) + (wayIndex * segmentLen);
                var finalRandomDistance = (peopleIndex + 1) * segmentLen + randomShift;

                Vector3 routePosition = GetRoutePosition(pointArray, finalRandomDistance, pCount, loopPath);

                float XPos = UnityEngine.Random.Range(-randXPos, randXPos);
                float ZPos = UnityEngine.Random.Range(-randZPos, randZPos);

                routePosition = new Vector3(routePosition.x + XPos, routePosition.y, routePosition.z + ZPos);
                Vector3 or;

                RaycastHit[] rrr = Physics.RaycastAll(or = new Vector3(routePosition.x, routePosition.y + 10000, routePosition.z), Vector3.down, Mathf.Infinity);

                float dist = 0;
                int bestCandidate = 0;

                rrr = Physics.RaycastAll(or = new Vector3(routePosition.x, routePosition.y + 10000, routePosition.z), Vector3.down, Mathf.Infinity);

                for (int i = 0; i < rrr.Length; i++)
                {
                    if (dist < Vector3.Distance(rrr[0].point, or))
                    {
                        bestCandidate = i;
                        dist = Vector3.Distance(rrr[0].point, or);
                    }
                }

                if (rrr.Length > 0)
                {
                    routePosition.y = rrr[bestCandidate].point.y;
                }

                people = Instantiate(peoplePrefabs[pickList[peopleIndex]], routePosition, Quaternion.identity) as GameObject;

                var movePath = people.AddComponent<MovePath>();

                movePath.randXFinish = XPos;
                movePath.randZFinish = ZPos;

                people.transform.parent = par.transform;
                movePath.walkPath = gameObject;

                string animName;
                if (isWalk)
                    animName = "walk";
                else
                    animName = "run";

                movePath.MyStart(wayIndex, GetRoutePoint((peopleIndex + 1) * segmentLen + randomShift, wayIndex, pCount, forward, loopPath), animName, loopPath, forward, walkSpeed, runSpeed);
                movePath.InitializeAnimation(_overrideDefaultAnimationMultiplier, _customWalkAnimationMultiplier, _customRunAnimationMultiplier);
                movePath.SetLookPosition();
            }
        }
    }
}