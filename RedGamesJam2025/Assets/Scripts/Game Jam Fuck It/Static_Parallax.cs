using UnityEngine;

public class StaticParallax : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxSpeed = 0.5f;
    
    private Vector3 startPosition;
    private Vector3 lastCameraPosition;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        startPosition = transform.position;
        lastCameraPosition = cameraTransform.position;
    }
    
    void Update()
    {
        if (cameraTransform == null) return;
        
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        transform.position += deltaMovement * parallaxSpeed;
        
        lastCameraPosition = cameraTransform.position;
    }
}