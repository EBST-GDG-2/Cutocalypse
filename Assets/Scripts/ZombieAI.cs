using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public Animator zombieAsset;
    public HealthBar healthBar;
    public int maxHeatlh = 4;
    public int currentHealth;
    public GameObject attackArea, canvas;
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    public float timeBetweenAttacks;
    bool alreadyAttacked;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRan;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        agent = gameObject.GetComponent<NavMeshAgent>();
        currentHealth = maxHeatlh;
        healthBar.SetMaxHealth(maxHeatlh);
    }

    private void Update()
    {
        canvas.transform.LookAt(Camera.main.transform.position);
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
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
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

        if (distanceToWalkPoint.magnitude < 1f)
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
        Vector3 p1 = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(p1);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            StartCoroutine(ResetAttack(timeBetweenAttacks));
        }
    }
    IEnumerator Die()
    {
        zombieAsset.SetTrigger("Dead");
        agent.SetDestination(transform.position);
        Destroy(attackArea);
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
    IEnumerator ResetAttack(float second)
    {
        attackArea.SetActive(true);
        zombieAsset.SetTrigger("Attack");
        yield return new WaitForSeconds(second);
        attackArea.SetActive(false);
        alreadyAttacked = false;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            TakeDamage(2);
        }
    }
}
