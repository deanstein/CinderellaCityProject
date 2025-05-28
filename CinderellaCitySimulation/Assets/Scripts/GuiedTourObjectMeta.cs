public struct GuidedTourObjectMeta
{
    public string partialName;
    public bool doTimeTravelPeek;
    public bool doTimeTravelPeriodic;

    public GuidedTourObjectMeta(
        string partialName, 
        bool doTimeTravelPeek, 
        bool doTimeTravelPeriodic)
    {
        this.partialName = partialName;
        this.doTimeTravelPeek = doTimeTravelPeek;
        this.doTimeTravelPeriodic = doTimeTravelPeriodic;
    }
}