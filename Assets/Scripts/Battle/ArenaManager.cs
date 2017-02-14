using System;
using UnityEngine;

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
    public bool firstTurn = true, yup = false, falseInit = false;

    private RectTransform outer; // RectTransform of the slightly larger white box under the arena (it's the border).
    private RectTransform inner; // RectTransform of the inner part of the arena.
    private int pxPerSecond = 100 * 10; // How many pixels per second the arena should resize and move

    private float currentWidth; // Current width of the arena as it is resizing
    private float currentHeight; // Current height of the arena as it is resizing
    private float currentX; // Current X of the arena as it is moving
    private float currentY; // Current Y of the arena as it is moving
    internal float newWidth; // Desired width of the arena; internal so the Lua Arena object may refer to it (lazy)
    internal float newHeight; // Desired height of the arena; internal so the Lua Arena object may refer to it (lazy)
    internal float newX; // Desired x of the arena; internal so the Lua Arena object may refer to it (lazy)
    internal float newY; // Desired y of the arena; internal so the Lua Arena object may refer to it (lazy)
    private bool movePlayer;

    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        // unlike the player we really dont want this on two components at the same time
        if (instance != null)
            throw new InvalidOperationException("Currently, the ArenaManager may only be attached to one object.");
        
        outer = GameObject.Find("arena_border_outer").GetComponent<RectTransform>();
        inner = GameObject.Find("arena").GetComponent<RectTransform>();
        newWidth = currentWidth;
        newHeight = currentHeight;
        instance = this;
        luaStatus = new LuaArenaStatus();
    }

    private void Start() { LateUpdater.lateActions.Add(LateStart); }

    private void LateStart() {
        arenaAbs = new Rect(inner.position.x - inner.rect.width / 2, inner.position.y - inner.rect.height / 2, inner.rect.width, inner.rect.height);
        arenaCenter = RTUtil.AbsCenterOf(inner);
        newX = currentX = 320;
        newY = currentY = 90;
        currentWidth = inner.rect.width;
        currentHeight = inner.rect.height;
        basisCoordinates = arenaCenter;
        //outer.localPosition = new Vector3(0, -50, 0);
        //outer.position = new Vector3(320, 90, outer.position.z);
    }

    /// <summary>
    /// Set the desired size of this arena, after which it will keep resizing until it reaches the desired size.
    /// </summary>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void Resize(float newWidth, float newHeight) {
        this.newWidth = newWidth; 
        this.newHeight = newHeight;
    }

    /// <summary>
    /// Move the arena, after which it will keep moving until it reaches the desired position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    public void Move(float newX, float newY, bool movePlayer) {
        this.newX += newX;
        this.newY += newY;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Set the desired position of this arena, after which it will keep moving until it reaches the desired position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    public void MoveTo(float newX, float newY, bool movePlayer) {
        this.newX = newX;
        this.newY = newY;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Set the desired position and size of this arena, after which it will keep moving and resizing until it reaches the desired size and position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveAndResize(float newX, float newY, float newWidth, float newHeight, bool movePlayer) {
        this.newX += newX;
        this.newY += newY;
        this.newWidth = newWidth;
        this.newHeight = newHeight;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Set the desired position and size of this arena, after which it will keep moving and resizing until it reaches the desired size and position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveToAndResize(float newX, float newY, float newWidth, float newHeight, bool movePlayer) {
        this.newX = newX;
        this.newY = newY;
        this.newWidth = newWidth;
        this.newHeight = newHeight;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Set the desired size of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    public void ResizeImmediate(float newWidth, float newHeight) {
        Resize(newWidth, newHeight);
        currentWidth = this.newWidth;
        currentHeight = this.newHeight;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired x of the arena</param>
    /// <param name="newy">Desired y of the arena</param>
    public void MoveImmediate(float newX, float newY, bool movePlayer) {
        Move(newX, newY, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired x of the arena</param>
    /// <param name="newy">Desired y of the arena</param>
    public void MoveToImmediate(float newX, float newY, bool movePlayer) {
        MoveTo(newX, newY, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position and size of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveAndResizeImmediate(float newX, float newY, float newWidth, float newHeight, bool movePlayer) {
        MoveAndResize(newX, newY, newWidth, newHeight, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        currentWidth = this.newWidth;
        currentHeight = this.newHeight;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Set the desired position and size of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveToAndResizeImmediate(float newX, float newY, float newWidth, float newHeight, bool movePlayer) {
        MoveToAndResize(newX, newY, newWidth, newHeight, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        currentWidth = this.newWidth;
        currentHeight = this.newHeight;
        applyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Gets how far along this arena is with changes. Does this by returning the lowest of ratios of desired width/height/x/y to intended width/height/x/y.
    /// </summary>
    /// <returns>0.0 if the changes has just started, 1.0 if it has finished.</returns>
    public float getProgress() {
        // depending on whether arena gets larger or smaller or its movement, adjust division order
        float widthFrac = newWidth > currentWidth ? currentWidth / newWidth : newWidth / currentWidth;
        float heightFrac = newHeight > currentHeight ? currentHeight / newHeight : newHeight / currentHeight;
        float xFrac = newX > currentX ? currentX / newX : newX / currentX;
        float yFrac = newY > currentY ? currentY / newY : newY / currentY;
        return Mathf.Min(widthFrac, heightFrac, xFrac, yFrac);
    }

    /// <summary>
    /// Used to check if the arena is currently in the process of resizing.
    /// </summary>
    /// <returns>true if it hasn't reached the intended size yet, false otherwise</returns>
    public bool isResizeInProgress() {
        return currentWidth != newWidth || currentHeight != newHeight;
    }

    public bool isMoveInProgress() {
        return currentX != newX || currentX != newX;
    }

    /// <summary>
    /// Resizes the arena if the desired size is different from the current size.
    /// </summary>
    private void Update() {
        if (firstTurn) {
            if (!falseInit) {
                Vector2[] enemyPositions = GameObject.FindObjectOfType<EnemyEncounter>().enemyPositions;
                EnemyController[] rts = GameObject.FindObjectsOfType<EnemyController>();
                
                bool nope = false;
                for (int i = 0; i < rts.Length; i++)
                    if (rts[i].GetComponent<RectTransform>().position.y != 231 + enemyPositions[rts.Length - i - 1].y)
                        nope = true;
                if (!nope)
                    falseInit = true;
            }
            if (yup)       firstTurn = false;
            if (falseInit)  yup = true;
            return;
        }
        //if (UIController.instance.state != UIController.UIState.DEFENDING && UIController.instance.state != UIController.UIState.ENEMYDIALOGUE)
        //    outer.position = new Vector3(320, 90, outer.position.z);
        if (currentWidth == newWidth && currentHeight == newHeight && currentX == newX && currentY == newY)
            return;
        if (currentWidth < newWidth) {
            currentWidth += pxPerSecond * Time.deltaTime;
            if (currentWidth >= newWidth)
                currentWidth = newWidth;
        } else if (currentWidth > newWidth) {
            currentWidth -= pxPerSecond * Time.deltaTime;
            if (currentWidth <= newWidth)
                currentWidth = newWidth;
        }

        if (currentHeight < newHeight) {
            currentHeight += pxPerSecond * Time.deltaTime;
            if (currentHeight >= newHeight)
                currentHeight = newHeight;
        } else if (currentHeight > newHeight) {
            currentHeight -= pxPerSecond * Time.deltaTime;
            if (currentHeight <= newHeight)
                currentHeight = newHeight;
        }

        if (!firstTurn) {
            if (currentX < newX) {
                currentX += pxPerSecond * Time.deltaTime / 2;
                if (currentX >= newX)
                    currentX = newX;
            } else if (currentX > newX) {
                currentX -= pxPerSecond * Time.deltaTime / 2;
                if (currentX <= newX)
                    currentX = newX;
            }

            if (currentY < newY) {
                currentY += pxPerSecond * Time.deltaTime / 2;
                if (currentY >= newY)
                    currentY = newY;
            } else if (currentY > newY) {
                currentY -= pxPerSecond * Time.deltaTime / 2;
                if (currentY <= newY)
                    currentY = newY;
            }
        }

        applyChanges(currentX, currentY, currentWidth, currentHeight);
        if (outer.position == new Vector3(0, 0, outer.position.z))
            outer.position = new Vector3(320, 90, outer.position.z);
    }

    /// <summary>
    /// Takes care of actually applying the resize and updating the arena's rectangle.
    /// </summary>
    /// <param name="arenaWidth">New width</param>
    /// <param name="arenaHeight">New height</param>
    private void applyChanges(float arenaX, float arenaY, float arenaWidth, float arenaHeight, bool first = false) {
        inner.sizeDelta = new Vector2(arenaWidth, arenaHeight);
        outer.sizeDelta = new Vector2(arenaWidth + 10, arenaHeight + 10);
        if (movePlayer)
            PlayerController.instance.MoveDirect(new Vector2(arenaX - outer.position.x, arenaY - outer.position.y));
        if (!firstTurn) {
            outer.position = new Vector2(arenaX, arenaY);
            outer.localPosition = new Vector3(outer.localPosition.x, outer.localPosition.y, 0);
            arenaAbs.x = inner.position.x - inner.rect.width / 2;
            arenaAbs.y = inner.position.y - inner.rect.height / 2;
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