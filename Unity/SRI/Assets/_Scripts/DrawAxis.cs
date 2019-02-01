using UnityEngine;
using System.Collections;

public class DrawAxis : MonoBehaviour
{
    float size = 1f;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.rotation*Vector3.right * size + transform.position);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.rotation * Vector3.up * size + transform.position);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.rotation * Vector3.forward * size + transform.position);
        Gizmos.color = Color.white;
    }
}