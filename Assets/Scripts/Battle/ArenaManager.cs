using System;
using UnityEngine;

/// <summary>
/// Behaviour attached to the arena used to resize it.
/// </summary>
public class ArenaManager : MonoBehaviour {
    public const int UIWidth = 565; // Width of the inner Undertale UI box.
    public const int UIHeight = 130; // Height of the inner Undertale UI box.
    public Vector2 usualPosition; // Usual position of the arena.
    public static ArenaManager instance; // Static instance of this class for referencing purposes.
    [HideInInspector] public static Rect arenaAbs; // Arena hitbox.
    [HideInInspector] public static Vector2 arenaCenter; // Arena center, updated here to save computation time on doing it per frame.
    [HideInInspector] public static LuaArenaStatus luaStatus { get; private set; } // The Lua Arena object on the C# side.

    public bool firstTurn = true, yup = false, falseInit = false; // TODO: Check what those are used for, it seems it's for some kind of delay.

    private RectTransform outer; // RectTransform of the slightly larger white box under the arena (it's the border).
    private RectTransform inner; // RectTransform of the inner part of the arena.
    private readonly int pxPerSecond = 100 * 10; // How many pixels per second the arena should resize and move.

    private float currentWidth; // Current width of the arena as it is resizing.
    private float currentHeight; // Current height of the arena as it is resizing.
    private float currentX; // Current X of the arena as it is moving.
    private float currentY; // Current Y of the arena as it is moving.
    internal float newWidth; // Desired width of the arena.
    internal float newHeight; // Desired height of the arena.
    internal float newX; // Desired x of the arena.
    internal float newY; // Desired y of the arena.
    private bool movePlayer; // Shall the Player move with the Arena?

    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        // Only one Arena at the same time, please.
        if (instance != null)
            throw new CYFException("Currently, the ArenaManager may only be attached to one object.");

        inner = GameObject.Find("arena").GetComponent<RectTransform>();
        outer = inner.parent.GetComponent<RectTransform>();
        newWidth = currentWidth;
        newHeight = currentHeight;
        instance = this;
        luaStatus = new LuaArenaStatus();
    }

    /// <summary>
    /// First function launched by this script when an instance is created.
    /// </summary>
    private void Start() {
        // Some variables must be set after waiting the end of the frame.
        LateUpdater.lateActions.Add(LateStart);
    }

    /// <summary>
    /// Function used to manipulate data we wouldn't have access to if we didn't wait until the end of the frame.
    /// </summary>
    private void LateStart() {
        try {
            // Recreate the Arena if it's not created correctly.
            if (inner == null || outer == null) {
                inner = GameObject.Find("arena").GetComponent<RectTransform>();
                outer = inner.parent.GetComponent<RectTransform>();
            }
            // Creates the arena's hitbox.
            arenaAbs = new Rect(inner.position.x - inner.sizeDelta.x / 2, inner.position.y - inner.sizeDelta.y / 2, inner.rect.width, inner.rect.height);
            arenaCenter = RTUtil.AbsCenterOf(inner);
            // Moves the arena and resizes it.
            newX = currentX = 320;
            newY = currentY = 90;
            currentWidth = inner.rect.width;
            currentHeight = inner.rect.height;
            usualPosition = arenaCenter;
        } catch {
            // If there's an error, try again on the next frame.
            LateUpdater.lateActions.Add(LateStart);
        }
    }

    /// <summary>
    /// Sets the desired size of the arena, after which it will be resized until it reaches the desired size.
    /// </summary>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void Resize(float newWidth, float newHeight) {
        this.newWidth = newWidth; 
        this.newHeight = newHeight;
    }

    /// <summary>
    /// Moves the arena, after which it will move until it reaches the desired position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    public void Move(float newX, float newY, bool movePlayer = true) {
        this.newX += newX;
        this.newY += newY;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Sets the desired position of the arena, after which it will move until it reaches the desired position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    public void MoveTo(float newX, float newY, bool movePlayer = true) {
        this.newX = newX;
        this.newY = newY;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Sets the desired position and size of the arena, after which it will move and be resized until it reaches the desired size and position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveAndResize(float newX, float newY, float newWidth, float newHeight, bool movePlayer = true) {
        this.newX += newX;
        this.newY += newY;
        this.newWidth = newWidth;
        this.newHeight = newHeight;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Sets the desired position and size of the arena, after which it will move and be resized until it reaches the desired size and position.
    /// </summary>
    /// <param name="newX">Desired x of the arena</param>
    /// <param name="newY">Desired y of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveToAndResize(float newX, float newY, float newWidth, float newHeight, bool movePlayer = true) {
        this.newX = newX;
        this.newY = newY;
        this.newWidth = newWidth;
        this.newHeight = newHeight;
        this.movePlayer = movePlayer;
    }

    /// <summary>
    /// Sets the desired size of the arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    public void ResizeImmediate(float newWidth, float newHeight) {
        Resize(newWidth, newHeight);
        currentWidth = this.newWidth;
        currentHeight = this.newHeight;
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Moves the arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired x of the arena</param>
    /// <param name="newy">Desired y of the arena</param>
    public void MoveImmediate(float newX, float newY, bool movePlayer = true) {
        Move(newX, newY, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Sets the desired position of the arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired x of the arena</param>
    /// <param name="newy">Desired y of the arena</param>
    public void MoveToImmediate(float newX, float newY, bool movePlayer = true) {
        MoveTo(newX, newY, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Sets the desired position and size of the arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveAndResizeImmediate(float newX, float newY, float newWidth, float newHeight, bool movePlayer = true) {
        MoveAndResize(newX, newY, newWidth, newHeight, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        currentWidth = this.newWidth;
        currentHeight = this.newHeight;
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Sets the desired position and size of the arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    /// <param name="newWidth">Desired width of the arena</param>
    /// <param name="newHeight">Desired height of the arena</param>
    public void MoveToAndResizeImmediate(float newX, float newY, float newWidth, float newHeight, bool movePlayer = true) {
        MoveToAndResize(newX, newY, newWidth, newHeight, movePlayer);
        currentX = this.newX;
        currentY = this.newY;
        currentWidth = this.newWidth;
        currentHeight = this.newHeight;
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
    }

    /// <summary>
    /// Gets how far along this arena is with changes. Does this by returning the lowest of ratios of desired width/height/x/y to intended width/height/x/y.
    /// </summary>
    /// <returns>0.0 if the changes has just started, 1.0 if it has finished.</returns>
    public float GetProgress() {
        // depending on whether arena gets larger or smaller or its movement, adjust division order
        float widthFrac = newWidth > currentWidth ? currentWidth / newWidth : newWidth / currentWidth;
        float heightFrac = newHeight > currentHeight ? currentHeight / newHeight : newHeight / currentHeight;
        float xFrac = newX > currentX ? currentX / newX : newX / currentX;
        float yFrac = newY > currentY ? currentY / newY : newY / currentY;
        return Mathf.Min(widthFrac, heightFrac, xFrac, yFrac);
    }

    /// <summary>
    /// Used to check if the arena is currently being resized.
    /// </summary>
    /// <returns>True if it hasn't reached the intended size yet, False otherwise.</returns>
    public bool isResizeInProgress() {
        return currentWidth != newWidth || currentHeight != newHeight;
    }

    /// <summary>
    /// Used to check if the arena is currently being moved.
    /// </summary>
    /// <returns>True if it is moving, False otherwise.</returns>
    public bool isMoveInProgress() {
        return currentX != newX || currentX != newX;
    }

    /// <summary>
    /// Updates the arena.
    /// </summary>
    private void Update() {
        // On the first frames of a battle, the Arena is fucky and doesn't move where it's supposed to.
        // This block is here to assure it'll stay where it is supposed to.
        // Should be removed if any other way to do it better is fine.
        if (firstTurn) {
            if (!falseInit) {
                Vector2[] enemyPositions = GameObject.FindObjectOfType<EnemyEncounter>().enemyPositions;
                EnemyController[] rts = GameObject.FindObjectsOfType<EnemyController>();
                
                bool nope = false;
                for (int i = 0; i < rts.Length; i++)
                    // Checks for the enemy's position. If one of the enemy's position is not right, the sprites are not placed well yet.
                    if (rts[i].GetComponent<RectTransform>().position.y != 231 + enemyPositions[rts.Length - i - 1].y)
                        nope = true;
                // If the enemy sprites are well placed, that means we can continue.
                if (!nope)
                    falseInit = true;
            }
            // Wait for one last frame then the Arena will be available.
            if (yup)        firstTurn = false;
            // Wait for one frame...
            if (falseInit)  yup = true;
            return;
        }
        
        // If the Arena is at its desired position/size, then we don't need to continue.
        //if (UIController.instance.state != UIController.UIState.DEFENDING && UIController.instance.state != UIController.UIState.ENEMYDIALOGUE)
        //    outer.position = new Vector3(320, 90, outer.position.z);
        
        // do not resize the arena if the state is frozen with NONE
        if (UIController.instance.frozenState != UIController.UIState.NONE)
            return;
        
        if (currentWidth == newWidth && currentHeight == newHeight && currentX == newX && currentY == newY)
            return;

        // Changes the arena's width.
        if (currentWidth != newWidth) {
            bool increases = currentWidth < newWidth;
            // Adds a value per second if the width is lower than the goal width, substracts it otherwise.
            currentWidth += pxPerSecond * Time.deltaTime * (increases ? 1 : -1);
            if (currentWidth < newWidth != increases)
                currentWidth = newWidth;
        }

        // Changes the arena's height.
        if (currentHeight != newHeight) {
            bool increases = currentHeight < newHeight;
            // Adds a value per second if the height is lower than the goal width, substracts it otherwise.
            currentHeight += pxPerSecond * Time.deltaTime * (increases ? 1 : -1);
            if (currentHeight < newHeight != increases)
                currentHeight = newHeight;
        }

        // Changes the arena's x position.
        if (currentX != newX) {
            bool increases = currentX < newX;
            // Adds a value per second if the x position is lower than the goal width, substracts it otherwise.
            // This value is divided by 2 so the animation when moving and resizing the Arena is smooth.
            currentX += pxPerSecond * Time.deltaTime / 2 * (increases ? 1 : -1);
            if (currentX < newX != increases)
                currentX = newX;
        }

        // Changes the arena's y position.
        if (currentY != newY) {
            bool increases = currentY < newY;
            // Adds a value per second if the y position is lower than the goal width, substracts it otherwise.
            // This value is divided by 2 so the animation when moving and resizing the Arena is smooth.
            currentY += pxPerSecond * Time.deltaTime / 2 * (increases ? 1 : -1);
            if (currentY < newY != increases)
                currentY = newY;
        }

        // Applies the changes we've applied to the current position and size values. 
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);

        // The arena can't be at x = 0 and y = 0 because that's where the Arena is when the weird "Arena not at the right place" bug occurs.
        // Will be removed in the future, hopefully.
        if (outer.position == new Vector3(0, 0, outer.position.z))
            outer.position = new Vector3(320, 90, outer.position.z);
    }

    /// <summary>
    /// Takes care of actually applying the resize and movement of the arena's rectangle.
    /// </summary>
    /// <param name="arenaX">New X position of the arena.</param>
    /// <param name="arenaY">New Y position of the arena.</param>
    /// <param name="arenaWidth">New width of the arena.</param>
    /// <param name="arenaHeight">New height of the arena.</param>
    private void ApplyChanges(float arenaX, float arenaY, float arenaWidth, float arenaHeight) {
        // Resizes the Arena. The arena's outer rectangle is always 10px bigger than the arena's inner rectangle.
        inner.sizeDelta = new Vector2(arenaWidth, arenaHeight);
        outer.sizeDelta = new Vector2(arenaWidth + 10, arenaHeight + 10);

        // Moves the player along with the Arena if the option for it has been set to true.
        if (movePlayer)
            PlayerController.instance.MoveDirect(new Vector2(arenaX - outer.position.x, arenaY - outer.position.y));

        // This block was in this function but firstTurn can't be false here, so I'll keep it for further investigations.
        /*if (!firstTurn) {
            outer.position = new Vector2(arenaX, arenaY);
            outer.localPosition = new Vector3(outer.localPosition.x, outer.localPosition.y, 0);
            arenaAbs.x = inner.position.x - inner.sizeDelta.x / 2;
            arenaAbs.y = inner.position.y - inner.sizeDelta.y / 2;
        }*/

        // Modifies the arena's hitbox accordingly to its new size.
        arenaAbs.width = inner.rect.width;
        arenaAbs.height = inner.rect.height;
        // Moves the arena's center accordingly to its new position.
        arenaCenter = new Vector2(inner.transform.position.x, inner.transform.position.y);
    }

    /// <summary>
    /// Resets the arena's position and size.
    /// </summary>
    public void ResetArena() {
        // Moves the arena back only if the Arena is not ready yet.
        // Must be removed in the future too, we're interacting too much with the Arena when it's not ready.
        if (!firstTurn)
            MoveToImmediate(320, 90, false);
        Resize(UIWidth, UIHeight);
    }
}