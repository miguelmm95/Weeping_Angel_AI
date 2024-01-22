using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeepingAngelAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent navAI;
    [SerializeField] Transform player;
    [SerializeField] Vector3 destination;
    [SerializeField] private float aiSpeed;
    [SerializeField] public bool wasBehindAnObstacle;

    public void startChasing()
    {
        navAI.speed = aiSpeed;
        destination = player.position;
        navAI.destination = destination;
    }

    public void endChasing()
    {
        navAI.speed = 0;
        navAI.SetDestination(transform.position);
    }

    public void endChasingWithDelay()
    {
        navAI.speed = 0;
        wasBehindAnObstacle = false;
    }
}
