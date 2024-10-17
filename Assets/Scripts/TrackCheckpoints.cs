using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common.GameUI;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    [SerializeField] private List<Transform> carTransformList;
    private List<Checkpoint> checkpointSingleList;
    private List<int> nextCheckpointSingleIndexList;

    private void Awake()
    {
        
        Transform checkpointsTransform = transform.Find("Checkpoints");
        checkpointSingleList = new List<Checkpoint>();
        foreach(Transform checkpointTransform in checkpointsTransform){
            Checkpoint checkpointSingle = checkpointTransform.GetComponent<Checkpoint>();
            checkpointSingle.SetTrackCheckpoints(this);
            checkpointSingleList.Add(checkpointSingle);
        }
        nextCheckpointSingleIndexList=new List<int>();
        foreach(Transform carTransform in carTransformList){
            nextCheckpointSingleIndexList.Add(0);
        }
        Debug.Log("Car list initialized with " + carTransformList.Count + " cars.");
    }

    public void playerThroughCheckpoint(Checkpoint checkpoint, Transform CarTransform){
            int carIndex = carTransformList.IndexOf(CarTransform);

    if (carIndex == -1)
    {
        // Handle the case where the car is not found in the list
        Debug.LogError("CarTransform not found in carTransformList!");
        return;
    }
        int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[carIndex];

            if(checkpointSingleList.IndexOf(checkpoint) == nextCheckpointSingleIndex){
                    //correct
                    Debug.Log("Correct");
                    nextCheckpointSingleIndexList[carIndex]=(nextCheckpointSingleIndex+1) % checkpointSingleList.Count;
            }else{
                  Debug.Log("Wrong");
            };
    }

}
