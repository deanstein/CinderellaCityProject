﻿public struct GuidedTourCameraMeta
{
    public string partialName;
    public bool doTimeTravelPeek;
    public bool doTimeTravelPeriodic;

    public GuidedTourCameraMeta(
        string partialName, 
        bool doTimeTravelPeek, 
        bool doTimeTravelPeriodic)
    {
        this.partialName = partialName;
        this.doTimeTravelPeek = doTimeTravelPeek;
        this.doTimeTravelPeriodic = doTimeTravelPeriodic;
    }
}