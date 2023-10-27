/*******************************************************************************
File:      CameraHint.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/16/2022

Description:
    Overrides camera parameters within a trigger region.

*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHint : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////
    // DESIGNER VARIABLES
    ////////////////////////////////////////////////////////////////////////////

    //Values to override the camera with
    public float MaxPitchUp = 85.0f;
    public float MaxPitchDown = 85.0f;
    public float DefaultPitchAngle = 0.0f;
    public float DefaultZoom = 100.0f;

    ////////////////////////////////////////////////////////////////////////////

    //Save the old values so they can be restored
    private float OldMaxPitchUp;
    private float OldMaxPitchDown;
    private float OldDefaultPitchAngle;
    private float OldDefaultZoom;
  
    //Override the camera and the pitch values
    private void OnTriggerEnter(Collider other)
    {
        //Not the player, so never mind...
        if (other.gameObject.name != "Player")
            return;

        //Save the default zoom and override it with the hint's value
        CameraOcclusion camera = GameObject.Find("PlayerCamera").GetComponent<CameraOcclusion>();
        OldDefaultZoom = camera.GetDefaultZoom();
        camera.SetDefaultZoom(DefaultZoom);

        //Save the pitch angles and override them with the hint's values
        CameraPitch pitchPivot = GameObject.Find("PitchPivot").GetComponent<CameraPitch>();
        OldDefaultPitchAngle = pitchPivot.GetDefaultPitchAngle();
        OldMaxPitchUp = pitchPivot.GetMaxPitchUp();
        OldMaxPitchDown = pitchPivot.GetMaxPitchDown();
        pitchPivot.SetDefaultPitchAngle(DefaultPitchAngle);
        pitchPivot.SetMaxPitchUp(MaxPitchUp);
        pitchPivot.SetMaxPitchDown(MaxPitchDown);
    }

    //Restore the camera and the pitch values
    private void OnTriggerExit(Collider other)
    {
        //Not the player, so never mind...
        if (other.gameObject.name != "Player")
            return;

        //Restore the default zoom
        CameraOcclusion camera = GameObject.Find("PlayerCamera").GetComponent<CameraOcclusion>();
        camera.SetDefaultZoom(OldDefaultZoom);

        //Restore the pitch angles
        CameraPitch pitchPivot = GameObject.Find("PitchPivot").GetComponent<CameraPitch>();
        pitchPivot.SetDefaultPitchAngle(OldDefaultPitchAngle);
        pitchPivot.SetMaxPitchUp(OldMaxPitchUp);
        pitchPivot.SetMaxPitchDown(OldMaxPitchDown);
    }

}
