﻿using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Behaviour attached to the arena used to resize it.
/// </summary>
public class ArenaManager : MonoBehaviour {
    public const int UIWidth = 565; // width of the inner Undertale UI box
    public const int UIHeight = 130; // height of the inner Undertale UI box
    public Vector2 basisCoordinates;
    public static ArenaManager instance; // Static instance of this class for referencing purposes.
    [HideInInspector]
    public static Rect arenaAbs; // arena hitbox
    [HideInInspector]
    public static Vector2 arenaCenter; // arena center, updated here to save computation time on doing it per frame
    [HideInInspector]
    public static LuaArenaStatus luaStatus { get; private set; } // The Lua Arena object on the C# side
    public LuaSpriteController innerSprite; // inner part's sprite
    public LuaSpriteController outerSprite; // outer part's sprite
    public bool firstTurn = true, yup, falseInit;

    private RectTransform outer; // RectTransform of the slightly larger white box under the arena (it's the border).
    private RectTransform inner; // RectTransform of the inner part of the arena.
    private const int pxPerSecond = 100 * 10; // How many pixels per second the arena should resize and move

    private float currentWidth; // Current width of the arena as it is resizing
    private float currentHeight; // Current height of the arena as it is resizing
    private float currentX; // Current X of the arena as it is moving
    private float currentY; // Current Y of the arena as it is moving
    internal float desiredWidth; // Desired width of the arena; internal so the Lua Arena object may refer to it (lazy)
    internal float desiredHeight; // Desired height of the arena; internal so the Lua Arena object may refer to it (lazy)
    internal float desiredX; // Desired x of the arena; internal so the Lua Arena object may refer to it (lazy)
    internal float desiredY; // Desired y of the arena; internal so the Lua Arena object may refer to it (lazy)
    private bool movePlayer;
    private int errCount = 1;

    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        // unlike the player we really dont want this on two components at the same time
        if (instance != null)
            throw new CYFException("Currently, the ArenaManager may only be attached to one object.");

        inner = GameObject.Find("arena").GetComponent<RectTransform>();
        outer = inner.parent.GetComponent<RectTransform>();
        innerSprite = LuaSpriteController.GetOrCreate(GameObject.Find("arena"));
        outerSprite = LuaSpriteController.GetOrCreate(GameObject.Find("arena_border_outer"));
        /*outer = GameObject.Find("arena_border_outer").GetComponent<RectTransform>();
        inner = GameObject.Find("arena").GetComponent<RectTransform>();*/
        desiredWidth = currentWidth;
        desiredHeight = currentHeight;
        instance = this;
        luaStatus = new LuaArenaStatus();
    }

    private void Start() { LateUpdater.lateActions.Add(LateStart); }

    private void LateStart() {
        try {
            if (inner == null || outer == null) {
                //UnitaleUtil.WriteInLogAndDebugger(outer == null && inner == null ? "outer & inner = null" : (outer == null ? "outer == null" : "inner == null"));
                inner = GameObject.Find("arena").GetComponent<RectTransform>();
                outer = inner.parent.GetComponent<RectTransform>();
            }
            arenaAbs = new Rect(inner.position.x - inner.sizeDelta.x / 2, inner.position.y - inner.sizeDelta.y / 2, inner.rect.width, inner.rect.height);
            arenaCenter = RTUtil.AbsCenterOf(inner);
            desiredX = currentX = 320;
            desiredY = currentY = 90;
            currentWidth = inner.rect.width;
            currentHeight = inner.rect.height;
            basisCoordinates = arenaCenter;
        } catch {
            LateUpdater.lateActions.Add(LateStart);
            UnitaleUtil.WriteInLogAndDebugger("Error during the Arena's initialization! (#" + errCount++ + ")");
        }
        //outer.localPosition = new Vector3(0, -50, 0);
        //outer.position = new Vector3(320, 90, outer.position.z);
    }

    /// <summary>
    /// Set the desired size of this arena, after which it will keep resizing until it reaches the desired size.
    /// </summary>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void Resize(float newWidth, float newHeight) {
        desiredWidth = newWidth;
        desiredHeight = newHeight;
    }

    /// <summary>
    /// Move the arena, after which it will keep moving until it reaches the desired position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void Move(float newX, float newY, bool newMovePlayer = true) {
        desiredX += newX;
        desiredY += newY;
        movePlayer = newMovePlayer;
    }

    /// <summary>
    /// Set the desired position of this arena, after which it will keep moving until it reaches the desired position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void MoveTo(float newX, float newY, bool newMovePlayer = true) {
        desiredX = newX;
        desiredY = newY;
        movePlayer = newMovePlayer;
    }

    /// <summary>
    /// Set the desired position and size of this arena, after which it will keep moving and resizing until it reaches the desired size and position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void MoveAndResize(float newX, float newY, float newWidth, float newHeight, bool newMovePlayer = true) {
        desiredX += newX;
        desiredY += newY;
        desiredWidth = newWidth;
        desiredHeight = newHeight;
        movePlayer = newMovePlayer;
    }

    /// <summary>
    /// Set the desired position and size of this arena, after which it will keep moving and resizing until it reaches the desired size and position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void MoveToAndResize(float newX, float newY, float newWidth, float newHeight, bool newMovePlayer = true) {
        desiredX = newX;
        desiredY = newY;
        desiredWidth = newWidth;
        desiredHeight = newHeight;
        movePlayer = newMovePlayer;
    }

    /// <summary>
    /// Set the desired size of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void ResizeImmediate(float newWidth, float newHeight) {
        Resize(newWidth, newHeight);
        currentWidth = desiredWidth;
        currentHeight = desiredHeight;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void MoveImmediate(float newX, float newY, bool newMovePlayer = true) {
        Move(newX, newY, newMovePlayer);
        currentX = desiredX;
        currentY = desiredY;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void MoveToImmediate(float newX, float newY, bool newMovePlayer = true) {
        MoveTo(newX, newY, newMovePlayer);
        currentX = desiredX;
        currentY = desiredY;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position and size of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newX">Desired width of the arena</param>
    /// <param name="newY">Desired height of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void MoveAndResizeImmediate(float newX, float newY, float newWidth, float newHeight, bool newMovePlayer = true) {
        MoveAndResize(newX, newY, newWidth, newHeight, newMovePlayer);
        currentX = desiredX;
        currentY = desiredY;
        currentWidth = desiredWidth;
        currentHeight = desiredHeight;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position and size of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newX">Desired width of the arena</param>
    /// <param name="newY">Desired height of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    /// <param name="newMovePlayer"></param>
    public void MoveToAndResizeImmediate(float newX, float newY, float newWidth, float newHeight, bool newMovePlayer = true) {
        MoveToAndResize(newX, newY, newWidth, newHeight, newMovePlayer);
        currentX = desiredX;
        currentY = desiredY;
        currentWidth = desiredWidth;
        currentHeight = desiredHeight;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Makes the arena invisible, but it will stay active.
    /// </summary>
    public void Hide() {
        inner.GetComponent<Image>().enabled = false;
        outer.GetComponent<Image>().enabled = false;
    }

    /// <summary>
    /// Makes the arena visible, if it was previously set invisible with Hide().
    /// </summary>
    public void Show() {
        inner.GetComponent<Image>().enabled = true;
        outer.GetComponent<Image>().enabled = true;
    }

    /// <summary>
    /// Gets how far along this arena is with changes. Does this by returning the lowest of ratios of desired width/height/x/y to intended width/height/x/y.
    /// </summary>
    /// <returns>0.0 if the changes has just started, 1.0 if it has finished.</returns>
    public float getProgress() {
        // depending on whether arena gets larger or smaller or its movement, adjust division order
        float widthFrac = desiredWidth > currentWidth ? currentWidth / desiredWidth : desiredWidth / currentWidth;
        float heightFrac = desiredHeight > currentHeight ? currentHeight / desiredHeight : desiredHeight / currentHeight;
        float xFrac = desiredX > currentX ? currentX / desiredX : desiredX / currentX;
        float yFrac = desiredY > currentY ? currentY / desiredY : desiredY / currentY;
        return Mathf.Min(widthFrac, heightFrac, xFrac, yFrac);
    }

    /// <summary>
    /// Used to check if the arena is currently in the process of resizing.
    /// </summary>
    /// <returns>true if it hasn't reached the intended size yet, false otherwise</returns>
    public bool isResizeInProgress() {
        return currentWidth != desiredWidth || currentHeight != desiredHeight;
    }

    public bool isMoveInProgress() {
        return currentX != desiredX || currentX != desiredX;
    }

    /// <summary>
    /// Resizes the arena if the desired size is different from the current size.
    /// </summary>
    private void Update() {
        if (firstTurn) {
            if (!falseInit) {
                Vector2[] enemyPositions = FindObjectOfType<EnemyEncounter>().enemyPositions;
                EnemyController[] rts = FindObjectsOfType<EnemyController>();

                bool nope = false;
                for (int i = 0; i < rts.Length; i++)
                    if (rts[i].GetComponent<RectTransform>().position.y != 231 + enemyPositions[rts.Length - i - 1].y)
                        nope = true;
                if (!nope)
                    falseInit = true;
            }
            if (yup)        firstTurn = false;
            if (falseInit)  yup = true;
            return;
        }
        //if (UIController.instance.state != UIController.UIState.DEFENDING && UIController.instance.state != UIController.UIState.ENEMYDIALOGUE)
        //    outer.position = new Vector3(320, 90, outer.position.z);

        // do not resize the arena if the state is frozen with PAUSE
        if (UIController.instance.frozenState != "PAUSE")
            return;

        if (currentWidth == desiredWidth && currentHeight == desiredHeight && currentX == desiredX && currentY == desiredY)
            return;
        if (currentWidth < desiredWidth) {
            currentWidth += pxPerSecond * Time.deltaTime;
            if (currentWidth >= desiredWidth)
                currentWidth = desiredWidth;
        } else if (currentWidth > desiredWidth) {
            currentWidth -= pxPerSecond * Time.deltaTime;
            if (currentWidth <= desiredWidth)
                currentWidth = desiredWidth;
        }

        if (currentHeight < desiredHeight) {
            currentHeight += pxPerSecond * Time.deltaTime;
            if (currentHeight >= desiredHeight)
                currentHeight = desiredHeight;
        } else if (currentHeight > desiredHeight) {
            currentHeight -= pxPerSecond * Time.deltaTime;
            if (currentHeight <= desiredHeight)
                currentHeight = desiredHeight;
        }

        if (!firstTurn) {
            if (currentX < desiredX) {
                currentX += pxPerSecond * Time.deltaTime / 2;
                if (currentX >= desiredX)
                    currentX = desiredX;
            } else if (currentX > desiredX) {
                currentX -= pxPerSecond * Time.deltaTime / 2;
                if (currentX <= desiredX)
                    currentX = desiredX;
            }

            if (currentY < desiredY) {
                currentY += pxPerSecond * Time.deltaTime / 2;
                if (currentY >= desiredY)
                    currentY = desiredY;
            } else if (currentY > desiredY) {
                currentY -= pxPerSecond * Time.deltaTime / 2;
                if (currentY <= desiredY)
                    currentY = desiredY;
            }
        }

        applyChanges(currentX, currentY, currentWidth, currentHeight);
        if (outer.position == new Vector3(0, 0, outer.position.z))
            outer.position = new Vector3(320, 90, outer.position.z);
    }

    /// <summary>
    /// Takes care of actually applying the resize and updating the arena's rectangle.
    /// </summary>
    /// <param name="arenaX"></param>
    /// <param name="arenaY"></param>
    /// <param name="arenaWidth">New width</param>
    /// <param name="arenaHeight">New height</param>
    private void applyChanges(float arenaX, float arenaY, float arenaWidth, float arenaHeight) {
        inner.sizeDelta = new Vector2(arenaWidth, arenaHeight);
        outer.sizeDelta = new Vector2(arenaWidth + 10, arenaHeight + 10);
        if (movePlayer)
            PlayerController.instance.MoveDirect(new Vector2(arenaX - outer.position.x, arenaY - outer.position.y));
        if (!firstTurn) {
            outer.position = new Vector2(arenaX, arenaY);
            outer.localPosition = new Vector3(outer.localPosition.x, outer.localPosition.y, 0);
            arenaAbs.x = inner.position.x - inner.sizeDelta.x / 2;
            arenaAbs.y = inner.position.y - inner.sizeDelta.y / 2;
        }
        arenaAbs.width = inner.rect.width;
        arenaAbs.height = inner.rect.height;
        arenaCenter = new Vector2(inner.transform.position.x, inner.transform.position.y);
    }

    public void resetArena() {
        if (!firstTurn)
            MoveToImmediate(320, 90, false);
        Resize(UIWidth, UIHeight);
    }
}