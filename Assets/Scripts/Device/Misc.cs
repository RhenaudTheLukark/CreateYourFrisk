using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

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
            
            GlobalControls.SetFullScreen(value, 2);
        }
    }

    public static int ScreenHeight {
        get { return Screen.currentResolution.height; }
    }

    public static int ScreenWidth {
        get { return Screen.currentResolution.width; }
    }
    
    public static float cameraX {
        get { return Camera.main.transform.position.x - 320; }
        set { Camera.main.transform.position = new Vector3(value + 320, Camera.main.transform.position.y, Camera.main.transform.position.z); }
    }
    
    public static float cameraY {
        get { return Camera.main.transform.position.y - 240; }
        set { Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, value + 240, Camera.main.transform.position.z); }
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
        MoveCameraTo(0f, 0f);
    }

    public static void DestroyWindow() { Application.Quit(); }

    #if UNITY_STANDALONE_WIN || UNITY_EDITOR
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
    
        public static int WindowWidth {
            get {
                Rect size = GetWindowRect();
                return (int)size.width;
            }
        }

        public static int WindowHeight {
            get {
                Rect size = GetWindowRect();
                return (int)size.height;
            }
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
    
        public static int WindowWidth {
            get { 
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
                return 0;
            }
        }

        public static int WindowHeight {
            get { 
                UnitaleUtil.DisplayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
                return 0;
            }
        }
#endif
}