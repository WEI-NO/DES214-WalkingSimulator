/*******************************************************************************
File:      CameraOcclusion.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/16/2022

Description:
    This component is added to the camera to detect and resolve occlusion and
    regulate camera zoom.

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOcclusion : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////
    // DESIGNER VARIABLES
    ////////////////////////////////////////////////////////////////////////////

    //Default zoom distance
    private float DefaultZoom = 20.0f; //THIS VALUE IS WAY TOO LARGE AND SHOULD BE IN THE 10 TO 20 RANGE
    //Getter and setter so this can be manipulated by a camera hint
    public float GetDefaultZoom() { return DefaultZoom; }
    public void SetDefaultZoom(float distance) { DefaultZoom = distance; }

    //Maximum zoom distance
    private float MaxZoom = 1000.0f;
    //Zoom distance at the player becomes transparent
    private float TransparentZoom = 0.45f;
    //Minimum zoom distance
    private float MinZoom = 0.2f;
    //Zoom in speed
    private float ZoomInSpeed = 10.0f;
    //Zoom out speed (should be a lot slower than zoom in speed)
    private float ZoomOutSpeed = 1.0f;
    //Ray casting offset, should be about half the width of the player
    private float HorizontalOffsetDistance = 0.5f;
    //Ray casting rotation, should be between 5.0f and 30.0f degrees
    private float HorizontalRotationAngle = 10.0f;
    //Ray casting upwards rotation, should be at least 15.0f, could be up to 60.0f
    private float UpwardRotationAngle = 30.0f;
    //Ray casting downwards rotation, should be fairly small, otherwise the camera
    //will zoom in close just because there is a normal floor
    private float DownwardsRotationAngle = 5.0f;
    //Buffer from any occluding objects when calculating zoom
    //This is so the camera isn't too close to the occluding object
    private float ZoomBuffer = 0.5f;

    ////////////////////////////////////////////////////////////////////////////

    //Mask applied to occlusion raycast
    //Generally should just be left to the default
    public LayerMask OcclusionMask;
    //Reference to the MovePivot
    private GameObject MovePivot;
    //Whether to do debug ray drawing
    public bool DoDebug = false;

    //Start is called before the first frame update
    void Start()
    {
        //Save a reference to the move pivot so we don't have to find it every frame
        MovePivot = GameObject.Find("MovePivot");
        //Set initial zoom to maximum (the camera is in the negative z direction from the player)
        transform.localPosition = new Vector3(0.0f, 0.0f, -DefaultZoom);
    }

    //Fixed update is called once per physics update
    void FixedUpdate()
    {
        //Cast rays from the player to the camera to see if something is occluding
        float newZoom = CastRayFromPlayer(); //Direct ray to camera
        //Slightly offset rays to catch occluding objects even if they are only occluding part of the player
        newZoom = Mathf.Min(newZoom, CastRayFromPlayer(0.0f, 0.0f, HorizontalOffsetDistance)); //Ray offset to the right
        newZoom = Mathf.Min(newZoom, CastRayFromPlayer(0.0f, 0.0f, -HorizontalOffsetDistance)); //Ray offset to the left
        //Rays angled slightly right and left to catch incoming occluding objects on the sides
        newZoom = Mathf.Min(newZoom, CastRayFromPlayer(HorizontalRotationAngle, 0.0f)); //Ray angled right
        newZoom = Mathf.Min(newZoom, CastRayFromPlayer(-HorizontalRotationAngle, 0.0f)); //Ray angled left
        //Rays angled up and down to catch incoming occluding ceilings and floors
        newZoom = Mathf.Min(newZoom, CastRayFromPlayer(0.0f, UpwardRotationAngle)); //Ray angled up
        newZoom = Mathf.Min(newZoom, CastRayFromPlayer(0.0f, -DownwardsRotationAngle)); //Ray angled down

        //Make the player model and the face cube transparent if we are close
        if (newZoom < TransparentZoom)
        {
            SpecialOccluder occluder = GameObject.Find("PlayerModel").GetComponent<SpecialOccluder>();
            if (occluder != null)
                occluder.SetOccluded();
            occluder = GameObject.Find("Cube").GetComponent<SpecialOccluder>();
            if (occluder != null)
                occluder.SetOccluded();
        }

        //Don't zoom in too close
        newZoom = Mathf.Max(newZoom, MinZoom);
        //Don't zoom in too far out
        newZoom = Mathf.Min(newZoom, MaxZoom);

        //Get the current camera position
        Vector3 currentCameraPosition = transform.localPosition;
        //Get the new camera position, with a y offset based on the zoom value
        //(so the player isn't in the center of the screen)
        Vector3 newCameraPosition = new Vector3(0.0f, newZoom / 5.0f, -newZoom);
        //If the new zoom value is less than the current zoom value and the default zoom value, zoom in fast
        if (Mathf.Abs(currentCameraPosition.z) > newZoom && Mathf.Abs(currentCameraPosition.z) <= DefaultZoom)
            transform.localPosition = Vector3.Lerp(currentCameraPosition, newCameraPosition, ZoomInSpeed * Time.deltaTime);
        else //Otherwise go slow
            transform.localPosition = Vector3.Lerp(currentCameraPosition, newCameraPosition, ZoomOutSpeed * Time.deltaTime);
    }

    //Cast a ray from the player to the camera with the given offsets and rotations
    float CastRayFromPlayer(float horizontalRotation = 0.0f, float verticalRotation = 0.0f, float horizontalOffset = 0.0f, float verticalOffset = 0.0f)
    {
        //Use the move pivot as the player position, as that is the position of the player's "eyes"
        Vector3 playerPos = MovePivot.transform.position;
        //Remove any local x and y offsets from the camera position
        Vector3 cameraPos = new Vector3(transform.position.x - transform.localPosition.x, transform.position.y - transform.localPosition.y, transform.position.z);
        //Get a vector from the player to the camera
        Vector3 playerToCamera = (cameraPos - playerPos).normalized;
        //Any offsets are applied to the player position (i.e., where the ray is case from)
        playerPos = OffsetVector(playerPos, playerToCamera, horizontalOffset, verticalOffset);
        //Any rotations are applied to the direction the ray is facing
        playerToCamera = RotateVector(playerToCamera, horizontalRotation, verticalRotation);

        if (DoDebug)
            Debug.DrawRay(playerPos, playerToCamera * 5.0f, Color.red);

        //Get a ray starting at the offset player position with the rotated direction
        Ray occlusionRay = new Ray(playerPos, playerToCamera);
        //Use the physics engine to cast a ray out to the default zoom distance
        //Note that the occlusion mask allows us to exclude classes of objects from this cast
        RaycastHit[] results = Physics.RaycastAll(occlusionRay, DefaultZoom + ZoomBuffer, OcclusionMask, QueryTriggerInteraction.Ignore);

        //If nothing was hit, just return the default zoom
        if (results.Length == 0)
            return DefaultZoom;

        //Start at the default zoom
        float closestDistance = DefaultZoom + ZoomBuffer;
        //Then find any closer hits
        foreach (RaycastHit result in results)
        {
            //Determine whether each hit has a the special occluder component
            SpecialOccluder occluder = result.transform.GetComponent<SpecialOccluder>();
            if (occluder != null) //It does, so tell the object it is occluding (but don't zoom in)
                occluder.SetOccluded();
            else //It doesn't, so zoom in more if this object is closer than the closest object so far
                closestDistance = Mathf.Min(closestDistance, result.distance);
        }

        //Return the distance minus the zoom buffer
        return closestDistance - ZoomBuffer;
    }

    //Offset a vector either horizontally, vertically, or both
    Vector3 OffsetVector(Vector3 vectorToOffset, Vector3 vectorToOffsetRelativeTo, float horizontalOffset = 0.0f, float verticalOffset = 0.0f)
    {
        //Don't bother if the offsets are 0.0f
        if (horizontalOffset == 0.0f && verticalOffset == 0.0f)
            return vectorToOffset;

        //Get a vector at a right angle to the vector we are offsetting relative to
        Vector3 horizontalOffsetVector = Quaternion.AngleAxis(90.0f, Vector3.up) * vectorToOffsetRelativeTo;
        //Remove the y value, so the vector doesn't end up tilted
        horizontalOffsetVector.y = 0;
        //Normalize and multiply by the offset amount
        horizontalOffsetVector = horizontalOffsetVector.normalized * horizontalOffset;
        //Apply the offset to the vector we are offsetting
        vectorToOffset += horizontalOffsetVector;
        //Apply the vertical offset last (simpler because the player cannot rotate up or down)
        vectorToOffset.y += verticalOffset;

        return vectorToOffset;
    }

    //Rotate a vector either horizontally, vertically, or both
    Vector3 RotateVector(Vector3 vectorToRotate, float horizontalRotation = 0.0f, float verticalRotation = 0.0f)
    {
        //Rotate around the y axis
        if (horizontalRotation != 0.0f)
            vectorToRotate = Quaternion.AngleAxis(horizontalRotation, Vector3.up) * vectorToRotate;

        //Rotate around the cross product of the y axis and the vector we were given
        if (verticalRotation != 0.0f)
            vectorToRotate = Quaternion.AngleAxis(verticalRotation, Vector3.Cross(vectorToRotate, Vector3.up)) * vectorToRotate;

        return vectorToRotate;
    }

}
