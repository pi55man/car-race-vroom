using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;

public class Checkpoint : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;
    private HashSet<Transform> carTriggers = new HashSet<Transform>();

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Checkpoint triggered by: " + other.name);

        // Check if the triggering object is the main body of the car
        if (other.CompareTag("Car"))
        {
            Transform carTransform = other.transform.root;

            // Only trigger if this car hasn't already triggered this checkpoint
            if (!carTriggers.Contains(carTransform))
            {
                carTriggers.Add(carTransform); // Mark this car as having triggered the checkpoint
                trackCheckpoints.playerThroughCheckpoint(this, carTransform);
            }
            else
            {
                Debug.Log("Checkpoint already triggered by this car.");
            }
        }
        else
        {
            Debug.LogWarning("Triggered by non-car object: " + other.name);
        }
    }

    // Optional: Reset triggers when needed, e.g., on lap restart
    public void ResetCheckpoint(Transform carTransform)
    {
        carTriggers.Remove(carTransform);
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }
}
