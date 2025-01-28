using UnityEngine;

/// <summary>
/// Handles gameplay modes like Guided Tour
/// </summary>

public static class ModeState
{
    //
    // ANTI-GRAVITY MODE
    //

    public static bool isAntiGravityModeActive = false;

    //
    // GUIDED TOUR
    // 

    // guided tour causes player to walk and face certain paths, led by its agent
    public static bool isGuidedTourActive = false;
    // marked true if a player has taken control of FPSController during guided tour
    // suspension will end automatically after some idle time
    public static bool isGuidedTourPaused = false;

    // keep track of guided tour coroutines 
    // to ensure only one is active, and allow canceling them if necessary
    public static Coroutine restartGuidedTourCoroutine = null;
    public static Coroutine toggleToNextEraCoroutine = null;
    public static Coroutine setAgentOnPathAfterDelayCoroutine = null;

    // when guided tour is active, historic photos and people
    // are toggled on or off when agent is close to destination photo
    // FollowGuidedTour.cs looks for these and adjusts object visibility accordingly
    public static bool areHistoricPhotosRequestedVisible = false;
    public static bool arePeopleRequestedVisible = true;

    // whether to invert the right stick on the controller
    public static bool invertYAxis = true;
}

