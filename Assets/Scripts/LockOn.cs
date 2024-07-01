using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    public float lockonradius;
    public LayerMask targetlayer;
    public float minViewAngle = -70;
    public float maxViewAngle = 70;
    public List<Enemy> targetEnemy;
    public Transform lockonimage;
    public float cameraDirectionY;
    public float cameraDirectionSmoothTime;

    public Enemy currentTarget;
    public bool isLockOn = false;
    Camera maincam;
    Vector3 currentTargetPosition;

    // Start is called before the first frame update
    void Awake()
    {
        maincam = Camera.main;
        lockonimage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isLockOn)
        {
            if (isTargetRange())
            {
                LookAtTarget();
            }
            else
            {
                ResetTarget();
            }
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            if (isLockOn)
            {
                ResetTarget();
                currentTarget = null;
            }
            else
            {
                SearchingLockOnTarget();
            }
        }
    }
    void LookAtTarget()
    {
        if (currentTarget == null)
        {
            ResetTarget();
            return;
        }
        currentTargetPosition = currentTarget.targetPos.transform.position;
        lockonimage.position = Camera.main.WorldToScreenPoint(currentTargetPosition + Vector3.up*currentTarget.lcokonimageoffset);
    }
    void SearchingLockOnTarget()
    {
        Collider[] Searchtargets = Physics.OverlapSphere(transform.position, lockonradius, targetlayer); // Àû °Ë»ö
        if (Searchtargets.Length <= 0) { return; } 
        for (int i = 0; i < Searchtargets.Length; i++)
        {
            Enemy target = Searchtargets[i].GetComponent<Enemy>();
            if (target != null)
            {
                Vector3 targetDir = target.transform.position - transform.position;
                float viewAngle = Vector3.Angle(targetDir,maincam.transform.forward);
                if (viewAngle > minViewAngle && viewAngle < maxViewAngle)
                {
                    RaycastHit hit;
                    if (Physics.Linecast(transform.position, target.targetPos.transform.position, out hit, targetlayer))
                    {
                        targetEnemy.Add(target);
                    }
                }
                else
                {
                    ResetTarget();
                }
            }
        }
        float shotDistance = Mathf.Infinity;
        for (int i = 0; i < targetEnemy.Count; i++)
        {
            if (targetEnemy[i] != null)
            {
                float distancefromtaget = Vector3.Distance(transform.position, targetEnemy[i].transform.position);
                if (distancefromtaget < shotDistance)
                {
                    shotDistance = distancefromtaget;
                    currentTarget = targetEnemy[i];
                }
            }
            else
            {
                ResetTarget();
            }
        }
        if (currentTarget != null)
        {
            isLockOn = true;
            lockonimage.gameObject.SetActive(true);
        }
    }
    void ResetTarget()
    {
        currentTarget = null;
        isLockOn = false;
        targetEnemy.Clear();
        lockonimage.gameObject.SetActive(false);
    }
    bool isTargetRange()
    {
        float distance = (transform.position - currentTargetPosition).magnitude;
        return !(distance > lockonradius);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, lockonradius);
    }
}
