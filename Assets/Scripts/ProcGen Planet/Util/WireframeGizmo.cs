using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Renders a wireframe gizmo for the attached MeshFilter component.
    /// </summary>
    /// <author>Stuart Brown</author>
    public class WireframeGizmo : MonoBehaviour
    {
        [SerializeField] private Color gizmoColour = Color.red;

        private void OnDrawGizmos()
        {
            var meshFilter = transform.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;
            Gizmos.color = gizmoColour;
            Gizmos.DrawWireMesh(meshFilter.sharedMesh, transform.position, transform.rotation, transform.localScale);
        }
    }
}