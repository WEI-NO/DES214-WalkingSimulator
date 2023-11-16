using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSequence : MonoBehaviour
{
    public static bool GamePaused = false;
    public static System.Action GamePause;
    public static System.Action GameResume;

    private const float StartSequenceWaitTime = 3.0f;
    private const float StartSequenceCinematicWaitTime = 6.0f;

    public bool enableGameStartSequence = true;

    [Header("Game Start Sequence")]
    public Animator gameCurtainAnimator;

    private void Start()
    {
        if (enableGameStartSequence)
        {
            StartCoroutine(GameStartSequence());
        }
    }

    private IEnumerator GameStartSequence()
    {
        PauseGame();
        if (gameCurtainAnimator)
        {
            gameCurtainAnimator.SetTrigger("GameStart");
        }
        yield return new WaitForSeconds(StartSequenceWaitTime - 0.75f);

        CameraMaster.PlayCinematic(CinematicSequence.GameStart);

        yield return new WaitForSeconds(StartSequenceCinematicWaitTime);
        ResumeGame();
    
    }

    public static void PauseGame()
    {
        GamePaused = true;
        GamePause?.Invoke();
    }

    public static void ResumeGame()
    {
        GamePaused = false;
        GameResume?.Invoke();
    }
}
