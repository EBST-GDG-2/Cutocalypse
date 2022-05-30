using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject canv;
    public Texture2D curs;
    private TextMeshProUGUI tmpGUI;
    public HealthBar healthBar;
    [SerializeField] Animator animator;
    private Vector3 hareket;
    [SerializeField] private float hareketHizi, mermiHizi;
    //GameObject kamera;
    public Texture2D cursorTexture;
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
        cursorTexture = curs;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
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
    void Update()
    {
        canv.transform.LookAt(Camera.main.transform.position);
        
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
            SceneManager.LoadScene("Game");
        }

        PlayerControl();


        hareket.x = Input.GetAxis("Horizontal") * hareketHizi;
        hareket.z = Input.GetAxis("Vertical") * hareketHizi;
        gameObject.GetComponent<CharacterController>().SimpleMove(hareket);
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5.23f;

        Vector3 objectPos =Camera.main.WorldToScreenPoint(transform.position);

        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;

        float angle = Mathf.Atan2(mousePos.x, mousePos.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));

        //kamera.transform.position = new Vector3(transform.position.x, transform.position.y + 11, transform.position.z - 6);

        if (Input.GetButtonDown("Fire1"))
        {
            Shooting();
        }
        if (gameObject.GetComponent<CharacterController>().isGrounded)
        {
            yGravity= Physics.gravity.y * Time.deltaTime * 6;
        }
        else
        {
            yGravity += Physics.gravity.y * Time.deltaTime * 6;
        }
        hareket.y = yGravity;

        if (gameObject.GetComponent<CharacterController>().isGrounded)
        {
            isDashed = false;
        }

    }
    void Shooting()
    {
        //StartCoroutine(Curss());
        Instantiate(muzzleFlash, firePoint);
        animator.SetTrigger("shoot");
        GameObject firedBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
        firedBullet.GetComponent<Rigidbody>().AddForce(firePoint.forward*mermiHizi,ForceMode.Impulse);
    }


    /*IEnumerator Curss()
    {
        curs.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        curs.color = Color.white;
    }
    */
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            TakeDamage(1);
        }

        if (other.gameObject.tag == "Coin")
        {
            tmpGUI.text = (int.Parse(tmpGUI.text) + Random.Range(5,10)).ToString();
            Destroy(other.gameObject);
        }
    } 
    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }
    public void PlayerControl()
    {
        direction = firePoint.transform.position - gameObject.transform.position;
        if (direction.x > 0)
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                animator.SetBool("FW", true);
                animator.SetBool("BW", false);
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                animator.SetBool("BW", true);
                animator.SetBool("FW", false);
            }
            else
            {
                animator.SetBool("BW", false);
                animator.SetBool("FW", false);
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                animator.SetBool("R", true);
                animator.SetBool("L", false);
                Debug.Log("sag");
                Debug.Log(direction.x.ToString() + "  " + direction.z.ToString());
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                animator.SetBool("L", true);
                animator.SetBool("R", false);
                Debug.Log("sol");
                Debug.Log(direction.x.ToString() + "  " + direction.z.ToString());
            }
            else
            {
                animator.SetBool("R", false);
                animator.SetBool("L", false);
            }
        }
        if (direction.x < 0)
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                animator.SetBool("FW", false);
                animator.SetBool("BW", true);
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                animator.SetBool("BW", false);
                animator.SetBool("FW", true);
            }
            else
            {
                animator.SetBool("BW", false);
                animator.SetBool("FW", false);
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                animator.SetBool("R", false);
                animator.SetBool("L", true);
                Debug.Log(direction.x.ToString() + "  " + direction.z.ToString());
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                animator.SetBool("L", false);
                animator.SetBool("R", true); 
                Debug.Log(direction.x.ToString() + "  " + direction.z.ToString());
            }
            else
            {
                animator.SetBool("R", false);
                animator.SetBool("L", false);
            }
        }

        //if (direction.z > 0)
        //{
        //    if (Input.GetAxis("Vertical") > 0)
        //    {
        //        animator.SetBool("L", true);
        //        animator.SetBool("R", false);
        //    }
        //    else if (Input.GetAxis("Vertical") < 0)
        //    {
        //        animator.SetBool("R", true);
        //        animator.SetBool("L", false);
        //    }
        //    else
        //    {
        //        animator.SetBool("BW", false);
        //        animator.SetBool("FW", false);
        //    }
        //    if (Input.GetAxis("Horizontal") > 0)
        //    {
        //        animator.SetBool("FW", true);
        //        animator.SetBool("BW", false);
        //    }
        //    else if (Input.GetAxis("Horizontal") < 0)
        //    {
        //        animator.SetBool("BW", true);
        //        animator.SetBool("FW", false);
        //    }
        //    else
        //    {
        //        animator.SetBool("R", false);
        //        animator.SetBool("L", false);
        //    }
        //}

        //if (direction.z < 0)
        //{
        //    if (Input.GetAxis("Vertical") > 0)
        //    {
        //        animator.SetBool("L", false);
        //        animator.SetBool("R", true);
        //    }
        //    else if (Input.GetAxis("Vertical") < 0)
        //    {
        //        animator.SetBool("R", false);
        //        animator.SetBool("L", true);
        //    }
        //    else
        //    {
        //        animator.SetBool("BW", false);
        //        animator.SetBool("FW", false);
        //    }
        //    if (Input.GetAxis("Horizontal") > 0)
        //    {
        //        animator.SetBool("FW", false);
        //        animator.SetBool("BW", true);
        //    }
        //    else if (Input.GetAxis("Horizontal") < 0)
        //    {
        //        animator.SetBool("BW", false);
        //        animator.SetBool("FW", true);
        //    }
        //    else
        //    {
        //        animator.SetBool("R", false);
        //        animator.SetBool("L", false);
        //    }
        //}

        if(Input.GetKeyDown(KeyCode.LeftShift) && isDashed != true)
        {
            if (animator.GetBool("FW"))
            {
                animator.SetTrigger("rollFW");
            }
            if (animator.GetBool("BW"))
            {
                animator.SetTrigger("rollBW");
            }
            if (animator.GetBool("R"))
            {
                animator.SetTrigger("rollR");
            }
            if (animator.GetBool("L"))
            {
                animator.SetTrigger("rollL");
            }

        }




    }
}
