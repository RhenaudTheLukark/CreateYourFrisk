using UnityEngine;
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

    private RectTransform outer; // RectTransform of the slightly larger white box under the arena (it's the border).
    private RectTransform inner; // RectTransform of the inner part of the arena.
    private const int pxPerSecond = 100 * 10; // How many pixels per second the arena should resize and move

    public float currentWidth; // Current width of the arena as it is resizing
    public float currentHeight; // Current height of the arena as it is resizing
    public float currentX; // Current X of the arena as it is moving
    public float currentY; // Current Y of the arena as it is moving
    public float desiredWidth; // Desired width of the arena; internal so the Lua Arena object may refer to it (lazy)
    public float desiredHeight; // Desired height of the arena; internal so the Lua Arena object may refer to it (lazy)
    public float desiredX; // Desired x of the arena; internal so the Lua Arena object may refer to it (lazy)
    public float desiredY; // Desired y of the arena; internal so the Lua Arena object may refer to it (lazy)
    public bool showWhenWaveEnds = false; // Used to know if we need to run Arena.Show() at the end of a wave
    private bool movePlayer;

    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        // unlike the player we really dont want this on two components at the same time
        if (instance != null)
            throw new CYFException("Currently, the ArenaManager may only be attached to one object.");

        outer = GetComponent<RectTransform>();
        inner = outer.GetChild(outer.childCount - 1).GetComponent<RectTransform>();
        innerSprite = LuaSpriteController.GetOrCreate(GameObject.Find("arena"));
        outerSprite = LuaSpriteController.GetOrCreate(GameObject.Find("arena_border_outer"));
        desiredX = outer.position.x;
        desiredY = outer.position.y;
        desiredWidth = currentWidth;
        desiredHeight = currentHeight;
        instance = this;
        luaStatus = new LuaArenaStatus();
    }

    private void Start() {
        if (inner == null || outer == null) {
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
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
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
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
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
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
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
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
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
        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
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
        // do not resize the arena if the state is frozen with PAUSE
        if (!UIController.instance || UIController.instance.frozenState != "PAUSE")
            return;

        if (currentWidth != desiredWidth) {
            float sign = Mathf.Sign(desiredWidth - currentWidth);
            currentWidth += sign * pxPerSecond * Time.deltaTime;
            if (Mathf.Sign(desiredWidth - currentWidth) != sign)
                currentWidth = desiredWidth;
        }
        if (currentHeight != desiredHeight) {
            float sign = Mathf.Sign(desiredHeight - currentHeight);
            currentHeight += sign * pxPerSecond * Time.deltaTime;
            if (Mathf.Sign(desiredHeight - currentHeight) != sign)
                currentHeight = desiredHeight;
        }

        if (currentX != desiredX) {
            float sign = Mathf.Sign(desiredX - currentX);
            currentX += sign * pxPerSecond * Time.deltaTime / 2;
            if (Mathf.Sign(desiredX - currentX) != sign)
                currentX = desiredX;
        }
        if (currentY != desiredY) {
            float sign = Mathf.Sign(desiredY - currentY);
            currentY += sign * pxPerSecond * Time.deltaTime / 2;
            if (Mathf.Sign(desiredY - currentY) != sign)
                currentY = desiredY;
        }

        ApplyChanges(currentX, currentY, currentWidth, currentHeight);
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
    private void ApplyChanges(float arenaX, float arenaY, float arenaWidth, float arenaHeight) {
        inner.sizeDelta = new Vector2(arenaWidth, arenaHeight);
        outer.sizeDelta = new Vector2(arenaWidth + 10, arenaHeight + 10);
        if (movePlayer && UIController.instance.state != "ACTIONSELECT")
            PlayerController.instance.MoveDirect(new Vector2(arenaX - outer.position.x, arenaY - outer.position.y));
        outer.position = new Vector2(arenaX, arenaY);
        outer.localPosition = new Vector3(outer.localPosition.x, outer.localPosition.y, 0);
        arenaAbs.x = inner.position.x - inner.sizeDelta.x / 2;
        arenaAbs.y = inner.position.y - inner.sizeDelta.y / 2;
        arenaAbs.width = inner.rect.width;
        arenaAbs.height = inner.rect.height;
        arenaCenter = new Vector2(inner.transform.position.x, inner.transform.position.y);
    }

    public void ResetArena() {
        MoveToImmediate(320, 90, false);
        Resize(UIWidth, UIHeight);
    }
}