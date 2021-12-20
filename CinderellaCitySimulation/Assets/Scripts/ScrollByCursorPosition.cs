using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// When attached to a scroll area, watches the cursor position and 
/// attempts to scroll when the cursor is near the scroll area edges
/// </summary>

// attach this script to an object that needs to follow the player
public class ScrollByCursorPosition : MonoBehaviour
{
    public string scrollDirection;
    static ScrollRect thisScrollRect;
    static RectTransform thisRect;

    static readonly float maxScrollSpeed = 50.0f;
    // normalized distance from rect edge at which scrolling starts
    static readonly float scrollPosThresholdLower = 0.25f; 
    static readonly float scrollPosThresholdUpper = 1 - scrollPosThresholdLower;

    private void OnEnable()
    {
        thisScrollRect = this.GetComponent<ScrollRect>();
        thisRect = thisScrollRect.GetComponent<RectTransform>();
    }

    void Update()
    {
        ScrollContentsByCursorPosition(scrollDirection);
    }

    public static void ScrollContentsByCursorPosition(string scrollDirection)
    {
        Vector2 localMousePosition = thisRect.InverseTransformPoint(Input.mousePosition);
        Vector2 relativeMousePosition = new Vector2((localMousePosition.x - thisRect.rect.x) / thisRect.rect.width, (localMousePosition.y - thisRect.rect.y) / thisRect.rect.height);

        // conditions that must be met for scrolling to occur
        bool isHorizontalScrollingEnabled = thisScrollRect.horizontal;
        bool isCursorWithinScrollRect = relativeMousePosition.x > 0.0f
            && relativeMousePosition.x < 1.0f
            && relativeMousePosition.y > 0.0f
            && relativeMousePosition.y < 1.0f;
        bool isCursorPastUpperThreshold = relativeMousePosition.x > scrollPosThresholdUpper;
        bool isCursorPastLowerThreshold = relativeMousePosition.x < scrollPosThresholdLower;
        bool isScrollAreaScrollableToRight = thisScrollRect.horizontalNormalizedPosition < 1.0f;
        bool isScrollAreaScrollableToLeft = thisScrollRect.horizontalNormalizedPosition > 0.0f;

        switch (scrollDirection)
        {
            case string scrollDir when scrollDirection.Contains("x")
            || scrollDirection.Contains("horizontal"):
                // horizontal scroll - to the right
                if (isHorizontalScrollingEnabled
                    && isCursorWithinScrollRect
                    && isCursorPastUpperThreshold
                    && isScrollAreaScrollableToRight)
                {
                    float remappedSpeed = Utils.MathUtils.RemapRange(relativeMousePosition.x, scrollPosThresholdUpper, 1.0f, 0.0f, maxScrollSpeed);

                    Vector2 pos = new Vector2(thisScrollRect.content.localPosition.x - remappedSpeed, thisScrollRect.content.localPosition.y);
                    thisScrollRect.content.localPosition = pos;
                }
                // horizontal scroll - to the left
                if (isHorizontalScrollingEnabled
                    && isCursorWithinScrollRect
                    && isCursorPastLowerThreshold
                    && isScrollAreaScrollableToLeft)
                {
                    float remappedSpeed = Utils.MathUtils.RemapRange(relativeMousePosition.x, 0.0f, scrollPosThresholdLower, maxScrollSpeed, 0.0f);

                    Vector2 pos = new Vector2(thisScrollRect.content.localPosition.x + remappedSpeed, thisScrollRect.content.localPosition.y);
                    thisScrollRect.content.localPosition = pos;
                }
                return;

            case string scrollDir when scrollDirection.Contains("y")
            || scrollDirection.Contains("vertical"):

                // vertical scroll - down (not implemented yet)

                // vertical scroll - up (not implemented yet)

                return;
        }
    }
}