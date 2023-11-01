/*******************************************************************************
File:      PlayerJump.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/16/2022

Description:
    Handles the player jumping, including detecting contact with the ground.
    This script is attached to the ground detector object,
    which must be parented the player for it to work.

*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////
    // DESIGNER VARIABLES
    ////////////////////////////////////////////////////////////////////////////

    //Speed (and therefore height) that the player jumps.
    public float JumpSpeed = 10.0f; //A fairly reasonable value

    //Gravity strength. Make this higher (along with increasing the
    //jump speed) to make the player feel less "floaty".
    public float Gravity = 50.0f; //A fairly reasonable value

    //Maximum slope that can be walked up or jumped on
    private float SlopeLimit = 60.0f;

    ////////////////////////////////////////////////////////////////////////////

    //Start is called before the first frame update
    void Start()
    {
        //Gravity pulls down on the Y axis
        Physics.gravity = new Vector3(0.0f, -Gravity, 0.0f);
    }

    //Update is called once per frame
    void Update()
    {
        //Can't jump unless you are grounded
        if (!IsGrounded())
            return;
        //Can't jump unless you hit space
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        //Jump detected, so first get our current velocity
        Vector3 newVelocity = transform.parent.GetComponent<Rigidbody>().velocity;
        //Adjust the jump speed if we are on a slope and already going up
        float adjustedSpeed = JumpSpeed;
        if (newVelocity.y > 0.0f)
            adjustedSpeed *= GetSlopeAdjustment();
        //Modify the y component of the velocity only
        newVelocity.y = adjustedSpeed;
        transform.parent.GetComponent<Rigidbody>().velocity = newVelocity;
    }

    //Number of non-trigger objects currently in contact
    private int ContactCount = 0;

    //Increment the contact count if the other object is not a trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
            ++ContactCount;
    }

    //Decrement the contact count if the other object is not a trigger
    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
            --ContactCount;
    }

    //Return whether the player is grounded
    public bool IsGrounded()
    {
        //Update the ground slope
        CheckGroundSlope();
        //Under the slop limit, so we are grounded
        if (GroundSlope <= SlopeLimit)
            return true;
        //Over the sloper limit, so we are not grounded
        if (GroundSlope < float.MaxValue)
            return false;
        //No slopes detected, but there are contacts, so
        //that still counts as grounded (maybe a narrow object?)
        if (ContactCount > 0)
            return true;
        //No slopes, no contacts, so we are not grounded
        return false;
    }

    //Track the current lowest ground slope
    private float GroundSlope = 0.0f;

    //Get the speed adjustment for moving and jumping
    public float GetSlopeAdjustment()
    {
        //Not grounded, so no moving or jumping
        if (!IsGrounded())
            return 0.0f;
        //Calculate the slope adjustment
        if (GroundSlope <= SlopeLimit)
            return (SlopeLimit - GroundSlope) / SlopeLimit;
        //This means the ground slope detection did not find
        //anything, but the contact count did. Don't adjust
        //the speed, just to make sure the player doesn't get stuck.
        return 1.0f;
    }

    //Checks for ground below the player and updates the ground angle
    private void CheckGroundSlope()
    {
        //Get the collider and a box half it's size
        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 halfBox = box.size / 2.0f;
        //Get the starting position for rays from the center and corners of the box
        Vector3 pos = transform.position;
        Vector3 boxMiddle = new Vector3(pos.x, pos.y + halfBox.y / 2.0f, pos.z);
        Vector3 boxCorner1 = new Vector3(pos.x - halfBox.x, boxMiddle.y, pos.z - halfBox.z);
        Vector3 boxCorner2 = new Vector3(pos.x - halfBox.x, boxMiddle.y, pos.z + halfBox.z);
        Vector3 boxCorner3 = new Vector3(pos.x + halfBox.x, boxMiddle.y, pos.z - halfBox.z);
        Vector3 boxCorner4 = new Vector3(pos.x + halfBox.x, boxMiddle.y, pos.z + halfBox.z);
        //Find the smallest ground angle below the player
        float angle = GroundAngle(boxMiddle, box.size.y);
        angle = Mathf.Min(angle, GroundAngle(boxCorner1, box.size.y));
        angle = Mathf.Min(angle, GroundAngle(boxCorner2, box.size.y));
        angle = Mathf.Min(angle, GroundAngle(boxCorner3, box.size.y));
        angle = Mathf.Min(angle, GroundAngle(boxCorner4, box.size.y));
        //Update the ground slope
        GroundSlope = angle;
    }

    //Determines the angle of any ground below the player
    private float GroundAngle(Vector3 position, float rayLength)
    {
        Ray ray = new Ray(position, -transform.up);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, rayLength + 0.01f, -1, QueryTriggerInteraction.Ignore))
            return float.MaxValue; //Nothing hit, so return float max
        return Vector3.Angle(hit.normal, transform.up);
    }

}
