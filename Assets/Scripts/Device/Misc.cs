using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public class Misc {
    public string MachineName {
        get { return System.Environment.UserName; }
    }

    public void ShakeScreen(float duration, float intensity = 3, bool isIntensityDecreasing = true) {
        Camera.main.GetComponent<GlobalControls>().ShakeScreen(duration, intensity, isIntensityDecreasing);
    }

    public void StopShake() {
        GlobalControls.stopScreenShake = true;
    }

    public bool FullScreen {
        get { return Screen.fullScreen; }
        set {
            Screen.fullScreen = value;
            ScreenResolution.SetFullScreen(value, 2);
        }
    }

    public static int WindowWidth {
        get { return (Screen.fullScreen && ScreenResolution.wideFullscreen) ? Screen.currentResolution.width : (int)ScreenResolution.displayedSize.x; }
    }

    public static int WindowHeight {
        get { return (Screen.fullScreen && ScreenResolution.wideFullscreen) ? Screen.currentResolution.height : (int)ScreenResolution.displayedSize.y; }
    }

    public static int ScreenWidth {
        get { return (Screen.fullScreen && !ScreenResolution.wideFullscreen) ? (int)ScreenResolution.displayedSize.x : Screen.currentResolution.width; }
    }

    public static int ScreenHeight {
        get { return Screen.currentResolution.height; }
    }

    public static int MonitorWidth {
        get { return ScreenResolution.lastMonitorWidth; }
    }

    public static int MonitorHeight {
        get { return ScreenResolution.lastMonitorHeight; }
    }

    public void SetWideFullscreen(bool borderless) {
        if (!GlobalControls.isInFight)
            throw new CYFException("SetWideFullscreen is only usable from within battles.");
        ScreenResolution.wideFullscreen = borderless;
        if (Screen.fullScreen)
            ScreenResolution.SetFullScreen(true, 0);
    }

    public static float cameraX {
        get { return Camera.main.transform.position.x - 320; }
        set {
            if (UnitaleUtil.IsOverworld && !GlobalControls.isInShop)
                PlayerOverworld.instance.cameraShift.x += value - (Camera.main.transform.position.x - 320);
            else {
                Camera.main.transform.position = new Vector3(value + 320, Camera.main.transform.position.y, Camera.main.transform.position.z);
                if (UserDebugger.instance)
                    UserDebugger.instance.transform.position = new Vector3(value + 620, UserDebugger.instance.transform.position.y, UserDebugger.instance.transform.position.z);
            }
        }
    }

    public static float cameraY {
        get { return Camera.main.transform.position.y - 240; }
        set {
            if (UnitaleUtil.IsOverworld && !GlobalControls.isInShop)
                PlayerOverworld.instance.cameraShift.y += value - (Camera.main.transform.position.y - 240);
            else {
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, value + 240, Camera.main.transform.position.z);
                if (UserDebugger.instance)
                    UserDebugger.instance.transform.position = new Vector3(UserDebugger.instance.transform.position.x, value + 480, UserDebugger.instance.transform.position.z);
            }
        }
    }

    public static void MoveCamera(float x, float y) {
        cameraX += x;
        cameraY += y;
    }

    public static void MoveCameraTo(float x, float y) {
        cameraX = x;
        cameraY = y;
    }

    public static void ResetCamera() {
        if (UnitaleUtil.IsOverworld && !GlobalControls.isInShop)
            PlayerOverworld.instance.cameraShift = Vector2.zero;
        else
            MoveCameraTo(0f, 0f);
    }

    public LuaSpriteShader ScreenShader {
        get { return CameraShader.luashader; }
    }

    public static void DestroyWindow() { Application.Quit(); }

    public static LuaFile OpenFile(string path, string mode = "rw") { // TODO: When OW is reworked, add 3rd argument to open a file in any of "mod", "map" or "default" locations
        return new LuaFile(path, mode);
    }

    public bool FileExists(string path) {
        if (path.Contains(".."))
            throw new CYFException("You cannot check for a file outside of a mod folder. The use of \"..\" is forbidden.");
        return File.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/'));
    }

    public bool DirExists(string path) {
        if (path.Contains(".."))
            throw new CYFException("You cannot check for a directory outside of a mod folder. The use of \"..\" is forbidden.");
        return Directory.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/'));
    }

    public bool CreateDir(string path) {
        if (path.Contains(".."))
            throw new CYFException("You cannot create a directory outside of a mod folder. The use of \"..\" is forbidden.");

        if (!Directory.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/'))) {
            Directory.CreateDirectory((FileLoader.ModDataPath + "/" + path));
            return true;
        }
        return false;
    }

    private bool PathValid(string path) { return (path != " " && path != "" && path != "/" && path != "\\" && path != "." && path != "./" && path != ".\\"); }

    public bool MoveDir(string path, string newPath) {
        if (path.Contains("..") || newPath.Contains(".."))
            throw new CYFException("You cannot move a directory outside of a mod folder. The use of \"..\" is forbidden.");

        if (DirExists(path) && !DirExists(newPath) && PathValid(path)) {
            Directory.Move(FileLoader.ModDataPath + "/" + path, FileLoader.ModDataPath + "/" + newPath);
            return true;
        }
        return false;
    }

    public bool RemoveDir(string path, bool force = false) {
        if (path.Contains(".."))
            throw new CYFException("You cannot remove a directory outside of a mod folder. The use of \"..\" is forbidden.");

        if (Directory.Exists((FileLoader.ModDataPath + "/" + path).Replace('\\', '/')))
            try { Directory.Delete((FileLoader.ModDataPath + "/" + path), force); } catch {}
        return false;
    }

    public string[] ListDir(string path, bool getFolders = false) {
        if (path == null)
            throw new CYFException("Cannot list a directory with a nil path.");
        if (path.Contains(".."))
            throw new CYFException("You cannot list directories outside of a mod folder. The use of \"..\" is forbidden.");

        path = (FileLoader.ModDataPath + "/" + path).Replace('\\', '/');
        if (!Directory.Exists(path))
            throw new CYFException("Invalid path:\n\n\"" + path + "\"");

        DirectoryInfo d = new DirectoryInfo(path);
        System.Collections.Generic.List<string> retval = new System.Collections.Generic.List<string>();
        if (!getFolders)
            foreach (FileInfo fi in d.GetFiles())
                retval.Add(Path.GetFileName(fi.ToString()));
        else
            foreach (DirectoryInfo di in d.GetDirectories())
                retval.Add(di.Name);
        return retval.ToArray();
    }

    public static string OSType {
        get {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)  return "Windows";
            else if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer) return "Linux";
            else                                                                                                                 return "Mac";
        }
    }

    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        private static extern int GetActiveWindow();
        public static int window = GetActiveWindow();

        public static void RetargetWindow() { window = GetActiveWindow(); }

        [DllImport("user32.dll")]
        public static extern int FindWindow(string className, string windowName);
        [DllImport("user32.dll")]
        private static extern int MoveWindow(int hwnd, int x, int y, int nWidth, int nHeight, int bRepaint);
        [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(int hwnd, StringBuilder lpWindowText, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(int hwnd, string text);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(int hWnd, out RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static string WindowName {
            get {
                StringBuilder strbTitle = new StringBuilder(9999);
                GetWindowText(window, strbTitle, strbTitle.Capacity + 1);
                return strbTitle.ToString();
            }
            set { SetWindowText(window, value); }
        }

        public static int WindowX {
                get {
                    Rect size = GetWindowRect();
                    return (int)size.x;
                }
                set {
                     Rect size = GetWindowRect();
                     MoveWindow(window, value, (int)size.y, (int)size.width, (int)size.height, 1);
                }
            }

        public static int WindowY {
            get {
                Rect size = GetWindowRect();
                return Screen.currentResolution.height - (int)size.y - (int)size.height;
            }
            set {
                 Rect size = GetWindowRect();
                 MoveWindow(window, (int)size.x, Screen.currentResolution.height - value - (int)size.height, (int)size.width, (int)size.height, 1);
            }
        }

        public static void MoveWindow(int X, int Y) {
            Rect size = GetWindowRect();
            if (!Screen.fullScreen)
                MoveWindow(window, (int)size.x + X, (int)size.y - Y, (int)size.width, (int)size.height, 1);
        }

        public static void MoveWindowTo(int X, int Y) {
            Rect size = GetWindowRect();
            if (!Screen.fullScreen)
                MoveWindow(window, X, Screen.currentResolution.height - Y - (int)size.height, (int)size.width, (int)size.height, 1);
        }

        private static Rect GetWindowRect() {
            RECT r = new RECT();
            GetWindowRect(window, out r);
            return new Rect(r.Left, r.Top, Mathf.Abs(r.Right - r.Left), Mathf.Abs(r.Top - r.Bottom));
        }
#else
        public static string WindowName {
            get {
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
                return "";
            }
            set { UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }

        public static int WindowX {
            get {
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
                return 0;
            }
            set { UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }

        public static int WindowY {
            get {
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
                return 0;
            }
            set { UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }

        public static void MoveWindowTo(int X, int Y) {
            UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
            return;
        }

        public static void MoveWindow(int X, int Y) {
            UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
            return;
        }

        public static Rect GetWindowRect() {
            UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here.");
            return new Rect();
        }
#endif
}