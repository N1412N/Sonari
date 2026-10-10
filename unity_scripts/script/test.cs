using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        float y = Mathf.Sin(Time.time) * 10f;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
