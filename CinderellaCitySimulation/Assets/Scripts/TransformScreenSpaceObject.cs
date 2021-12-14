using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Positions screen space objects (UI) based on the available screen real estate and proportions
/// </summary>

public static class TransformScreenSpaceObject
{
    public static float screenMidPointX = Screen.width / 2;
    public static float screenMidPointY = Screen.height / 2;

    public static float GetObjectTopEdgePositionY(GameObject screenSpaceObject)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectHeight = rectTransform.rect.height * rectTransform.localScale.y;
        //Utils.DebugUtils.DebugLog("Object height: " + imageHeight);

        float objectPositionY = rectTransform.position.y;

        float objectTopEdgePositionY = objectPositionY + (objectHeight / 2);

        return objectTopEdgePositionY;
    }

    public static float GetObjectBottomEdgePositionY(GameObject screenSpaceObject)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectHeight = rectTransform.rect.height * rectTransform.localScale.y;
        //Utils.DebugUtils.DebugLog("Object height: " + objectHeight);

        float objectPositionY = rectTransform.position.y;

        float objectBottomEdgePositionY = objectPositionY - (objectHeight / 2);

        return objectBottomEdgePositionY;
    }

    public static float GetObjectLeftEdgePositionX(GameObject screenSpaceObject)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectWidth = rectTransform.rect.width * rectTransform.localScale.x;
        //Utils.DebugUtils.DebugLog("Object width: " + objectWidth);

        float objectPositionX = rectTransform.position.x;

        float objectLeftEdgePositionX = objectPositionX - (objectWidth / 2);

        return objectLeftEdgePositionX;
    }

    public static float GetObjectRightEdgePositionX(GameObject screenSpaceObject)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectWidth = rectTransform.rect.width * rectTransform.localScale.x;
        //Utils.DebugUtils.DebugLog("Object width: " + objectWidth);

        float objectPositionX = rectTransform.position.x;

        float objectRightEdgePositionX = objectPositionX + (objectWidth / 2);

        return objectRightEdgePositionX;
    }

    // for cases where the contents of a scroll area may need to be resized
    // to encapsulate the last child, resizing when not necessary causes issues
    // so set a flag to determine whether resizing needs to happen or not
    public static bool GetIsChildObjectBeyondParentBoundary(GameObject parentObject, GameObject childObject, string testDirection)
    {
        switch (testDirection)
        {
            case string dir when dir.Contains("down"):

                float parentBottomEdgeYPos = GetObjectBottomEdgePositionY(parentObject);
                float childBottomEdgeYPos = GetObjectBottomEdgePositionY(childObject);

                if (childBottomEdgeYPos < parentBottomEdgeYPos)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case string dir when dir.Contains("right"):

                float parentRightEdgeXPos = GetObjectRightEdgePositionX(parentObject);
                float childRightEdgeXPos = GetObjectRightEdgePositionX(childObject);

                if (childRightEdgeXPos > parentRightEdgeXPos)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            default:
                return false;
        }
    }

    // for scrolling elements like toggle containers and menu nav containers,
    // resize the container if the child is beyond the container's bounds
    // so the scroll area scrolls as expected
    public static void ResizeParentContainerToFitLastChild(GameObject parent, GameObject child, float marginRatio, string direction)
    {
        // determine if resizing is necessary
        bool needsResized = GetIsChildObjectBeyondParentBoundary(parent, child, direction);

        if (needsResized)
        {
            switch (direction)
            {
                case string dir when dir.Contains("down"):
                    ResizeObjectHeightByBufferRatioFromNeighborBottom(parent, child, marginRatio);
                    return;
                case string dir when dir.Contains("right"):
                    ResizeObjectWidthByBufferRatioFromNeighborRight(parent, child, marginRatio);
                    return;
            }
        }
    }

    public static void PositionObjectByHeightRatioFromScreenTop(GameObject screenSpaceObject, float bufferProportion)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectHeight = rectTransform.rect.height * rectTransform.localScale.y;
        //Utils.DebugUtils.DebugLog("Object height: " + imageHeight);

        float objectPositionX = rectTransform.position.x;
        float objectPositionY = rectTransform.position.y;

        float objectTopY = objectPositionY + (objectHeight / 2);
        float objectBuffer = Screen.height - objectTopY;
        //Utils.DebugUtils.DebugLog("Current buffer: " + currentBuffer);

        float requestedBuffer = Screen.height * bufferProportion;
        
        // determine the delta: the number of pixels to move up or down (if negative) to achieve the requested buffer
        float bufferDelta = objectBuffer - requestedBuffer;
        //Utils.DebugUtils.DebugLog("Buffer delta: " + bufferDelta);

        // move the object as required
        Vector3 newObjectPosition = new Vector3(objectPositionX, objectPositionY + bufferDelta, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectByWidthRatioFromScreenLeft(GameObject screenSpaceObject, float bufferProportion)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectWidth = rectTransform.rect.width * rectTransform.localScale.x;
        //Utils.DebugUtils.DebugLog("Object height: " + imageHeight);

        float objectPositionX = rectTransform.position.x;
        float objectPositionY = rectTransform.position.y;

        float objectLeftY = objectPositionX - (objectWidth / 2);
        float objectBuffer = objectLeftY;
        //Utils.DebugUtils.DebugLog("Current buffer: " + currentBuffer);

        float requestedBuffer = Screen.width * bufferProportion;

        // determine the delta: the number of pixels to move up or down (if negative) to achieve the requested buffer
        float bufferDelta = objectBuffer - requestedBuffer;
        //Utils.DebugUtils.DebugLog("Buffer delta: " + bufferDelta);

        // move the object as required
        Vector3 newObjectPosition = new Vector3(objectPositionX - bufferDelta, objectPositionY, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectByHeightRatioFromNeighborTop(GameObject screenSpaceObject, GameObject neighborObject, float bufferProportion)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectHeight = rectTransform.rect.height * rectTransform.localScale.y;
        //Utils.DebugUtils.DebugLog("Object height: " + imageHeight);

        float screenSpaceObjectPosX = rectTransform.position.x;
        float screenSpaceObjectPosY = rectTransform.position.y;

        float screenSpaceObjectTopEdgeY = screenSpaceObjectPosY + (objectHeight / 2);
        float neighborObjectTopEdgeY = GetObjectTopEdgePositionY(neighborObject);

        float objectBuffer = neighborObjectTopEdgeY - screenSpaceObjectTopEdgeY;
        //Utils.DebugUtils.DebugLog("Current buffer: " + currentBuffer);

        float requestedBuffer = Screen.height * bufferProportion;

        // determine the delta: the number of pixels to move up or down (if negative) to achieve the requested buffer
        float bufferDelta = objectBuffer + requestedBuffer;
        //Utils.DebugUtils.DebugLog("Buffer delta: " + bufferDelta);

        // move the object as required
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectPosX, screenSpaceObjectPosY + bufferDelta, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectByWidthRatioFromNeighborLeft(GameObject screenSpaceObject, GameObject neighborObject, float bufferProportion)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectWidth = rectTransform.rect.width * rectTransform.localScale.x;
        //Utils.DebugUtils.DebugLog("Object width: " + objectWidth + " and scale: " + rectTransform.localScale.x);

        float screenSpaceObjectPosX = rectTransform.position.x;
        float screenSpaceObjectPosY = rectTransform.position.y;

        float screenSpaceObjectLeftX = screenSpaceObjectPosX - (objectWidth / 2);
        float neighborObjectLeftX = GetObjectLeftEdgePositionX(neighborObject);

        float objectBuffer = neighborObjectLeftX - screenSpaceObjectLeftX;
        //Utils.DebugUtils.DebugLog("Current buffer: " + objectBuffer);

        float requestedBuffer = Screen.width * bufferProportion;

        // determine the delta: the number of pixels to move left or right (if negative) to achieve the requested buffer
        float bufferDelta = objectBuffer - requestedBuffer;
        //Utils.DebugUtils.DebugLog("Buffer delta: " + bufferDelta);

        // move the object as required
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectPosX + bufferDelta, screenSpaceObjectPosY, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectByWidthRatioFromNeighborRight(GameObject screenSpaceObject, GameObject neighborObject, float bufferProportion)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectWidth = rectTransform.rect.width * rectTransform.localScale.x;
        //Utils.DebugUtils.DebugLog("Object width: " + objectWidth + " and scale: " + rectTransform.localScale.x);

        float screenSpaceObjectPosX = rectTransform.position.x;
        float screenSpaceObjectPosY = rectTransform.position.y;

        float screenSpaceObjectLeftX = screenSpaceObjectPosX - (objectWidth / 2);
        float neighborObjectRightX = GetObjectRightEdgePositionX(neighborObject);

        float objectBuffer = neighborObjectRightX - screenSpaceObjectLeftX;
        //Utils.DebugUtils.DebugLog("Current buffer: " + objectBuffer);

        float requestedBuffer = Screen.width * bufferProportion;

        // determine the delta: the number of pixels to move up or down (if negative) to achieve the requested buffer
        float bufferDelta = objectBuffer + requestedBuffer;
        //Utils.DebugUtils.DebugLog("Buffer delta: " + bufferDelta);

        // move the object as required
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectPosX + bufferDelta, screenSpaceObjectPosY, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectByHeightRatioFromNeighborBottom(GameObject screenSpaceObject, GameObject neighborObject, float bufferProportion)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectHeight = rectTransform.rect.height * rectTransform.localScale.y;
        //Utils.DebugUtils.DebugLog("Object height: " + objectHeight);

        float screenSpaceObjectPosX = rectTransform.position.x;
        float screenSpaceObjectPosY = rectTransform.position.y;

        float screenSpaceObjectTopEdgeY = screenSpaceObjectPosY + (objectHeight / 2);
        //Utils.DebugUtils.DebugLog("Object top edge Y: " + screenSpaceObjectTopEdgeY);

        float neighborObjectBottomEdgeY = GetObjectBottomEdgePositionY(neighborObject);
        //Utils.DebugUtils.DebugLog("Neight object bottom edge Y: " + neighborObjectBottomEdgeY);

        float objectBuffer = neighborObjectBottomEdgeY - screenSpaceObjectTopEdgeY;
        //Utils.DebugUtils.DebugLog("Current buffer: " + objectBuffer);

        float requestedBuffer = Screen.height * bufferProportion;

        // determine the delta: the number of pixels to move up or down (if negative) to achieve the requested buffer
        float bufferDelta = objectBuffer - requestedBuffer;
        //Utils.DebugUtils.DebugLog("Buffer delta: " + bufferDelta);

        float newPosY = screenSpaceObjectPosY + bufferDelta;
        //Utils.DebugUtils.DebugLog("New pos Y : " + newPosY);

        // move the object as required
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectPosX, newPosY, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectAtCenterofScreen(GameObject screenSpaceObject)
    {
        // get the position at mid-width and mid-height of the screen
        float newPositionX = Screen.width / 2;
        float newPositionY = Screen.height / 2;

        // move the object so it's centered
        Vector3 newObjectPosition = new Vector3(newPositionX, newPositionY, 0);

        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectAtCenterofScreenWidth(GameObject screenSpaceObject)
    {
        // get the position at mid-width and mid-height of the screen
        float newPositionX = Screen.width / 2;

        // move the object so it's centered
        Vector3 newObjectPosition = new Vector3(newPositionX, screenSpaceObject.transform.position.y, 0);

        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectAtHorizontalCenterlineOfNeighbor(GameObject screenSpaceObject, GameObject alignmentObject)
    {
        // get the neighbor's position
        Vector3 neighborPosition = alignmentObject.transform.position;

        // move the object so it's centered relative to the neighbor
        Vector3 newObjectPosition = new Vector3(screenSpaceObject.transform.position.x, neighborPosition.y, 0);

        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        rectTransform.position = newObjectPosition;
    }

    public static void PositionObjectAtCenterpointOfNeighbor(GameObject screenSpaceObject, GameObject alignmentObject)
    {
        PositionObjectAtVerticalCenterlineOfNeighbor(screenSpaceObject, alignmentObject);
        PositionObjectAtHorizontalCenterlineOfNeighbor(screenSpaceObject, alignmentObject);
    }

    // very similar to centering an object, but accounts for the descender so text is more horizontally centered
    public static void PositionTextAtHorizontalCenterlineOfNeighbor(GameObject textToPosition, GameObject alignmentObject)
    {
        // get the neighbor's position
        Vector3 neighborPosition = alignmentObject.transform.position;

        // get the text descender height
        float textDescenderHeight = GetTextDescenderHeight(textToPosition);

        // move the text so it's centered relative to the neighbor
        // but moved down slightly to account for the text descender
        Vector3 newObjectPosition = new Vector3(textToPosition.transform.position.x, neighborPosition.y - textDescenderHeight, 0);

        RectTransform rectTransform = textToPosition.GetComponent<RectTransform>();
        rectTransform.position = newObjectPosition;
    }

    // get the descender height of the text in pixels
    public static float GetTextDescenderHeight(GameObject textToMeasure)
    {
        RectTransform rectTransform = textToMeasure.GetComponent<RectTransform>();

        float textHeight = rectTransform.rect.height;
        float textDescenderHeight = textHeight * UIGlobals.textDescenderProportion;

        return textDescenderHeight;
    }

    public static void PositionMultiObjectsAtHorizontalCenterlinesOfNeighbors(List<GameObject> screenSpaceObjects, List<GameObject> alignmentObjects)
    {
        for (var i = 0; i < screenSpaceObjects.Count; i++)
        {
            PositionObjectAtHorizontalCenterlineOfNeighbor(screenSpaceObjects[i], alignmentObjects[i]);
        }
    }

    public static void PositionObjectAtVerticalCenterlineOfNeighbor(GameObject screenSpaceObject, GameObject alignmentObject)
    {
        // get the neighbor's position
        Vector3 neighborPosition = alignmentObject.transform.position;

        // move the object so it's centered relative to the neighbor
        Vector3 newObjectPosition = new Vector3(neighborPosition.x, screenSpaceObject.transform.position.y, 0);

        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        rectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectWidthToMatchScreen(GameObject screenSpaceObject)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectWidth = rectTransform.rect.width;

        // redefine the width as the screen width
        rectTransform.sizeDelta = new Vector2 (Screen.width, rectTransform.rect.height);

        // center the object so its new width aligns with the screen borders
        Vector3 newObjectPosition = new Vector3(screenMidPointX, rectTransform.position.y, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectWidthFromCenter(GameObject screenSpaceObject, float screenWidthRatio)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();
        float objectWidth = rectTransform.rect.width;

        // redefine the width as the screen width
        rectTransform.sizeDelta = new Vector2(Screen.width * screenWidthRatio, rectTransform.rect.height);

        // center the object so its new width aligns with the screen borders
        Vector3 newObjectPosition = new Vector3(screenMidPointX, rectTransform.position.y, 0);
        rectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectWidthByScreenWidthRatioTowardRight(GameObject screenSpaceObject, float bufferProportion)
    {
        RectTransform screenSpaceObjectRectTransform = screenSpaceObject.GetComponent<RectTransform>();

        float screenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width;

        float screenSpaceObjectRightEdgePositionX = GetObjectRightEdgePositionX(screenSpaceObject);
        //Utils.DebugUtils.DebugLog("Screen space object right edge position X: " + screenSpceObjectRightEdgePositionX);

        float targetRightEdgePositionX = GetObjectLeftEdgePositionX(screenSpaceObject) + (Screen.width * bufferProportion);
        //Utils.DebugUtils.DebugLog("Alignment object right edge position X: " + neighborObjectRightEdgePositionX);

        float newScreenSpaceObjectRightEdgePositionX = targetRightEdgePositionX + (Screen.width * bufferProportion);
        //Utils.DebugUtils.DebugLog("New screen space object right edge position X: " + newScreenSpaceObjectRightEdgePositionX);

        float newScreenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width - (screenSpaceObjectRightEdgePositionX - newScreenSpaceObjectRightEdgePositionX);
        //Utils.DebugUtils.DebugLog("New screen space object width: " + newScreenSpaceObjectWidth);

        float screenSpaceObjectWidthDelta = newScreenSpaceObjectWidth - screenSpaceObjectWidth;
        screenSpaceObjectRectTransform.sizeDelta = new Vector2(newScreenSpaceObjectWidth, screenSpaceObjectRectTransform.rect.height);

        // the resize happened from the center, so adjust the position to compensate
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectRectTransform.position.x + (screenSpaceObjectWidthDelta / 2), screenSpaceObjectRectTransform.position.y, 0);
        screenSpaceObjectRectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectWidthByBufferRatioFromScreenLeft(GameObject screenSpaceObject, float bufferProportion)
    {
        // first, center the object in the screen, width-wise
        // this is important so that, if the game window is resized, this function doesn't use the old position for its calculations
        PositionObjectAtCenterofScreenWidth(screenSpaceObject);

        RectTransform screenSpaceObjectRectTransform = screenSpaceObject.GetComponent<RectTransform>();

        float screenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width;

        float screenSpaceObjectLeftEdgePositionX = GetObjectLeftEdgePositionX(screenSpaceObject);
        //Utils.DebugUtils.DebugLog("Screen space object left edge position X: " + screenSpaceObjectLeftEdgePositionX);

        float screenLeftEdgePositionX = 0;

        float newScreenSpaceObjectLeftEdgePositionX = screenLeftEdgePositionX + (Screen.width * bufferProportion);
        //Utils.DebugUtils.DebugLog("New screen space object left edge position X: " + newScreenSpaceObjectLeftEdgePositionX);

        float newScreenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width + (screenSpaceObjectLeftEdgePositionX - newScreenSpaceObjectLeftEdgePositionX);
        //Utils.DebugUtils.DebugLog("New screen space object width: " + newScreenSpaceObjectWidth);

        float screenSpaceObjectWidthDelta = newScreenSpaceObjectWidth - screenSpaceObjectWidth;
        screenSpaceObjectRectTransform.sizeDelta = new Vector2(newScreenSpaceObjectWidth, screenSpaceObjectRectTransform.rect.height);

        // the resize happened from the center, so adjust the position to compensate
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectRectTransform.position.x - (screenSpaceObjectWidthDelta / 2), screenSpaceObjectRectTransform.position.y, 0);
        screenSpaceObjectRectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectWidthByBufferRatioFromNeighborLeft(GameObject screenSpaceObject, GameObject neighborObject, float bufferProportion)
    {
        RectTransform screenSpaceObjectRectTransform = screenSpaceObject.GetComponent<RectTransform>();

        float screenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width;

        float screenSpaceObjectLeftEdgePositionX = GetObjectLeftEdgePositionX(screenSpaceObject);
        //Utils.DebugUtils.DebugLog("Screen space object left edge position X: " + screenSpceObjectRightEdgePositionX);

        float neighborObjectLeftEdgePositionX = GetObjectLeftEdgePositionX(neighborObject);
        //Utils.DebugUtils.DebugLog("Alignment object left edge position X: " + neighborObjectRightEdgePositionX);

        float newScreenSpaceObjectLeftEdgePositionX = neighborObjectLeftEdgePositionX - (Screen.width * bufferProportion);
        //Utils.DebugUtils.DebugLog("New screen space object left edge position X: " + newScreenSpaceObjectRightEdgePositionX);

        float newScreenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width + (screenSpaceObjectLeftEdgePositionX - newScreenSpaceObjectLeftEdgePositionX);
        //Utils.DebugUtils.DebugLog("New screen space object width: " + newScreenSpaceObjectWidth);

        float screenSpaceObjectWidthDelta = newScreenSpaceObjectWidth - screenSpaceObjectWidth;
        screenSpaceObjectRectTransform.sizeDelta = new Vector2(newScreenSpaceObjectWidth, screenSpaceObjectRectTransform.rect.height);

        // the resize happened from the center, so adjust the position to compensate
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectRectTransform.position.x - (screenSpaceObjectWidthDelta / 2), screenSpaceObjectRectTransform.position.y, 0);
        screenSpaceObjectRectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectWidthByBufferRatioFromNeighborRight(GameObject screenSpaceObject, GameObject neighborObject, float bufferProportion)
    {
        RectTransform screenSpaceObjectRectTransform = screenSpaceObject.GetComponent<RectTransform>();

        float screenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width;

        float screenSpaceObjectRightEdgePositionX = GetObjectRightEdgePositionX(screenSpaceObject);
        //Utils.DebugUtils.DebugLog("Screen space object right edge position X: " + screenSpceObjectRightEdgePositionX);

        float neighborObjectRightEdgePositionX = GetObjectRightEdgePositionX(neighborObject);
        //Utils.DebugUtils.DebugLog("Alignment object right edge position X: " + neighborObjectRightEdgePositionX);

        float newScreenSpaceObjectRightEdgePositionX = neighborObjectRightEdgePositionX + (Screen.width * bufferProportion);
        //Utils.DebugUtils.DebugLog("New screen space object right edge position X: " + newScreenSpaceObjectRightEdgePositionX);

        float newScreenSpaceObjectWidth = screenSpaceObjectRectTransform.rect.width - (screenSpaceObjectRightEdgePositionX - newScreenSpaceObjectRightEdgePositionX);
        //Utils.DebugUtils.DebugLog("New screen space object width: " + newScreenSpaceObjectWidth);

        float screenSpaceObjectWidthDelta = newScreenSpaceObjectWidth - screenSpaceObjectWidth;
        screenSpaceObjectRectTransform.sizeDelta = new Vector2(newScreenSpaceObjectWidth, screenSpaceObjectRectTransform.rect.height);

        // the resize happened from the center, so adjust the position to compensate
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectRectTransform.position.x + (screenSpaceObjectWidthDelta / 2), screenSpaceObjectRectTransform.position.y, 0);
        screenSpaceObjectRectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectHeightByBufferRatioFromNeighborBottom(GameObject screenSpaceObject, GameObject neighborObject, float bufferProportion)
    {
        RectTransform screenSpaceObjectRectTransform = screenSpaceObject.GetComponent<RectTransform>();

        float screenSpaceObjectHeight = screenSpaceObjectRectTransform.rect.height;
        //Utils.DebugUtils.DebugLog("Screen space object height: " + screenSpaceObjectHeight);

        float screenSpaceObjectBottomEdgePositionY = GetObjectBottomEdgePositionY(screenSpaceObject);
        //Utils.DebugUtils.DebugLog("Screen space object bottom edge position Y: " + screenSpaceObjectBottomEdgePositionY);

        float neighborObjectBottomEdgePositionY = GetObjectBottomEdgePositionY(neighborObject);
        //Utils.DebugUtils.DebugLog("Alignment object bottom edge position Y: " + neighborObjectBottomEdgePositionY);

        float newScreenSpaceObjectBottomEdgePositionY = neighborObjectBottomEdgePositionY - (Screen.height * bufferProportion);
        //Utils.DebugUtils.DebugLog("New screen space object bottom edge position Y: " + newScreenSpaceObjectBottomEdgePositionY);

        float newScreenSpaceObjectHeight = screenSpaceObjectRectTransform.rect.height + (screenSpaceObjectBottomEdgePositionY - newScreenSpaceObjectBottomEdgePositionY);
        //Utils.DebugUtils.DebugLog("New screen space object height: " + newScreenSpaceObjectHeight);

        float screenSpaceObjectHeightDelta = newScreenSpaceObjectHeight - screenSpaceObjectHeight;
        //Utils.DebugUtils.DebugLog("Screen space object height delta: " + screenSpaceObjectHeightDelta);
        screenSpaceObjectRectTransform.sizeDelta = new Vector2(screenSpaceObjectRectTransform.rect.width, newScreenSpaceObjectHeight);

        // the resize happened from the center, so adjust the position to compensate
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectRectTransform.position.x, screenSpaceObjectRectTransform.position.y - (screenSpaceObjectHeightDelta / 2), 0);
        screenSpaceObjectRectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectHeightByBufferRatioFromScreenBottom(GameObject screenSpaceObject, float bufferProportion)
    {
        RectTransform screenSpaceObjectRectTransform = screenSpaceObject.GetComponent<RectTransform>();

        float screenSpaceObjectHeight = screenSpaceObjectRectTransform.rect.height;

        float screenSpaceObjectBottomEdgePositionY = GetObjectBottomEdgePositionY(screenSpaceObject);
        //Utils.DebugUtils.DebugLog("Screen space object bottom edge position Y: " + screenSpaceObjectBottomEdgePositionY);

        float screenBottomEdgePositionY = 0;

        float newScreenSpaceObjectBottomEdgePositionY = screenBottomEdgePositionY + (Screen.height * bufferProportion);
        //Utils.DebugUtils.DebugLog("New screen space object bottom edge position Y: " + newScreenSpaceObjectBottomEdgePositionY);

        float newScreenSpaceObjectHeight = screenSpaceObjectRectTransform.rect.height + (screenSpaceObjectBottomEdgePositionY - newScreenSpaceObjectBottomEdgePositionY);
        //Utils.DebugUtils.DebugLog("New screen space object height: " + newScreenSpaceObjectHeight);

        float screenSpaceObjectHeightDelta = newScreenSpaceObjectHeight - screenSpaceObjectHeight;
        screenSpaceObjectRectTransform.sizeDelta = new Vector2(screenSpaceObjectRectTransform.rect.width, newScreenSpaceObjectHeight);

        // the resize happened from the center, so adjust the position to compensate
        Vector3 newObjectPosition = new Vector3(screenSpaceObjectRectTransform.position.x, screenSpaceObjectRectTransform.position.y - (screenSpaceObjectHeightDelta / 2), 0);
        screenSpaceObjectRectTransform.position = newObjectPosition;
    }

    public static void ResizeObjectToMatchNeighborBothDirections(GameObject screenSpaceObject, GameObject neighborObject)
    {
        RectTransform currentRectTransform = screenSpaceObject.GetComponent<RectTransform>();
        RectTransform requestedRectTransform = neighborObject.GetComponent<RectTransform>();

        currentRectTransform.sizeDelta = requestedRectTransform.sizeDelta;
    }

    public static void MatchRectTransform(GameObject sourceObject, GameObject changeObject)
    {
        RectTransform changeObjectRectTransform = changeObject.GetComponent<RectTransform>();
        RectTransform sourceObjectRectTransform = sourceObject.GetComponent<RectTransform>();

        changeObjectRectTransform.position = sourceObjectRectTransform.position;
        changeObjectRectTransform.localPosition = sourceObjectRectTransform.localPosition;
        changeObjectRectTransform.anchorMin = sourceObjectRectTransform.anchorMin;
        changeObjectRectTransform.anchorMax = sourceObjectRectTransform.anchorMax;
        changeObjectRectTransform.anchoredPosition = sourceObjectRectTransform.anchoredPosition;
        changeObjectRectTransform.sizeDelta = sourceObjectRectTransform.sizeDelta;
    }

    public static Vector2 ResizeObjectFromCenterByMargin(GameObject screenSpaceObject, float screenSizeRatioMarginWidth, float screenSizeRatioMarginHeight)
    {
        RectTransform currentRectTransform = screenSpaceObject.GetComponent<RectTransform>();

        float widthMarginPixels = Screen.width * screenSizeRatioMarginWidth;
        float heightMarginPixels = Screen.height * screenSizeRatioMarginHeight;

        float newRectWidth = currentRectTransform.rect.width + (2 * widthMarginPixels);
        float newRectHeight = currentRectTransform.rect.height + (2 * heightMarginPixels);

        currentRectTransform.sizeDelta = new Vector2(newRectWidth, newRectHeight);

        return new Vector2(newRectWidth, newRectHeight);
    }

    public static Vector2 ResizeTextExtentsToFitContents(Text text)
    {
        RectTransform introMessageRectTransform = text.GetComponent<RectTransform>();

        float textWidth = LayoutUtility.GetPreferredWidth(introMessageRectTransform);
        float textHeight = LayoutUtility.GetPreferredHeight(introMessageRectTransform);

        introMessageRectTransform.sizeDelta = new Vector2(textWidth, textHeight);

        Vector2 finalSize = new Vector2(LayoutUtility.GetPreferredWidth(introMessageRectTransform), LayoutUtility.GetPreferredHeight(introMessageRectTransform));

        introMessageRectTransform.sizeDelta = finalSize;

        return finalSize;
    }

    public static void ScaleObjectByScreenHeightProportion(GameObject screenSpaceObject, float heightProportion)
    {
        RectTransform rectTransform = screenSpaceObject.GetComponent<RectTransform>();

        // get the object height
        float objectHeight = rectTransform.rect.height;

        // determine the new height
        float newObjectHeight = heightProportion * Screen.height;

        // get the current object scale
        Vector2 scale = rectTransform.localScale;

        // adjust the scale to fit the requested proportion
        scale *= newObjectHeight / objectHeight;
        rectTransform.localScale = scale;
    }

    // scale images to always fill the entire screen
    public static void ScaleImageToFillScreen(Image screenSpaceImage)
    {
        // for some reason this is required
        RectTransform rectTransform = screenSpaceImage.gameObject.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector2(1.0f, 1.0f);

        // get the screen aspect ratio
        float imageWidth = screenSpaceImage.preferredWidth;
        float imageHeight = screenSpaceImage.preferredHeight;
        float screenWidthHeightRatio = imageWidth / imageHeight;

        // get the fitter script, if it exists
        AspectRatioFitter fitter = screenSpaceImage.gameObject.GetComponent<AspectRatioFitter>();
        // otherwise, add a fitter script
        if (fitter == null)
        {
            fitter = screenSpaceImage.gameObject.AddComponent<AspectRatioFitter>();
        }

        // configure the fitter script
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = screenWidthHeightRatio;
    }
}