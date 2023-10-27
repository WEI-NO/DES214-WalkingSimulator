/*******************************************************************************
File:      CameraYaw.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/16/2022

Description:
    Handles the yaw (left-right) rotation of a third-person follow camera.
    This script is attached to the yaw pivot object, which must be parented
    to the player for it to work. There must also be a "PlayerModel" object
    which is also parented to the player. Note that we cannot use the player
    object itself for this, because rotating the player object would also
    rotate the yaw pivot, which would defeat the whole purpose of having a
    separate yaw pivot.

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraYaw : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////
    // DESIGNER VARIABLES
    ////////////////////////////////////////////////////////////////////////////

    //Interpolant value (which will be multipled by delta time) for the
    //amount to rotate the yaw pivot towards the player model every frame
    private float YawInterpolation = 1000.0f; //THIS VALUE IS WAY TOO HIGH AND SHOULD BE IN THE SINGLE DIGITS

    //The minimum amount to interpolate in a single frame (which will be
    //multiplied by delta time), to prevent the interpolation from taking
    //a long time to finish when the player stops rotating.
    private float MinInterpolation = 1000.0f; //THIS VALUE IS WAY TOO HIGH AND SHOULD BE IN THE LOW SINGLE DIGITS

    ////////////////////////////////////////////////////////////////////////////

    //The transform of the player model (not the base player object),
    //which is the rotation we are interpolating to
    private Transform PlayerModel;

    //Start is called before the first frame update
    void Start()
    {
        //Save the player model transform
        PlayerModel = GameObject.Find("PlayerModel").transform;
        //Make sure the camera starts behind the player
        transform.localRotation = Quaternion.Euler(0.0f, PlayerModel.localRotation.y, 0.0f);
    }

    //Fixed update is called once per physics update
    void FixedUpdate()
    {
        //Get the difference between the current camera yaw and the player model yaw,
        //making sure the angle is between -180 and +180 degrees
        float yawDiff = CorrectAngle(PlayerModel.localEulerAngles.y - transform.localEulerAngles.y);

        //Calculate the amount to rotate the yaw pivot this frame,
        //making sure it is at least the minimum rotation amount
        float rotationAmount = MinimumAngle(yawDiff * YawInterpolation * Time.deltaTime, MinInterpolation * Time.deltaTime);
        //Decrease the rotation amount to the maximum, if needed
        rotationAmount = MaximumAngle(rotationAmount, yawDiff);

        //Calculate the new angle for the yaw pivot
        float newYaw = transform.localEulerAngles.y + rotationAmount;

        //Set the yaw pivot to it's new rotation
        transform.localRotation = Quaternion.Euler(0.0f, newYaw, 0.0f);
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
