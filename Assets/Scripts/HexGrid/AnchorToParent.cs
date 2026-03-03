using UnityEngine;

public class AnchorToParent : MonoBehaviour
{
    public Transform targetParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.SetParent(targetParent);
        transform.localPosition = Vector3.zero;
        //transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
