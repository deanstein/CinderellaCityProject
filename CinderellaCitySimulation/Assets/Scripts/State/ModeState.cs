using UnityEngine;

/// <summary>
/// Handles gameplay modes like Guided Tour
/// </summary>

public static class ModeState
{
    //
    // GUIDED TOUR
    // 

    // guided tour causes player to be walk and face certain paths, led by its agent
    public static bool isGuidedTourActive = false;
    // marked true if a player has taken control of FPSController during guided tour
    // suspension will end automatically after some idle time
    public static bool isGuidedTourPaused = false;

    // keep track of guided tour coroutines 
    // to ensure only one is active, and allow canceling them if necessary
    public static Coroutine restartGuidedTourCoroutine = null;
    public static Coroutine toggleToNextEraCoroutine = null;
}

