using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColliderVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public bool alwaysShowColliders = true;
    public Color colliderColor = new Color(0, 1, 0, 0.3f); // Green
    public Color triggerColor = new Color(0, 0.5f, 1, 0.3f); // Blue

    void OnDrawGizmos()
    {
        if (!alwaysShowColliders) return;

        foreach (Collider collider in GetComponents<Collider>())
        {
            if (!collider.enabled) continue;

            Matrix4x4 originalMatrix = Gizmos.matrix;
            Color originalColor = Gizmos.color;

            //set color based on trigger status
            Gizmos.color = collider.isTrigger ? triggerColor : colliderColor;

            //handle different collider types
            if (collider is BoxCollider box)
            {
                DrawBoxCollider(box);
            }
            else if (collider is SphereCollider sphere)
            {
                DrawSphereCollider(sphere);
            }
            else if (collider is CapsuleCollider capsule)
            {
                DrawCapsuleCollider(capsule);
            }
            else if (collider is MeshCollider meshCollider)
            {
                if (meshCollider.convex)
                {
                    Gizmos.DrawWireMesh(meshCollider.sharedMesh,
                                      transform.position,
                                      transform.rotation,
                                      transform.lossyScale);
                }
            }

            Gizmos.matrix = originalMatrix;
            Gizmos.color = originalColor;
        }
    }

    void DrawBoxCollider(BoxCollider box)
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.TransformPoint(box.center),
            transform.rotation,
            transform.lossyScale
        );

        Gizmos.DrawWireCube(Vector3.zero, box.size);
    }

    void DrawSphereCollider(SphereCollider sphere)
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.TransformPoint(sphere.center),
            transform.rotation,
            transform.lossyScale
        );

        Gizmos.DrawWireSphere(Vector3.zero, sphere.radius);
    }

    void DrawCapsuleCollider(CapsuleCollider capsule)
    {
        Vector3 direction = GetDirectionVector(capsule.direction);
        float radius = capsule.radius;
        float height = capsule.height;

        Vector3 localTop = capsule.center + direction * (height / 2 - radius);
        Vector3 localBottom = capsule.center - direction * (height / 2 - radius);

        Vector3 worldTop = transform.TransformPoint(localTop);
        Vector3 worldBottom = transform.TransformPoint(localBottom);

        //draw spheres and connecting lines
        Gizmos.DrawWireSphere(worldTop, radius * transform.lossyScale.x);
        Gizmos.DrawWireSphere(worldBottom, radius * transform.lossyScale.x);
        Gizmos.DrawLine(worldTop, worldBottom);
    }

    Vector3 GetDirectionVector(int direction)
    {
        return direction switch
        {
            0 => Vector3.right,
            1 => Vector3.up,
            2 => Vector3.forward,
            _ => Vector3.up,
        };
    }
}
