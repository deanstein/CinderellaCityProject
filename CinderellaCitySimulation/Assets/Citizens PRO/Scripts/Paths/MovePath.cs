using UnityEngine;
using System.Collections;

[System.Serializable]
public class MovePath : MonoBehaviour {

    [SerializeField]
    public Vector3 startPos;
    [SerializeField]
    public Vector3 finishPos;
    [SerializeField]
    public int w;
    
    [SerializeField]
    public int targetPoint;
    [SerializeField]
    public int targetPointsTotal;

    [SerializeField]
    public string animName;
    [SerializeField]
    public float walkSpeed;
    [SerializeField]
    public float runSpeed;
    [SerializeField]
    public bool loop;
    [SerializeField]
    public bool forward;
    [SerializeField]
    public GameObject walkPath;

    [HideInInspector] public float randXFinish;
    [HideInInspector] public float randZFinish;
    [SerializeField] [Tooltip("Set your animation speed / Установить свою скорость анимации?")] private bool _overrideDefaultAnimationMultiplier;
    [SerializeField] [Tooltip("Speed animation walking / Скорость анимации ходьбы")] private float _customWalkAnimationMultiplier = 1.0f;
    [SerializeField] [Tooltip("Running animation speed / Скорость анимации бега")] private float _customRunAnimationMultiplier = 1.0f;

    public void InitializeAnimation(bool overrideAnimation, float walk, float run)
    {
        _overrideDefaultAnimationMultiplier = overrideAnimation;
        _customWalkAnimationMultiplier = walk;
        _customRunAnimationMultiplier = run;
    }

    public void MyStart(int _w, int _i, string anim, bool _loop, bool _forward, float _walkSpeed, float _runSpeed)
    {
        forward = _forward;
        walkSpeed = _walkSpeed;
        runSpeed = _runSpeed;
        var _WalkPath = walkPath.GetComponent<WalkPath>();
        w = _w;
        targetPointsTotal = _WalkPath.getPointsTotal(0) - 2;

        loop = _loop;
        animName = anim;

        if(loop)
        {
            if(_i < targetPointsTotal && _i > 0)
            {
                if(forward)
                {
                    targetPoint = _i + 1;
                    finishPos = _WalkPath.getNextPoint(w, _i+1);
                }
                else 
                {
                    targetPoint = _i;
                    finishPos = _WalkPath.getNextPoint(w, _i);
                }
            }
            else
            {
                if(forward)
                {
                    targetPoint = 1;
                    finishPos = _WalkPath.getNextPoint(w, 1);
                }
                else 
                {
                    targetPoint = targetPointsTotal;
                    finishPos = _WalkPath.getNextPoint(w, targetPointsTotal);
                }
            }
        }

        else
        {
            if(forward)
            {
                targetPoint = _i + 1;
                finishPos = _WalkPath.getNextPoint(w, _i+1);
            }
            else 
            {
                targetPoint = _i;
                finishPos = _WalkPath.getNextPoint(w, _i);
            }
        }
    }

    public void SetLookPosition()
    {
        Vector3 targetPos = new Vector3(finishPos.x, transform.position.y, finishPos.z);
        transform.LookAt(targetPos);
    }

    void Start()
    {
        Animator animator = GetComponent<Animator>();

        animator.CrossFade(animName, 0.1f, 0, Random.Range(0.0f, 1.0f));
        if (animName == "walk")
        {
            if (_overrideDefaultAnimationMultiplier)
            {
                animator.speed = walkSpeed * _customWalkAnimationMultiplier;
            }
            else
            {
                animator.speed = walkSpeed * 1.2f;
            }
        }
        else if (animName == "run")
        {
            if (_overrideDefaultAnimationMultiplier)
            {
                animator.speed = runSpeed * _customRunAnimationMultiplier;
            }
            else
            {
                animator.speed = runSpeed / 3;
            }
        }
    }
    void Update ()
    {   
        RaycastHit hit;

        if(Physics.Raycast(transform.position + new Vector3(0, 2, 0), -transform.up, out hit))
        {
            finishPos.y = hit.point.y;
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }

        //Vector3 targetPos = new Vector3(finishPos.x, transform.position.y, finishPos.z);

        Vector3 randFinishPos = new Vector3(finishPos.x + randXFinish, finishPos.y, finishPos.z + randZFinish);

        Vector3 targetPos = new Vector3(randFinishPos.x, transform.position.y, randFinishPos.z);

        var _WalkPath = walkPath.GetComponent<WalkPath>();

        var richPointDistance = Vector3.Distance(Vector3.ProjectOnPlane(transform.position, Vector3.up), Vector3.ProjectOnPlane(randFinishPos, Vector3.up));

        if (richPointDistance < 0.2f && animName == "walk" && ((loop) || (!loop && targetPoint > 0 && targetPoint < targetPointsTotal)))
        {

            if(forward)
            {
                if(targetPoint < targetPointsTotal)
                    targetPos = _WalkPath.getNextPoint(w, targetPoint + 1);
                else
                    targetPos = _WalkPath.getNextPoint(w, 0);
                targetPos.y = transform.position.y;
            }

            else
            {
                if(targetPoint > 0)
                    targetPos = _WalkPath.getNextPoint(w, targetPoint - 1);
                else
                    targetPos = _WalkPath.getNextPoint(w, targetPointsTotal);
                targetPos.y = transform.position.y;
            }
        }

        if(richPointDistance < 0.5f && animName == "run" && ((loop) || (!loop && targetPoint > 0 && targetPoint < targetPointsTotal)))
        {

            if(forward)
            {
                if(targetPoint < targetPointsTotal)
                    targetPos = _WalkPath.getNextPoint(w, targetPoint + 1);
                else
                    targetPos = _WalkPath.getNextPoint(w, 0);
                targetPos.y = transform.position.y;
            }

            else
            {
                if(targetPoint > 0)
                    targetPos = _WalkPath.getNextPoint(w, targetPoint - 1);
                else
                    targetPos = _WalkPath.getNextPoint(w, targetPointsTotal);
                targetPos.y = transform.position.y;
            }
        }

        Vector3 targetVector = targetPos - transform.position;

        if(targetVector != Vector3.zero)
        {
            /*Quaternion look = Quaternion.identity;
            if(animName == "walk")
                look = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetVector), Time.deltaTime * 3f * moveSpeed);
            else if(animName == "run")
                look = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetVector), Time.deltaTime * 1.3f* moveSpeed);
            transform.rotation = look;*/

            Vector3 newDir = Vector3.zero;
            newDir = Vector3.RotateTowards(transform.forward, targetVector, 2 * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
        }

        if (richPointDistance > 1)
        {
            if (Time.deltaTime > 0)
            {
                //transform.position += transform.forward * moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, finishPos, Time.deltaTime * 1.0f * (animName == "walk" ? walkSpeed : runSpeed));
            }
        }
        else if (richPointDistance <= 1 && forward){

            if(targetPoint != targetPointsTotal)
            {   
                    targetPoint++;

                finishPos = _WalkPath.getNextPoint(w, targetPoint);
            }
            else if(targetPoint == targetPointsTotal)
            {
                if(loop)
                {
                    finishPos = _WalkPath.getStartPoint(w);

                        targetPoint = 0;
                }

                else
                {
                    _WalkPath.SpawnOnePeople(w, forward, walkSpeed, runSpeed);
                    Destroy(gameObject);
                }
            }

        }
        else if (richPointDistance <= 1 && !forward){

            if(targetPoint > 0)
            {   
                    targetPoint--;

                finishPos = _WalkPath.getNextPoint(w, targetPoint);
            }
            else if(targetPoint == 0)
            {
                if(loop)
                {
                    finishPos = _WalkPath.getNextPoint(w, targetPointsTotal);

                        targetPoint = targetPointsTotal;
                }

                else
                {
                    _WalkPath.SpawnOnePeople(w, forward, walkSpeed, runSpeed);
                    Destroy(gameObject);
                }
            }

        }

    }
}
