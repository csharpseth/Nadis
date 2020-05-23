using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TornadoController : MonoBehaviour
{
    public Vector3 origin;
    public float distanceToFindNext = 5f;
    public float searchRadius = 50f;
    public LayerMask groundMask;
    public float moveSpeed = 5f;

    Vector3 destination;

    private void Update()
    {
        CheckSetDestination();
    }

    private void CheckSetDestination()
    {
        if(destination == Vector3.zero)
        {
            destination = Util.RandomInsideSphereNormalized(origin, searchRadius, groundMask);
        }else
        {
            float sqrDistance = (destination - transform.position).sqrMagnitude;
            if(sqrDistance <= (distanceToFindNext * distanceToFindNext))
            {
                destination = Vector3.zero;
            }else
            {
                Vector3 pos = transform.position;
                pos = pos + ((destination - transform.position).normalized * moveSpeed * Time.deltaTime);
                pos = Util.NormalizeToLayer(pos, groundMask);
                transform.position = pos;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, searchRadius);
        Gizmos.DrawSphere(transform.position, 2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, distanceToFindNext);
    }
}
