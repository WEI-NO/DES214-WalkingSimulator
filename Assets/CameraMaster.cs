using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Diagnostics;

public enum CinematicSequence
{
    GameStart
}

public class CameraMaster : MonoBehaviour
{
    public static CameraMaster instance;

    private const float DistanceToReturn = 0.01f;
    private const float ReturnSpeed = 0.02f;

    public Camera playerCamera;
    public Camera cinematicCamera;

    public Animator cameraCinematicAnimator;

    private void Start()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);

        if (cinematicCamera) cinematicCamera.gameObject.SetActive(false);
    }

    public static void PlayCinematic(CinematicSequence sequence)
    {
        instance.EnableCinematicCamera();

        if (instance.cameraCinematicAnimator)
        {
            instance.cameraCinematicAnimator.enabled = true;
            instance.cameraCinematicAnimator.SetTrigger(sequence.ToString());
        } else
        {
            instance.EnablePlayerCamera();
        }
    }

    public void EnableCinematicCamera()
    {
        playerCamera.gameObject.SetActive(false);
        cinematicCamera.gameObject.SetActive(true);
    }

    public void EnablePlayerCamera()
    {
        playerCamera.gameObject.SetActive(true);
        cinematicCamera.gameObject.SetActive(false);
    }

    public void LerpToPlayerCamera()
    {
        cameraCinematicAnimator.enabled = false;
        StartCoroutine(SmoothLerpToPlayerCamera());
    }

    private IEnumerator SmoothLerpToPlayerCamera()
    {
        float distance = Vector3.Distance(playerCamera.transform.position, cinematicCamera.transform.position);

        while (distance > DistanceToReturn && GameSequence.GamePaused)
        {
            Vector3 lerped = Lerp(cinematicCamera.transform.position, playerCamera.transform.position, ReturnSpeed);
            cinematicCamera.transform.position = lerped;
            yield return null;
            distance = Vector3.Distance(playerCamera.transform.position, cinematicCamera.transform.position);
        }

        EnablePlayerCamera();
    }

    private Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        
        return a + (b - a) * t;
    }

}
