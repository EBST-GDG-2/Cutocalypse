using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class muzzleFlas : MonoBehaviour
{
    // Start is called before the first frame update


    void Start()
    {
        StartCoroutine(muzzleFlashing());
    }

    IEnumerator muzzleFlashing()
    {
        yield return new WaitForSeconds(0.15f);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
