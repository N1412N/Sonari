using UnityEngine;

public class wall : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Push player back inside
            Vector3 pos = other.transform.position;
            pos = transform.position; // or clamp to safe area
            other.transform.position = pos;

            Debug.Log("Player tried to leave boundary!");
        }
    }
}
