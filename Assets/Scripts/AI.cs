using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    
    public NavMeshAgent agent;
    private GameObject player;
    //public ParticleSystem particle;
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
    }
    void Update()
    {
        agent.SetDestination(player.transform.position);
        
    }
}
