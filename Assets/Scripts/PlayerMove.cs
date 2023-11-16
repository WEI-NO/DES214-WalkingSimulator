/*******************************************************************************
File:      PlayerMove.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/16/2022

Description:
    Handles player movement and rotation, with instance accelaration/deceleration.
    This script is attached to the player model, which must be parented to the
    player object in order to work.

    The player object is a multi-object construct with a hierarchy of child
    objects (do not change this hierarchy) as follows:

    Player (Root) - [RigidBody, Collider] -- has the physics components
        PlayerModel - [PlayerMove, MeshFilter, MeshRenderer] -- handles player movement and has the mesh components
        GroundDetector - [PlayerJump, Collider (Trigger)] -- handles player jumping and ground detection
        YawPivot - [CameraYaw] -- handles horizontal camera rotation
            MovePivot - [n/a] -- has a position offset to where the player's "eyes" are
                PitchPivot - [CameraPitch] -- handles setting the pitch of the camera
                    Camera - [Camera, CameraOcclusion] -- handles camera occlusion and has the camera component

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////
    // DESIGNER VARIABLES
    ////////////////////////////////////////////////////////////////////////////

    //Speed that the player moves (units per second)
    public float MoveSpeed = 10.0f; //A fairly reasonable value
    //Speed that the player rotates (degrees per second)
    private float RotateSpeed = 150.0f; //A fairly reasonable value
    ////////////////////////////////////////////////////////////////////////////

    //A reference to the ground detector
    PlayerJump GroundDetector;
    Rigidbody rigidbody;

    //Start is called before the first frame update
    void Start()
    {
        //Save the ground detector
        GroundDetector = GameObject.Find("GroundDetector").GetComponent<PlayerJump>();
        rigidbody = GetComponentInParent<Rigidbody>();
    }

    //Fixed update is called once per physics update
    void FixedUpdate()
    {
        if (GameSequence.GamePaused) return;

        //Rotate player around the Y axis
        if (Input.GetKey(KeyCode.A))
            transform.Rotate(0.0f, -1.0f * RotateSpeed * Time.deltaTime, 0.0f, Space.Self);
        if (Input.GetKey(KeyCode.D))
            transform.Rotate(0.0f, 1.0f * RotateSpeed * Time.deltaTime, 0.0f, Space.Self);

        //Start with a zero vector
        Vector3 dir = Vector3.zero;

        //Set movement direction based on where the player model is facing
        if (Input.GetKey(KeyCode.W))
            dir += transform.forward;
        if (Input.GetKey(KeyCode.S))
            dir -= transform.forward;
        if (Input.GetKey(KeyCode.E))
            dir += transform.right;
        if (Input.GetKey(KeyCode.Q))
            dir -= transform.right;

        //If we aren't on the ground, don't mess with the velocity
        if (!GroundDetector.IsGrounded())
        {
            return;
        }

        //Adjust the speed if we are going up a slope
        float adjustedSpeed = MoveSpeed;
        float verticalVelocity = transform.parent.GetComponent<Rigidbody>().velocity.y;
        if (verticalVelocity > 0.0f)
            adjustedSpeed *= GroundDetector.GetSlopeAdjustment();

        //Normalize direction and apply the adjusted speed
        dir = dir.normalized * adjustedSpeed; //No delta time because the physics engine will handle that
        //Do not overwrite the Y velocity because that would screw up jumping and falling
        transform.parent.GetComponent<Rigidbody>().velocity = new Vector3(dir.x, verticalVelocity, dir.z);
    }
}
