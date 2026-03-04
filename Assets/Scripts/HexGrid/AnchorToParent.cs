using UnityEngine;

public class AnchorToParent : MonoBehaviour
{
    public Transform targetParent;
    public float setY = -0.25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.SetParent(targetParent);
        transform.localPosition = Vector3.zero;
        Vector3 currentPosition = transform.localPosition;
        Vector3 newPosition = new Vector3(currentPosition.x, setY, currentPosition.z);
        //transform.localRotation = Quaternion.identity;
        transform.localPosition = newPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
