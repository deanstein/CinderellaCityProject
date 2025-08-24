using UnityEngine;

/// <summary>
/// Various game modes or app states that other scripts subscribe to
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

    // if true, guided tour will be enabled automatically when the game loads
    /** this may be overwritten by startup config **/
    public static bool autoStart = false;
    // if true, guided tour will time-travel automatically after some period
    /** this may be overwritten by startup config **/
    public static bool autoTimeTravelPeriodic = false;
    public static bool isPeriodicTimeTraveling = false;
    // if true, guided tour will time-travel automatically for a few seconds after each photo is seen
    /** this may be overwritten by startup config **/
    public static bool autoTimeTravelPeek = false;
    public static bool isTimeTravelPeeking = false;

    // guided tour causes player to walk and face certain paths, led by its agent
    /** this may be overwritten by startup config **/
    public static bool isGuidedTourActive = false;
    // marked true if a player has taken control of FPSController during guided tour
    // suspension will end automatically after some idle time
    public static bool isGuidedTourPaused = false;

    public static bool isTraversingNavMeshLink = false;

    // shuffle guided tour destinations
    /** this may be overwritten by startup config **/
    public static bool shuffleDestinations = false;

    /** keep track of guided tour coroutines **/
    /** to ensure only one is active, and allow canceling them if necessary **/
    // used for attempting re-path after a few seconds when current path is invalid
    public static Coroutine setAgentOnPathAfterDelayCoroutine = null;

    // when guided tour is active, historic photos and people
    // are toggled on or off when agent is close to destination photo
    // FollowGuidedTour.cs looks for these and adjusts object visibility accordingly
    public static bool areHistoricPhotosVisible = false;
    public static bool areHistoricPhotosRequestedVisible = true; // visible by default
    public static bool arePeopleVisible = false;
    public static bool arePeopleRequestedVisible = true; // visible by default

    // time-traveling label is shown right before and during a time-travel transition
    public static bool doShowTimeTravelingLabel = false;

    //
    // INPUT
    //

    // whether to invert the right stick on the controller
    public static bool invertYAxis = false;
}

