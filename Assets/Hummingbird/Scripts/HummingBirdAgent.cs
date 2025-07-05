using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.XR;

public class HummingBirdAgent : Agent
{
    public float moveForce = 2f;
    public float pitchSpeed = 100f;
    public float yawSpeed = 100f;

    public Transform beakTip;

    public Camera agentCamera;

    public bool trainingMode;

    private float prevDistanceToFlower;


    new private Rigidbody rigidbody;

    private FlowerArea flowerArea;

    private Flower nearestFlower;

    private float smoothPitchChange = 0f;
    private float smoothYawChange = 0f;

    private const float MaxPitchAngle = 80f;

    private const float BeakTipRadius = 0.008f;

    private bool frozen  = false;

    public float NectarObtained {  get; private set; }


    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();

        flowerArea = GetComponentInParent<FlowerArea>();

        MaxStep = trainingMode ? 1500 : 0;


    }

    public override void OnEpisodeBegin()
    {
        if (trainingMode) 
        {
           flowerArea.ResetFlowers();
        }

        NectarObtained = 0;

        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        bool inFrontFLower = true;

        if (trainingMode) 
        {
            inFrontFLower = Random.value > 0.1f;
        }

        MoveToSafeRandomPosition(inFrontFLower);

        UpdateNearestFlower();

        
        if (nearestFlower != null)
            prevDistanceToFlower = Vector3.Distance(beakTip.position, nearestFlower.FlowerCenterPosition);
    }



    /// <summary>
    /// actions[i] represents;
    /// index 0 : move vector x ( +1 = right , -1 left)
    /// index 1 : move vector y ( +1 = up , -1 down)
    /// index 2 : move vector z ( +1 = forward , -1 backward)
    /// index 3 : pitch angle  ( +1 = pitchup, -1 pitch down)
    /// index 4 : yaw angle  ( +1 = turn right , -1 turn left )
    /// 
    /// 
    /// </summary>
    /// <param name="actions"></param>

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (frozen) return;

        // ─── Movement & rotation (your existing code) ───────────────
        Vector3 move = new Vector3(
            actions.ContinuousActions[0],
            actions.ContinuousActions[1],
            actions.ContinuousActions[2]);
        rigidbody.AddForce(move * moveForce);

        Vector3 rotationVector = transform.rotation.eulerAngles;
        float pitchChange = actions.ContinuousActions[3];
        float yawChange = actions.ContinuousActions[4];

        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
   
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if(nearestFlower == null) 
        {
            sensor.AddObservation(new float[10]);
            return;
        }

        sensor.AddObservation(transform.localRotation.normalized);

        Vector3 toFlower = nearestFlower.FlowerCenterPosition - beakTip.position;

        sensor.AddObservation(toFlower.normalized);


        sensor.AddObservation(Vector3.Dot(toFlower.normalized , -nearestFlower.FlowerUpVector.normalized));

        sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, - nearestFlower.FlowerUpVector.normalized ));

        sensor.AddObservation(toFlower.magnitude / FlowerArea.AreaDiameter);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        
        Vector3 forward = Vector3.zero;
        Vector3 left = Vector3.zero;    
        Vector3 up = Vector3.zero;
        float pitch = 0f;
        float yaw = 0f;

        // Forward/backward
        if (Input.GetKey(KeyCode.W)) forward = transform.forward;
        else if (Input.GetKey(KeyCode.S)) forward = -transform.forward;

        // Right/left
        if (Input.GetKey(KeyCode.A)) left = -transform.right;
        else if (Input.GetKey(KeyCode.D)) left = transform.right;

        // Up/down
        if (Input.GetKey(KeyCode.E)) up = transform.up;
        else if (Input.GetKey(KeyCode.C)) up = -transform.up;

        // Pitch
        if (Input.GetKey(KeyCode.UpArrow)) pitch = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) pitch = -1f;

        // Yaw
        if (Input.GetKey(KeyCode.RightArrow)) yaw = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) yaw = -1f;

        Vector3 combined = (forward + up + left).normalized;

        // Assign actions
        continuousActionsOut[0] = combined.x;
        continuousActionsOut[1] = combined.y;
        continuousActionsOut[2] = combined.z;
        continuousActionsOut[3] = pitch;
        continuousActionsOut[4] = yaw;
    }
    

    public void FreezeAgent() 
    {
        Debug.Assert(trainingMode == false , "Freeze/Unfreeze not supported in training ");
        frozen = true;
        rigidbody.Sleep();
    }

    public void UnFreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training ");
        frozen = false;
        rigidbody.WakeUp();
    }




    private void UpdateNearestFlower()
    {
        foreach(Flower flower in flowerArea.Flowers) 
        {
            if(nearestFlower == null && flower.HasNectar) 
            {
                nearestFlower = flower;
            }
            else if (flower.HasNectar) 
            {
                float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);
                float distanceToCurrentNearestFlower = Vector3.Distance(nearestFlower.transform.position, beakTip.position);

                if (!nearestFlower.HasNectar || distanceToFlower < distanceToCurrentNearestFlower)
                {
                    nearestFlower = flower;
                }
            }
        }
    }

    private void MoveToSafeRandomPosition(bool inFrontOfFlower)
    {
        bool safePositionFound = false;
        int attemptsRemaining = 100; // Prevent an infinite loop
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // Loop until a safe position is found or we run out of attempts
        while (!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining--;
            if (inFrontOfFlower)
            {
                // Pick a random flower
                Flower randomFlower = flowerArea.Flowers[UnityEngine.Random.Range(0, flowerArea.Flowers.Count)];

                // Position 10 to 20 cm in front of the flower
                float distanceFromFlower = UnityEngine.Random.Range(.05f, .1f);
                potentialPosition = randomFlower.transform.position + randomFlower.FlowerUpVector * distanceFromFlower;

                // Point beak at flower (bird's head is center of transform)
                Vector3 toFlower = randomFlower.FlowerCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);
            }
            else
            {
                // Pick a random height from the ground
                float height = UnityEngine.Random.Range(1.2f, 2.5f);

                // Pick a random radius from the center of the area
                float radius = UnityEngine.Random.Range(2f, 7f);

                // Pick a random direction rotated around the y axis
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);

                // Combine height, radius, and direction to pick a potential position
                potentialPosition = flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;

                // Choose and set random starting pitch and yaw
                float pitch = UnityEngine.Random.Range(-60f, 60f);
                float yaw = UnityEngine.Random.Range(-180f, 180f);
                potentialRotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // Check to see if the agent will collide with anything
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);

            // Safe position has been found if no colliders are overlapped
            safePositionFound = colliders.Length == 0;
        }

        Debug.Assert(safePositionFound, "Could not find a safe position to spawn");

        // Set the position and rotation
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }


    private void OnTriggerEnter(Collider other)
    {
        TriggerEnterOrStay(other);
    }


    private void OnTriggerStay(Collider other)
    {
        TriggerEnterOrStay(other);

    }

    private void TriggerEnterOrStay(Collider collider)
    {
        if (collider.CompareTag("nectar"))
        {
            Vector3 closestPointToBeakTip = collider.ClosestPoint(beakTip.position);

            // Check if the closest collision point is close to the beak tip
            // Note: a collision with anything but the beak tip should not count
            if (Vector3.Distance(beakTip.position, closestPointToBeakTip) < BeakTipRadius)
            {
                // Look up the flower for this nectar collider
                Flower flower = flowerArea.GetFlowerFromNectar(collider);

                // Attempt to take .01 nectar
                // Note: this is per fixed timestep, meaning it happens every .02 seconds, or 50x per second
                float nectarReceived = flower.Feed(.01f);

                // Keep track of nectar obtained
                NectarObtained += nectarReceived;

                if (trainingMode)
                {
                    // Calculate reward for getting nectar
                    float bonus = .02f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, -nearestFlower.FlowerUpVector.normalized));
                    AddReward(.05f + bonus);
                }

                // If flower is empty, update the nearest flower
                if (!flower.HasNectar)
                {
                    UpdateNearestFlower();
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ( trainingMode && collision.collider.CompareTag("boundary")) 
        {
            AddReward(-0.5f);
          
        }
    }


    private void Update()
    {
        if (nearestFlower != null) 
        {
            Debug.DrawLine(beakTip.position, nearestFlower.FlowerCenterPosition, Color.green);


        }
    }

    private void FixedUpdate()
    {
        
        if(nearestFlower != null && !nearestFlower.HasNectar) 
        {
            UpdateNearestFlower();
        }

    }
}
