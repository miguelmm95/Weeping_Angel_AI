using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InCamera : MonoBehaviour
{
    Camera mainCamera;
    Plane[] cameraFrustum;
    [SerializeField] WeepingAngelAI enemie;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        var bounds = enemie.GetComponent<Collider>().bounds;
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        if(GeometryUtility.TestPlanesAABB(cameraFrustum, bounds))
        {
            enemie.endChasing();
        }
        else
        {
            enemie.startChasing();
        }
    }
}
