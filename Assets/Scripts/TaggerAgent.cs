using System.Collections;                           
using System.Collections.Generic;                   
using UnityEngine;                                  
using Unity.MLAgents;                               
using Unity.MLAgents.Actuators;                     
using Unity.MLAgents.Sensors;                       
                                                    
public class TaggerAgent : Agent                
{
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float multiplier = 5f;
    
    [SerializeField] private RunnerAgent classObject;

    [SerializeField] private float timePerEpisode = 45f;
    private float timeLeft;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        transform.localRotation = Quaternion.Euler(new Vector3(0, Random.Range(-180f, 180f), 0));
        transform.localPosition = new Vector3(Random.Range(-10f, 10f), 5f, Random.Range(-20f, 0f));
        target.localPosition = new Vector3(Random.Range(-10f, 10f), 5f, Random.Range(-20f, 0f));
        EpisodeTimerNew();
    }

    private void Update()
    {
        CheckRemainingTime();
    }

    /*public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }*/

    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotate = actions.ContinuousActions[0];
        float move = actions.ContinuousActions[1];
        float vert = actions.ContinuousActions[2];

        rb.AddForce(transform.forward * move * multiplier);
        rb.AddForce(Vector3.up * vert * multiplier, ForceMode.Force);
        transform.Rotate(0f, rotate * 10f, 0f, Space.Self);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.rotation.eulerAngles.y, 0f));
        AddReward(-0.001f * (transform.localPosition - target.localPosition).magnitude);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[2] = Input.GetAxisRaw("VeryVertical");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Runner"))
        {
            AddReward(100f);
            classObject.AddReward(-100f);
            classObject.EndEpisode();
            EndEpisode();
        }
    }

    private void EpisodeTimerNew()
    {
        timeLeft = Time.time + timePerEpisode;
    }

    private void CheckRemainingTime()
    {
        if (Time.time >= timeLeft)
        {
            AddReward(-100f);
            classObject.AddReward(100f);
            EndEpisode();
        }
    }
}                                                   
                                                    