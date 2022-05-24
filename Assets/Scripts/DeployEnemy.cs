using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployEnemy : MonoBehaviour
{
    private GameObject player;
    public GameObject zombie;
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        StartCoroutine(Wave());
    }
    IEnumerator Wave()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            //new WaitForSeconds(4f);
            SpawnEnemy();
        }
    }
    private void SpawnEnemy()
    {
        Vector3 location;
        switch (Random.Range(0, 4))
        {
            case 0://up
                location= new Vector3(Random.Range(player.transform.position.x + 22, player.transform.position.x + 32), player.transform.position.y, Random.Range(player.transform.position.z -18, player.transform.position.z + 18));
                break;
            case 1://right
                location = new Vector3(Random.Range(player.transform.position.x -32, player.transform.position.x +32), player.transform.position.y, Random.Range(player.transform.position.z + 9, player.transform.position.z + 18));
                break;
            case 2://left
                location = new Vector3(Random.Range(player.transform.position.x -32, player.transform.position.x + 32), player.transform.position.y, Random.Range(player.transform.position.z - 9, player.transform.position.z - 18));
                break;
            default://down
                location = new Vector3(Random.Range(player.transform.position.x - 22, player.transform.position.x - 32), player.transform.position.y, Random.Range(player.transform.position.z - 18, player.transform.position.z + 18));
                break;
        }
        Instantiate(zombie, location, Quaternion.identity);
    }
}
