using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public VariableJoystick left,right;
    public GameObject canv;
    public Image curs;
    private TextMeshProUGUI tmpGUI;
    public HealthBar healthBar;
    [SerializeField] Animator animator;
    private Vector3 hareket;
    RaycastHit hit;
    [SerializeField] private float hareketHizi, mermiHizi;
    //GameObject kamera;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float moveSpeed;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] float jumpingForce;
    float yGravity;
    int jumpCount = 0;
    private Vector3 direction;
    private bool isDashed = false;
    private bool atesetti = false;
    public int maxHeatlh = 100;
    public int currentHealth;
    private void Awake()
    {
        currentHealth = maxHeatlh;
        healthBar.SetMaxHealth(maxHeatlh);
        tmpGUI = GameObject.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        //kamera = GameObject.FindWithTag("MainCamera");
    }
    void Start()
    {

        //Cursor.visible = false;
        
        animator.SetBool("fwd", false);
        animator.SetBool("left", false);
        animator.SetBool("right", false);
        animator.SetBool("bwd", false);
    }
    void Move()
    {
        hareket.x = left.Horizontal * hareketHizi;
        hareket.z = left.Vertical * hareketHizi;
        hareket.y = 0;
        gameObject.GetComponent<CharacterController>().Move(hareket * (Time.fixedDeltaTime / moveSpeed));
    }
    void Look()
    {
        Vector3 direction;
        direction.x = transform.position.x + right.Horizontal * 10f;
        direction.z = transform.position.z + right.Vertical * 10f;
        direction.y = transform.position.y;
        transform.LookAt(direction);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(firePoint.transform.position, firePoint.transform.forward * 10f);
    }
    void Update()
    {
        canv.transform.LookAt(Camera.main.transform.position);
        if (Physics.Raycast(firePoint.transform.position, firePoint.transform.forward, out hit, 20f, layerMask) && !shoot)
        {
            if (hit.collider.gameObject.GetComponent<ZombieAI>().alive)
            {
                StartCoroutine(Shooting());
            }
        }
        Move();
        Look();
    }
    bool shoot = false;
    IEnumerator Shooting()
    {
        shoot = true;
        StartCoroutine(Curss());
        Instantiate(muzzleFlash, firePoint);
        animator.SetTrigger("shoot");
        GameObject firedBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
        firedBullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * mermiHizi, ForceMode.Impulse);
        yield return new WaitForSeconds(0.3f);
        shoot = false;
        
    }
    IEnumerator Curss()
    {
        curs.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        curs.color = Color.white;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            TakeDamage(1);
        }

        if (other.gameObject.tag == "Coin")
        {
            tmpGUI.text = (int.Parse(tmpGUI.text) + Random.Range(5, 10)).ToString();
            Destroy(other.gameObject);
        }
    }
    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
            SceneManager.LoadScene("Game");
        }
    }
}