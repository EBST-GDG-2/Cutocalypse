using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public Transform firePoint;
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;
    
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRan;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRan = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRan)
        {
            Patroling();
        }
        if (playerInSightRange && !playerInAttackRan)
        {
            ChasePlayer();
        }
        if (playerInSightRange && playerInAttackRan)
        {
            AttackPlayer();
        }
    }

    private void Patroling()
    {
        if (!walkPointSet)
        {
            SearchWalkPonit();
        }
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }
        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        
        if(distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPonit()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        Rigidbody rb = Instantiate(projectile, firePoint.position, firePoint.rotation).GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * 5f, ForceMode.Impulse);
        alreadyAttacked = false;
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Invoke(nameof(DestroyEnemy), .5f);
        }
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
