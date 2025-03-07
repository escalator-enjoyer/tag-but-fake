using System.Collections;                           
using System.Collections.Generic;                   
using UnityEngine;                                  
using Unity.MLAgents;                               
using Unity.MLAgents.Actuators;                     
using Unity.MLAgents.Sensors;                       
                                                    
public class RunnerAgent : Agent                
{
    [SerializeField] private Transform tagger;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float multiplier = 5f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        transform.localRotation = Quaternion.Euler(new Vector3(0, Random.Range(-180f, 180f), 0));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotate = actions.ContinuousActions[0];
        float move = actions.ContinuousActions[1];

        rb.MovePosition(transform.forward * move * multiplier * Time.deltaTime + transform.position);
        transform.Rotate(0f, rotate * multiplier, 0f, Space.Self);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.rotation.eulerAngles.y, 0f));
        AddReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
}                                                   
                                                    