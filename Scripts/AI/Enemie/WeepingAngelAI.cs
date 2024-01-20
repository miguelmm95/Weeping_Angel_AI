using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WeepingAngelAI : MonoBehaviour
{
    public NavMeshAgent navAI;
    public Transform player;
    Vector3 destination;
    public Camera playerCamera;
    [SerializeField] private float aiSpeed;

    private void Update()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        if (GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            navAI.speed = 0;
            navAI.SetDestination(transform.position);
        }

        if (!GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            navAI.speed = aiSpeed;
            destination = player.position;
            navAI.destination = destination;
        }
    }

    //private void OnBecameInvisible()
    //{
        
    //}
    //}
}
