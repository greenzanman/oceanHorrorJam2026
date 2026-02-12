using UnityEngine;

/*
This script creates an outline effect for the object it is attached to. 
It does this by creating a child object with a slightly larger scale and a different material. 
The outline can be enabled or disabled as needed.
*/
[RequireComponent(typeof(Renderer))]
public class Outline : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private float outlineWidth = 1.05f; // Scale factor
    [SerializeField] private bool outlineEnabled = false;
    
    private GameObject outlineObject;

    void Awake()
    {
        outlineEnabled = false;
        CreateOutline();
        DisableOutline();
    }

    public void EnableOutline()
    {
        outlineEnabled = true;
        if (outlineObject != null)
        {
            outlineObject.SetActive(true);
        }
    }
    
    public void DisableOutline()
    {
        outlineEnabled = false;
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }
    
    void CreateOutline()
    {
        // Create a copy of the object
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * outlineWidth;
        
        // Copy the mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshFilter outlineMeshFilter = outlineObject.AddComponent<MeshFilter>();
        outlineMeshFilter.mesh = meshFilter.mesh;
        
        // Create outline material
        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        Material outlineMaterial = new Material(Shader.Find("Standard"));
        outlineMaterial.color = outlineColor;
        outlineMaterial.SetFloat("_Mode", 2); // Fade mode
        outlineRenderer.material = outlineMaterial;
        
        // Render behind the original object
        outlineRenderer.sortingOrder = -1;
    }
}