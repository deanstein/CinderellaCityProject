using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

[System.Serializable]
public class WalkPath : MonoBehaviour
{
    [Tooltip("Objects of motion / Объекты движения")] public GameObject[] peoplePrefabs;

    [Tooltip("Number of paths / Количество путей")] public int numberOfWays;
    [Tooltip("Space between paths / Пространство между путями")] public float lineSpacing;
    [Tooltip("Density of movement of objects / Плотность движения объектов")] [Range(0.01f, 0.50f)] public float Density = 0.2f;
    [Tooltip("Distance between objects / Дистанция между объектами")][Range(1f, 10f)] public float _minimalObjectLength = 1f;
    [Tooltip("Make the path closed in the ring / Сделать путь замкнутым в кольцо")] public bool loopPath;

    protected float[] _distances;

    [HideInInspector] public List<Vector3> pathPoint = new List<Vector3>();
    [HideInInspector] public List<GameObject> pathPointTransform = new List<GameObject>();
    [HideInInspector] public Vector3[,] points;
    [HideInInspector] public List<Vector3> CalcPoint = new List<Vector3>();
    [HideInInspector] public int[] pointLength = new int[10];
    [HideInInspector] public bool disableLineDraw = false;
    [HideInInspector] public bool[] _forward;
    [HideInInspector] public GameObject par;
    [HideInInspector] public PathType pathType;

    /// <summary>
    /// Радиус сферы-стёрки [м]
    /// </summary>
    [Tooltip("Radius of the sphere-scraper [m] / Радиус сферы-стёрки [м]"), Range(0.1f, 25)]
    public float eraseRadius = 2f;

    /// <summary>
    /// Минимальное расстояние от курсора до линии при котором можно добавить новую точку в путь [м]
    /// </summary>
	[Tooltip("The minimum distance from the cursor to the line at which you can add a new point to the path [m] / Минимальное расстояние от курсора до линии, при котором можно добавить новую точку в путь [м]")] [Range(0.5f, 10)] public float addPointDistance = 2f;

    [Tooltip("Adjust the spawn of cars to the nearest surface. This option will be useful if there are bridges in the scene / Регулировка спавна людей к ближайшей поверхности. Этот параметор будет полезен, если в сцене есть мосты.")] public float highToSpawn = 1.0f;

    [Range(0.0f, 5.0f)] [Tooltip("Offset from the line along the X axis / Смещение от линии по оси X")] public float randXPos = 0.1f;
    [Range(0.0f, 5.0f)] [Tooltip("Offset from the line along the Z axis / Смещение от линии по оси Z")] public float randZPos = 0.1f;

    #region Create And Delete Additional Points

    /// <summary>
    /// Идёт ли процесс создания новой точки
    /// </summary>
    [HideInInspector] public bool newPointCreation = false;

    /// <summary>
    /// Идёт ли процесс удаления некоторой старой точки
    /// </summary>
    [HideInInspector] public bool oldPointDeleting = false;

    /// <summary>
    /// Позиция мышки на экране 
    /// </summary>
    [HideInInspector] public Vector3 mousePosition = Vector3.zero;

    /// <summary>
    /// Индекс точки из массива которую хотят удалить
    /// </summary>
    private int deletePointIndex = -1;

    // точки между которыми будет создаваться дополнительная
    /// <summary>
    /// Индекс первой точки в массиве всех точек
    /// </summary>
    private int firstPointIndex = -1;

    /// <summary>
    /// Индекс второй точки в массиве всех точек
    /// </summary>
    private int secondPointIndex = -1;

    #endregion

    public Vector3 getNextPoint(int w, int index)
    {
        return points[w, index];
    }

    public Vector3 getStartPoint(int w)
    {

        return points[w, 1];

    }

    public int getPointsTotal(int w)
    {

        return pointLength[w];

    }


    void Awake()
    {
        DrawCurved(false);
    }

    public virtual void SpawnOnePeople(int w, bool forward, float walkSpeed, float runSpeed) { }
    public virtual void SpawnPeople() { }
    public virtual void DrawCurved(bool withDraw) { }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (!disableLineDraw)
        {
            DrawCurved(true);
        }

        DrawNewPointCreation();
        DrawOldPointDeleting();
    }

    public void HideExistingIcons()
    {
        Transform t = transform.Find("points");

        foreach (Transform item in t)
        {
            DrawIcon(item.gameObject, 0, true);
        }
    }

    public void ShowExistingIcons()
    {
        Transform t = transform.Find("points");

        foreach (Transform item in t)
        {
            DrawIcon(item.gameObject, 1, false);
        }
    }

    private void DrawIcon(GameObject gameObject, int idx, bool basic)
    {
        GUIContent icon;

        if (!basic)
        {
            var largeIcons = GetTextures("sv_label_", string.Empty, 0, 8);
            icon = largeIcons[idx];
        }
        else
        {
            icon = EditorGUIUtility.IconContent("sv_icon_none");
        }

        var egu = typeof(EditorGUIUtility);
        var flags = System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        var args = new object[] { gameObject, icon.image };
        var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);

        if (basic)
        {
            setIcon.Invoke(null, new object[] { gameObject, null });
            return;
        }

        setIcon.Invoke(null, args);
    }

    private GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
    {
        GUIContent[] array = new GUIContent[count];

        for (int i = 0; i < count; i++)
        {
            array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
        }

        return array;
    }


    /// <summary>
    /// Блокировка разблокировка эдитора
    /// </summary>
    /// <param name="lockValue">true - залочен, false - разлочен</param>
    public void EditorLock(bool lockValue)
    {
        ActiveEditorTracker.sharedTracker.isLocked = lockValue;
    }

    /// <summary>
    /// Рисует всё что связанно с добавлением новой точки в массив
    /// </summary>
    public void DrawNewPointCreation()
    {
        if (!newPointCreation)
        {
            return;
        }

        Selection.activeGameObject = gameObject;

        bool collizion = false;
        for (int i = 0; i < pathPoint.Count - 1; i++)
        {
            if (PointWithLineCollision(pathPointTransform[i].transform.position,
                pathPointTransform[i + 1].transform.position, mousePosition))
            {
                collizion = true;
                firstPointIndex = i;
                secondPointIndex = i + 1;
            }
        }

        if (collizion)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
            firstPointIndex = -1;
            secondPointIndex = -1;
        }

        Gizmos.DrawSphere(mousePosition, addPointDistance);
    }

    /// <summary>
    /// Рисует всё что связано с удалением старой точки из массива
    /// </summary>
    public void DrawOldPointDeleting()
    {
        if (!oldPointDeleting)
        {
            return;
        }

        Selection.activeGameObject = gameObject;

        bool collizion = false;
        for (int i = 0; i < pathPoint.Count; i++)
        {
            if (PointWithSphereCollision(mousePosition, pathPointTransform[i].transform.position))
            {
                collizion = true;
                deletePointIndex = i;
            }
        }

        if (collizion)
        {
            Gizmos.color = Color.magenta;
        }
        else
        {
            Gizmos.color = Color.cyan;
            deletePointIndex = -1;
        }

        Gizmos.DrawSphere(mousePosition, eraseRadius);
    }

#endif

    protected Vector3 GetRoutePosition(Vector3[] pointArray, float distance, int pointCount, bool loopPath)
    {
        int point = 0;
        float length = _distances[_distances.Length - 1];
        distance = Mathf.Repeat(distance, length);

        while (_distances[point] < distance)
        {
            ++point;
        }

        var point1N = ((point - 1) + pointCount) % pointCount;
        var point2N = point;

        var i = Mathf.InverseLerp(_distances[point1N], _distances[point2N], distance);
        return Vector3.Lerp(pointArray[point1N], pointArray[point2N], i);
    }

    protected int GetRoutePoint(float distance, int wayIndex, int pointCount, bool forward, bool loopPath)
    {
        int point = 0;
        float length = _distances[_distances.Length - 1];
        distance = Mathf.Repeat(distance, length);

        while (_distances[point] < distance)
        {
            ++point;
        }

        return point;
    }

    /// <summary>
    /// Проверка на столкновение сферы для стирания точек с точкой
    /// </summary>
    /// <param name="colisionSpherePosition">позиция сферы</param>
    /// <param name="pointPosition">позиция точки</param>
    private bool PointWithSphereCollision(Vector3 colisionSpherePosition, Vector3 pointPosition)
    {
        return Vector3.Magnitude(colisionSpherePosition - pointPosition) < eraseRadius;
    }

    /// <summary>
    /// Проверка на столкновение точки и линии
    /// </summary>
    /// <param name="pointPosition">Координаты новой точки которую планируется создать</param>
    /// <returns>True - есть столкновение, False - нет</returns>
    private bool PointWithLineCollision(Vector3 lineStartPosition, Vector3 lineEndPosition, Vector3 pointPosition)
    {
        return Distance(lineStartPosition, lineEndPosition, pointPosition) < addPointDistance;
    }

    /// <summary>
    /// Возвращает минимальное расстояние от точки до прямой [м]
    /// </summary>
    /// <param name="lineStartPosition">Координата начала прямой</param>
    /// <param name="lineEndPosition">Координата конца прямой</param>
    /// <param name="pointPosition">Координата точки</param>
    private float Distance(Vector3 lineStartPosition, Vector3 lineEndPosition, Vector3 pointPosition)
    {
        // квадрат длинны линии с началом в точке lineStartPosition и концом в точке lineEndPosition
        float l2 = Vector3.SqrMagnitude(lineEndPosition - lineStartPosition);

        if (l2 == 0f)
            return Vector3.Distance(pointPosition, lineStartPosition);
        float t = Mathf.Max(0,
            Mathf.Min(1, Vector3.Dot(pointPosition - lineStartPosition, lineEndPosition - lineStartPosition) / l2));
        Vector3 projection = lineStartPosition + t * (lineEndPosition - lineStartPosition);
        return Vector3.Distance(pointPosition, projection);
    }

    /// <summary>
    /// Добавляет новую точку между двумя созданными до этого
    /// </summary>
    public void AddPoint()
    {
        // Если индексы точек между которыми нужно создавать точку не выбраны
        // точка не создаётся
        if (firstPointIndex == -1 && secondPointIndex == firstPointIndex)
        {
            return;
        }

        var prefab = GameObject.Find("Population System").GetComponent<PopulationSystemManager>().pointPrefab;
        var obj = Instantiate(prefab, mousePosition, Quaternion.identity) as GameObject;
        obj.name = "p+";
        obj.transform.parent = pathPointTransform[firstPointIndex].transform.parent;
#if UNITY_EDITOR
        //if (dontDrawYoJunkFool)
        //    DrawIcon(obj, 0, true);
#endif
        pathPointTransform.Insert(firstPointIndex + 1, obj);
        pathPoint.Insert(firstPointIndex + 1, obj.transform.position);
    }

    /// <summary>
    /// Удаляет выбранную точку
    /// </summary>
    public void DeletePoint()
    {
        // Если индекс точек для удаления не выбран
        // точка не удаляется
        if (deletePointIndex == -1)
        {
            return;
        }

        DestroyImmediate(pathPointTransform[deletePointIndex]);

        pathPointTransform.RemoveAt(deletePointIndex);
        pathPoint.RemoveAt(deletePointIndex);
    }
}
