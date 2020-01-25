using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayer : MonoBehaviour
{
    public float health = 30;
    public bool patrol = false;
    public float patrolSpeed = 1f;
    public List<Vector3> patrolPoints = new List<Vector3>();

    private int currPos = 0;
    private int nextPos;

    private void Start()
    {
        currPos = 0;
        if (patrol)
        {
            transform.position = patrolPoints[currPos];
            nextPos = currPos+1;
        }
    }

    public void DealDamage(float val=10)
    {
        health -= val;
        if (health <= 0) Destroy(this.gameObject);
    }

    private void Update()
    {
        if(patrol)
        {
            transform.Translate((patrolPoints[nextPos]- transform.position).normalized * patrolSpeed * Time.deltaTime);
            if ((transform.position - patrolPoints[nextPos]).sqrMagnitude < 0.1f)
            {
                currPos = nextPos;
                nextPos++;
                if (nextPos >= patrolPoints.Count)
                    nextPos = 0;    
            }
        }
    }
}
