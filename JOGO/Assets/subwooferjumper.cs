using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class subwooferjumper : MonoBehaviour
{
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("subwoofer"))
        {
            Debug.Log("Colidiu com: " + collision.gameObject.name);
            rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        }
    }
}
