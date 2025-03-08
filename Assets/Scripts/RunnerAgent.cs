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
    [SerializeField] private Camera agentCamera;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform mainSensor;

    private float xRotation = 0f;
    private bool isGrounded = false;
    private bool jumpRequested = false;
    private float lastJumpTime = -1f;
    private float previousDistance;

    private float mouseX, mouseY;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (agentCamera != null && gameObject.name != "r_mc")
        {
            agentCamera.gameObject.SetActive(false);
        }
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        transform.localRotation = Quaternion.Euler(new Vector3(0, Random.Range(-180f, 180f), 0));
        transform.localPosition = new Vector3(Random.Range(-45f, 45f), 5f, Random.Range(-45f, 45f));
        previousDistance = (tagger.localPosition - transform.localPosition).magnitude;
    }

    private void Update()
    {
        if (agentCamera != null && agentCamera.gameObject.activeSelf)
        {
            HandleCameraRotation();
            HandleInput();

            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void HandleCameraRotation()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        agentCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleInput()
    {
        if (agentCamera != null && agentCamera.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Time.time - lastJumpTime < 0.3f)
                {
                    jumpRequested = false;
                }
                else
                {
                    jumpRequested = true;
                }
                lastJumpTime = Time.time;
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (agentCamera != null && agentCamera.gameObject.activeSelf)
        {
            ActionSegment<float> continuous = actionsOut.ContinuousActions;
            continuous[0] = Input.GetAxis("Mouse X");
            continuous[1] = Input.GetAxis("Mouse Y");
            continuous[2] = Input.GetAxis("Vertical");
            continuous[3] = Input.GetAxis("Horizontal");

            ActionSegment<int> discrete = actionsOut.DiscreteActions;
            discrete[0] = jumpRequested ? 1 : 0;

            jumpRequested = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((tagger.localPosition - transform.localPosition).magnitude);
        sensor.AddObservation(rb.velocity);
        sensor.AddObservation(transform.forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float mouseX = actions.ContinuousActions[0] * mouseSensitivity * Time.fixedDeltaTime;
        float mouseY = actions.ContinuousActions[0] * mouseSensitivity * Time.fixedDeltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 45f);
        mainSensor.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        float vertical = actions.ContinuousActions[2];
        float horizontal = actions.ContinuousActions[3];
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;
        Vector3 movement = moveDirection * multiplier;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (actions.DiscreteActions[0] == 1 && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        float currentDistance = (tagger.localPosition - transform.localPosition).magnitude;
        AddReward(0.001f * (currentDistance - previousDistance));
        previousDistance = currentDistance;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
            isGrounded = false;
    }
}
