/*******************************************************************************
File:      SpecialOccluder.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/16/2022

Description:
    This component is added to any object that should be made semi-transparent
    when occluding the player.

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialOccluder : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////
    // DESIGNER VARIABLES
    ////////////////////////////////////////////////////////////////////////////

    //The standard material for the object
    private Material UsualMaterial;
    //The material to switch to when occluding the player
    public Material TransparentMaterial;

    ////////////////////////////////////////////////////////////////////////////

    //Whether the object is currently occluding the camera
    private bool IsOccluding = false;
    public void SetOccluded() { IsOccluding = true; }

    //Start is called once when the object is created
    void Start()
    {
        //Save the usual material so it can be switched back when needed
        UsualMaterial = GetComponent<MeshRenderer>().material;
    }

    //Fixed update is called once per physics update
    void FixedUpdate()
    {
        //Switch the material to transparent, if needed
        if (IsOccluding)
            GetComponent<MeshRenderer>().material = TransparentMaterial;
        else //Set it back to the usual material
            GetComponent<MeshRenderer>().material = UsualMaterial;

        //Reset the occluding state--this has to be set every frame that
        //occlusion is occurring.
        IsOccluding = false;
    }
}
