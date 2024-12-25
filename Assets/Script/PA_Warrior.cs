using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PA_Warrior : Enemy
{
    public float attackRange = 10f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime;

    public EnemyGunRayCast gun;
    public Transform gunPivot;

    public float maxRotationSpeed = 90f; // 최대 회전 속도 (도/초)
    public float maxAttackAngle = 67.5f; // 공격 가능한 각도 (도)

    // Idle 상태 이면 원래 위치로 돌아가도록
    public Vector3 initial_position;
    public Vector3 initial_rotation;

    private NavMeshAgent agent;
    private Animator animator;

    // 추가 체력
    public float pa_warriorHealth = 20f;
    
    protected override void Start()
    {
        base.Start();
        health += pa_warriorHealth;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        gun.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        gun.gameObject.SetActive(false);
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            isCCTVon = false;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > attackRange)
            {
                ChasePlayer();
            }
            else
            {
                if (Time.time >= nextAttackTime)
                {
                    // 플레이어와의 각도가 67.5도 이내라면 바로 공격
                    float angleToPlayer = Vector3.Angle(transform.forward, player.position - transform.position);
                    if (angleToPlayer <= maxAttackAngle)
                    {
                        AttackPlayer();
                    }
                    else
                    {
                        RotateTowardsPlayer(angleToPlayer);
                    }
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
        else
        {
            if (detectedCCTV != null)
            {
                MoveTo(detectedCCTV.transform.position);
            }
            else
            {
                Idle();
            }
        }

        UpdateAnimations();
    }

    private void RotateTowardsPlayer(float angleToPlayer)
    {
        // 목표와의 각도가 maxAttackAngle을 초과하면 회전 시작
        float rotationSpeed = maxRotationSpeed * Time.deltaTime;
        
        // 회전 방향 계산 (플레이어 위치를 향해 회전)
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Y축 회전만 고려
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

        // 회전
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player_last_position);
        animator.SetBool("HasTargetInrange", false);
    }

    private void AttackPlayer()
    {
        gun.Fire(player_last_position);
        agent.SetDestination(transform.position); 
        animator.SetBool("HasTargetInrange", true);
    }

    private void Idle()
    {
        // Idle 상태로 돌아가기 전에 초기 위치로 이동
        agent.SetDestination(initial_position);
        // animator.SetFloat("position_x", 0);
        // animator.SetFloat("position_y", 0);
        
        // // 초기 위치에 도달한 후 회전 시작
        // if (Vector3.Distance(transform.position, initial_position) < 0.5f)
        // {
        //     // 회전 (EulerAngles로 초기 회전값을 설정)
        //     Quaternion targetRotation = Quaternion.Euler(initial_rotation);
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);

        //     초기 회전값에 도달하면 Idle 상태로 전환
        //     if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
        //     {
        //         animator.SetBool("HasTargetInrange", false);
        //     }
        // }
    }

    private void UpdateAnimations()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
            animator.SetFloat("position_x", localVelocity.x);
            animator.SetFloat("position_y", localVelocity.z);
        }
        else
        {
            animator.SetFloat("position_x", 0);
            animator.SetFloat("position_y", 0);
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        //base.Die();
        animator.SetTrigger("Die");
        agent.isStopped = true;

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        gun.gameObject.SetActive(false);
        StartCoroutine(HandleDeath());

    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    public override void MoveTo(Vector3 targetPosition)
    {
        //base.MoveTo(targetPosition); 
        //Debug.Log("Overrided MoveTo");
        //Debug.Log(targetPosition);
        agent.SetDestination(targetPosition);
        animator.SetBool("HasTargetInrange", false);
        isCCTVon = true;
    }
    private void MoveToDetectedCCTV()
    {
        if (detectedCCTV != null)
        {
            MoveTo(detectedCCTV.transform.position);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 adjustedOrigin = transform.position + transform.forward * 0.3f;

        DrawCone(adjustedOrigin, transform.forward, viewAngle, detectionRange);
    }

    private void DrawCone(Vector3 origin, Vector3 forward, float angle, float range, int segments = 30)
    {
        forward.Normalize();

        Vector3 initialDirection = Quaternion.Euler(0, -angle / 2, 0) * forward;

        // Loop to draw the cone's edges
        for (int i = 0; i < segments; i++)
        {
            Vector3 currentDirection = Quaternion.Euler(0, (angle / segments) * i, 0) * initialDirection;
            Vector3 nextDirection = Quaternion.Euler(0, (angle / segments) * (i + 1), 0) * initialDirection;

            Vector3 currentPoint = origin + currentDirection * range;
            Vector3 nextPoint = origin + nextDirection * range;

            Gizmos.DrawLine(origin, currentPoint);
            Gizmos.DrawLine(currentPoint, nextPoint);
        }
    }
}