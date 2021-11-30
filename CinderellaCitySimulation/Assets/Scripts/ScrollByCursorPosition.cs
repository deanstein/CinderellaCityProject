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

    static readonly float scrollSpeed = 20.0f;
    static readonly float scrollPosThreshold = 0.85f; // normalized threshold at which scrolling starts

    private void Start()
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

        switch (scrollDirection)
        {
            case string scrollDir when scrollDirection.Contains("x")
            || scrollDirection.Contains("horizontal"):
                // horizontal scroll - to the right
                if (relativeMousePosition.x > scrollPosThreshold && thisScrollRect.horizontal && thisScrollRect.horizontalNormalizedPosition < 1.0f)
                {
                    Vector2 pos = new Vector2(Mathf.Sin(Time.time * 10f) * scrollSpeed, 0f);
                    thisScrollRect.content.localPosition = pos;
                }
                // horizontal scroll - to the left
                if (relativeMousePosition.x < (1 - scrollPosThreshold) && thisScrollRect.horizontal && thisScrollRect.horizontalNormalizedPosition > 0f)
                {
                    Vector2 pos = new Vector2(-Mathf.Sin(Time.time * 10f) * scrollSpeed, 0f);
                    thisScrollRect.content.localPosition = pos;
                }
                return;

            case string scrollDir when scrollDirection.Contains("y")
            || scrollDirection.Contains("vertical"):

                // vertical scroll - down
                if ((relativeMousePosition.y > scrollPosThreshold) && thisScrollRect.vertical)
                {
                    Vector2 pos = new Vector2(-Mathf.Sin(Time.time * 10f) * scrollSpeed, 0f);
                    thisScrollRect.content.localPosition = pos;
                }
                // vertical scroll - up
                if (relativeMousePosition.y < (1 - scrollPosThreshold) && thisScrollRect.vertical)
                {
                    Vector2 pos = new Vector2(Mathf.Sin(Time.time * 10f) * scrollSpeed, 0f);
                    thisScrollRect.content.localPosition = pos;
                }
                return;
        }
    }
}