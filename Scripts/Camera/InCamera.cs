using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class InCamera : MonoBehaviour
{
    Camera mainCamera;
    Plane[] cameraFrustum;
    [SerializeField] WeepingAngelAI enemy;
    [SerializeField] LayerMask obstacleLayer;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        //Debug.DrawLine(transform.position, enemy.transform.position);

        var bounds = enemy.GetComponent<Collider>().bounds;
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        if(GeometryUtility.TestPlanesAABB(cameraFrustum, bounds))
        {
            if (Physics.Linecast(transform.position, enemy.transform.position, obstacleLayer))
            {
                enemy.startChasing();
                enemy.wasBehindAnObstacle = true;
            }
            else
            {
                enemy.endChasing();
            }
        }
        else
        {
            enemy.startChasing();
        }
    }
}
