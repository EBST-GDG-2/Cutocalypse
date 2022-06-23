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
    [SerializeField] private GameObject coin;

    public Vector3 walkPoint;
    public bool alive;
    bool walkPointSet;
    public float walkPointRange;
    private int Puan;
    bool alreadyAttacked;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRan;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        coin = GameObject.Find("Coin");
        agent = gameObject.GetComponent<NavMeshAgent>();
        currentHealth = maxHeatlh;
        healthBar.SetMaxHealth(maxHeatlh);
        alive = true;
    }

    private void Update()
    {
        canvas.transform.LookAt(Camera.main.transform.position);
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRan = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRan && alive)
        {
            Patroling();
        }
        if (playerInSightRange && !playerInAttackRan && alive)
        {
            ChasePlayer();
        }
        if (playerInSightRange && playerInAttackRan && alive)
        {
            AttackPlayer();
        }
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
        if (!zombieAsset.GetCurrentAnimatorStateInfo(0).IsName("zombie_attack"))
        {
            attackArea.SetActive(false);
            alreadyAttacked = false;
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
            attackArea.SetActive(true);
            zombieAsset.SetTrigger("Attack");
        }
    }
    IEnumerator Die()
    {
        alive = false;
        gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
        zombieAsset.SetTrigger("Dead");
        agent.SetDestination(transform.position);
        playerInAttackRan = false;
        attackArea.SetActive(false);
        canvas.SetActive(false);
        yield return new WaitForSeconds(2);
        Instantiate(coin, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);
        
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
