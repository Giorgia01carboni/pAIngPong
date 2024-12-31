using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

/* Handles events from Disk.cs script*/
public enum Event
{
    HitOutOfBounds = 0,
    HitByPaddle = 1
}

public class PaddleAgent : Agent
{
    [SerializeField]
    public GameObject disk;
    public GameObject topWall;
    public GameObject floorWall;
    public float paddleSpeed = 8.5f;

    Rigidbody2D diskRb;
    private Vector2 previousPosition;
    private Vector2 startingPos;

    private int hitCounter;

    public override void Initialize()
    {
        diskRb = disk.GetComponent<Rigidbody2D>();
        previousPosition = transform.localPosition;
        startingPos = transform.localPosition;
        hitCounter = 0;

        ResetSceneParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Paddle position (1 floats)
        sensor.AddObservation(gameObject.transform.localPosition.y);

        // Disk velocity (2 floats)
        sensor.AddObservation(diskRb.velocity.x);
        sensor.AddObservation(diskRb.velocity.y);

        // Paddle velocity along y axis (1 float)
        float velocityY = (transform.localPosition.y - previousPosition.y) / Time.deltaTime;
        sensor.AddObservation(velocityY);

        // Distance between paddle and disk (2 floats)
        sensor.AddObservation(disk.transform.localPosition.x - gameObject.transform.localPosition.x);
        sensor.AddObservation(disk.transform.localPosition.y - gameObject.transform.localPosition.y);

        //// Disk absolute position (2 floats)
        //sensor.AddObservation(disk.transform.position.x);
        //sensor.AddObservation(disk.transform.position.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var paddleDir = Vector2.zero;
        float paddleTopEdge = gameObject.transform.localPosition.y + (gameObject.transform.localScale.y / 2);
        float paddleBottomEdge = gameObject.transform.localPosition.y - (gameObject.transform.localScale.y / 2);

        float topWallBottomEdge = topWall.transform.localPosition.y - (topWall.transform.localScale.y / 2);
        float floorWallTopEdge = floorWall.transform.localPosition.y + (floorWall.transform.localScale.y / 2);

        var move = actions.DiscreteActions[0];
        //Debug.Log($"Action received: {move}");
        /* DiscreteAction[0] moves the paddle: 
         * 0 = do nothing and stop,
         * 1 = move up
         * 2 = move down
         * 
         * Before moving, check that the paddle doesn't overlap with the top and bottom walls */

        if (move == 0 && paddleTopEdge < topWallBottomEdge)
            paddleDir = Vector2.up;
        else if (move == 1 && paddleBottomEdge > floorWallTopEdge)
            paddleDir = Vector2.down;
        else if (move == 2)
            paddleDir = Vector2.zero;

        if (paddleDir != Vector2.zero)
        {
            gameObject.transform.Translate(paddleDir * paddleSpeed * Time.deltaTime);
        }
    }

    // Heuristic implements some basic human controls for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            // Paddle goes up
            discreteActionsOut[0] = 0;
        } else if (Input.GetKey(KeyCode.S))
        {
            // Paddle goes down
            discreteActionsOut[0] = 1;
        } else
        {
            discreteActionsOut[0] = 2;
        }
    }

    /* Handles the rewards logic with the data given by the Disk.cs script:
     * When the disk goes out of bounds=> punish the agent by -1.0f
     * When the paddle hits the disk => reward the agent by 0.3f
     * If the paddle manages to keep the disk in game (consecutive hits) => reward the agent 
     */
    public void ResolveEvent(Event triggerEvent, float distance)
    {
        float reward = 0.0f;
        switch (triggerEvent)
        {
            case Event.HitOutOfBounds:
                reward = -1.0f;
                SetReward(reward);
                // Add penalisation based on the distance between
                // the paddle and the disk when it went out of bounds.
                SetReward(-0.01f * distance);
                this.EndEpisode();
                ResetSceneParameters();
                break;

            case Event.HitByPaddle:
                reward = 0.3f;
                SetReward(reward);
                hitCounter++;
                reward = 0.05f;
                SetReward(reward * hitCounter);
                break;
        }
    }

    /* Called when a new episode starts */
    private void ResetSceneParameters()
    {
        transform.localPosition = new Vector2(startingPos.x, startingPos.y); 
        previousPosition = transform.localPosition;
        hitCounter = 0;
    }
}
