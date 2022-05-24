using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTarget : MonoBehaviour
{
    [SerializeField] Transform player,firePoint;
    [SerializeField] float threshold;

    private void FixedUpdate()
    {
        transform.position = (player.position + 2 * (firePoint.position - player.position));
    }
}
