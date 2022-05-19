using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Animator animator;
    private Vector3 hareket;
    [SerializeField] private float hareketHizi, mermiHizi;
    GameObject kamera;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bullet;
    [SerializeField] float jumpingForce;
    float yGravity;
    int jumpCount = 0;
    private bool isDashed = false;
    private void Awake()
    {
        kamera = GameObject.FindWithTag("MainCamera");
    }



    void Start()
    {
        
    }

    void Update()
    {
        if (hareket.x == 0 && hareket.z == 0) 
        {
            animator.SetBool("move", false);
        }
        else
        {
            animator.SetBool("move", true);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift)&&isDashed==false)
        {
            isDashed = true;
            animator.SetTrigger("dash");
        }


        hareket.x = Input.GetAxis("Horizontal") * hareketHizi;
        hareket.z = Input.GetAxis("Vertical") * hareketHizi;
        gameObject.GetComponent<CharacterController>().Move(hareket*Time.deltaTime);
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5.23f;

        Vector3 objectPos = UnityEngine.Camera.main.WorldToScreenPoint(transform.position);

        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;

        float angle = Mathf.Atan2(mousePos.x, mousePos.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));

        kamera.transform.position = new Vector3(transform.position.x, transform.position.y + 11, transform.position.z - 6);

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

        if (Input.GetKeyDown("space")&&jumpCount<3)
        {
            Jumping();
        }
        if (gameObject.GetComponent<CharacterController>().isGrounded)
        {
            jumpCount = 1;
        }
        hareket.y = yGravity;

        if (gameObject.GetComponent<CharacterController>().isGrounded)
        {
            isDashed = false;
        }

    }
    
    void Shooting()
    {
        animator.SetTrigger("shoot");
        GameObject firedBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
        firedBullet.GetComponent<Rigidbody>().AddForce(firePoint.forward*mermiHizi,ForceMode.Impulse);
    }
    void Jumping()
    {
        yGravity = jumpingForce ;
        jumpCount++;
    }
}
