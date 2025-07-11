using UnityEngine;

public class Follow_Camera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float followSpeed = 8f;
    public Vector3 offset = new Vector3(-2, 0, -10);
    
    [Header("Follow Options")]
    public bool followX = true;
    public bool followY = false;
    
    private Vector3 startPosition;
    
    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        startPosition = transform.position;
    }
    
    void FixedUpdate()
    {
        if (target == null) return;
        
        Vector3 targetPosition = transform.position;
        
        if (followX)
        {
            targetPosition.x = target.position.x + offset.x;
        }
        
        if (followY)
        {
            targetPosition.y = target.position.y + offset.y;
        }
        else
        {
            targetPosition.y = startPosition.y + offset.y;
        }
        
        targetPosition.z = startPosition.z + offset.z;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
    }
}