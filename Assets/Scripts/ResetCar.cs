using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarReset : MonoBehaviour
{
    private Vector3 initialPosition;  // To store the initial position
    private Quaternion initialRotation; // To store the initial rotation

    private void Start()
    {
        // Store the initial position and rotation of the car
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        // Check if the "R" key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCar();
        }
    }

    private void ResetCar()
    {
        // Reset the car's position and rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Optionally, reset the car's velocity if it has a Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Reset velocity
            rb.angularVelocity = Vector3.zero; // Reset angular velocity
        }

        Debug.Log("Car has been reset to starting position.");
    }
}
