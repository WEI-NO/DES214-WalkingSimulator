/*******************************************************************************
File:      CameraPitch.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/16/2022

Description:
    Handles the pitch (up-down) rotation of a third-person follow camera.
    This script is attached to the pitch pivot object, which must be parented
    to the move pivot (which must be in turn parents to the yaw pivot and the
    player) for it to work. There must also be a "PlayerModel" object
    which is also parented to the player. Note that we cannot use the player
    object itself for this, because rotating the player object would also
    rotate the pitch pivot, which would defeat the whole purpose of having a
    separate pitch pivot (even though currently the player doesn't rotate up/down).

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPitch : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////
    // DESIGNER VARIABLES
    ////////////////////////////////////////////////////////////////////////////

    //Interpolant value (which will be multipled by delta time) for the
    //amount to rotate the pitch pivot towards the desired pitch every frame
    private float PitchInterpolation = 100.0f; //THIS VALUE IS WAY TOO HIGH AND SHOULD BE IN THE SINGLE DIGITS

    //The minimum amount to interpolate in a single frame (which will be
    //multiplied by delta time), to prevent the interpolation from taking
    //a long time to finish when the player stops.
    private float MinInterpolation = 100.0f; //THIS VALUE IS WAY TOO HIGH AND SHOULD BE IN THE LOW SINGLE DIGITS

    //The default pitch angle when the terrain does not dictate
    //a higher or lower angle.
    private float DefaultPitchAngle = 15.0f;
    //Getter and setter so a camera hint can access it
    public float GetDefaultPitchAngle() { return DefaultPitchAngle; }
    public void SetDefaultPitchAngle(float angle) { DefaultPitchAngle = angle; }

    //The maximum amount allowed to pitch up or down. Using camera hints
    //to change these limits in different locations often works well.
    private float MaxPitchUp = 85.0f; //Lower this to keep the camera from looking up to far (can even be negative)
    //Getter and setter so a camera hint can access it
    public float GetMaxPitchUp() { return MaxPitchUp; }
    public void SetMaxPitchUp(float angle) { MaxPitchUp = angle; }

    private float MaxPitchDown = 5.0f; //Lower this to keep the camera from looking down to far (can even be negative)
    //Getter and setter so a camera hint can access it
    public float GetMaxPitchDown() { return MaxPitchDown; }
    public void SetMaxPitchDown(float angle) { MaxPitchDown = angle; }

    //Distance ahead of the player to start checking terrain for pitch purposes.
    //This shouldn't be too short, as pitch generally shouldn't be changed based
    //on the terrain really close to the player.
    private float TerrainCheckStart = 4.0f;
    //Distance ahead of the player to stop checking terrain for pitch purposes.
    //This should be too long, as pitch generally shouldn't be changed based
    //on the terrain a long ways from the player (use a camera hint instead when
    //you want to alter the pitch based on something far away).
    private float TerrainCheckEnd = 10.0f;
    //Size of the increments of distance to check. Should generally be 1.0 or less
    //in order to not miss thinner terrain objects
    private float TerrainCheckIncrement = 0.5f;

    ////////////////////////////////////////////////////////////////////////////

    //The transform of the player model (not the base player object),
    //which is used for calculating the desired pitch
    private Transform PlayerModel;

    //Start is called before the first frame update
    void Start()
    {
        //Save the player model transform
        PlayerModel = GameObject.Find("PlayerModel").transform;
    }

    //Fixed update is called once per physics update
    void FixedUpdate()
    {
        //Calculate the desired pitch by checking to see what the
        //terrain is like in front of the player
        float desiredPitch = 360.0f;
        for (float distance = TerrainCheckStart; distance <= TerrainCheckEnd; distance += TerrainCheckIncrement)
        {
            float angle = PitchCheck(distance);
            if (Mathf.Abs(angle - DefaultPitchAngle) < Mathf.Abs(desiredPitch - DefaultPitchAngle))
                desiredPitch = angle;
        }

        //Interpolate the pitch to the angle caclulated above
        InterpolatePitch(desiredPitch);
    }

    //Interpolate the pitch pivot towards the desired pitch angle
    private void InterpolatePitch(float desiredPitch)
    {
        //Hard clamp the desired pitch to the minimum and maximum
        desiredPitch = Mathf.Clamp(desiredPitch, -MaxPitchUp, MaxPitchDown);

        //Get the difference between the current camera pitch and the desired pitch,
        //making sure the angle is between -180 and +180 degrees
        float pitchDiff = CorrectAngle(desiredPitch - transform.localEulerAngles.x);

        //Calculate the amount to rotate the pitch pivot this frame,
        //making sure it is at least the minimum rotation amount
        float rotationAmount = MinimumAngle(pitchDiff * PitchInterpolation * Time.deltaTime, MinInterpolation * Time.deltaTime);
        //Decrease the rotation amount to the maximum, if needed
        rotationAmount = MaximumAngle(rotationAmount, pitchDiff);

        //Calculate the new angle for the pitch pivot,
        //making sure the angle is between -180 and +180 degrees
        float newPitch = CorrectAngle(transform.localEulerAngles.x + rotationAmount);

        //Set the pitch pivot to it's new rotation
        transform.localRotation = Quaternion.Euler(newPitch, 0.0f, 0.0f);
    }

    //How far down should a ray be cast to detect terrain. Should be fairly
    //long, but doesn't need to be a really large amount.
    private float DownRayDistance = 50.0f; //The default will generally work fine

    //Check for the angle from the player to terrain at a specific distance
    private float PitchCheck(float forwardDistance)
    {
        //Find a vector with a clear line of sight in front of the player
        Vector3 forwardVector = GetForwardVector(PlayerModel.position, PlayerModel.forward, PlayerModel.up, forwardDistance);

        //Couldn't find a clear line of sight, so just return the default pitch angle
        if (forwardVector == Vector3.zero)
            return DefaultPitchAngle;

        //How far down are we going to look?
        var downCastDistance = DownRayDistance;
        //Look down from the point the forward cast collided
        var downCastPoint = PlayerModel.position + forwardVector * forwardDistance;
        //Do the raycast
        Ray downRay = new Ray(downCastPoint, -PlayerModel.up);
        RaycastHit[] downResults = Physics.RaycastAll(downRay, downCastDistance, -1, QueryTriggerInteraction.Ignore);
        //Find the nearest thing hit
        if (downResults.Length > 0) //If any objects are hit
        {
            foreach (RaycastHit result in downResults) //Loop through all objects hit
            {
                if (result.distance < downCastDistance)
                    downCastDistance = result.distance;
            }
        }
        //Determine the relative (to the player) height of the collision
        var relativeGroundHeight = forwardVector.y * forwardDistance - downCastDistance;
        //Remove the vertical component so we can get the horizontal distance
        forwardVector.y = 0.0f;
        //Determine the relative (to the player) distance to the collision
        var relativeGroundDistance = (forwardVector * forwardDistance).magnitude;
        //Return the angle in degrees
        return -180.0f * Mathf.Atan2(relativeGroundHeight, relativeGroundDistance) / Mathf.PI;
    }

    //How steeply should the forward ray cast be biased downwards and upwards to avoid
    //hitting ramps and other blcoking terrain?
    private float MaximumRayBias = 0.5f; //Defaults to a 1:2 bias, 1:1 would be 45 degrees

    //Check to see if we can find a forward vector that is not blocked by terrain
    Vector3 GetForwardVector(Vector3 position, Vector3 forward, Vector3 up, float distance)
    {
        //Check starting with a ray biased down, then incrementally bias it up
        //until we find something (if we can).
        for (float bias = -1.0f; bias <= 1.0f; bias += 0.5f)
        {
            //Get a direction to test
            Vector3 testDirection = (forward + up * MaximumRayBias * bias).normalized;
            //Do the raycast
            Ray ray = new Ray(position, testDirection);
            RaycastHit[] forwardResults = Physics.RaycastAll(ray, distance, -1, QueryTriggerInteraction.Ignore);
            //If we didn't hit anything, this is our vector 
            if (forwardResults.Length == 0)
                return testDirection;
        }
        //Couldn't find a clear line of sight
        return Vector3.zero;
    }

    //Check to see if anything is blocking along this ray
    bool BlockingTerrainCheck(Vector3 position, Vector3 direction, float distance)
    {
        //Do the raycast
        Ray ray = new Ray(position, direction);
        RaycastHit[] forwardResults = Physics.RaycastAll(ray, distance, -1, QueryTriggerInteraction.Ignore);
        if (forwardResults.Length > 0)
            return true; //An object was hit, so something is blocking
        return false; //Nothing blocking
    }

    //Correct the angle to be from -180 degrees to +180 degrees
    float CorrectAngle(float angle)
    {
        if (angle > 180.0f)
            return angle - 360.0f;
        if (angle < -180.0f)
            return angle + 360.0f;
        return angle;
    }

    //Limit the angle so it max or lower for a
    //positive max, and -max or higher for a negative max
    float MaximumAngle(float angle, float max)
    {
        if (max == 0.0f)
            return max;
        if (max > 0.0f && angle > max)
            return max;
        if (max < 0.0f && angle < max)
            return max;
        return angle;
    }

    //Limit the angle so it is min or higher for a
    //positive angle, or -min or lower for a negative angle
    float MinimumAngle(float angle, float min)
    {
        if (angle > 0.0f && angle < min)
            return min;
        if (angle < 0.0f && angle > -min)
            return -min;
        return angle;
    }

}
