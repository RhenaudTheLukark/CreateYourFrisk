using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class Misc : MonoBehaviour {
    #if UNITY_STANDALONE_WIN || UNITY_EDITOR
        #if UNITY_EDITOR
            [DllImport("user32.dll")]
            private static extern int GetForegroundWindow(); 
             
            private static int window = GetForegroundWindow();
        #else
            private static int window = FindWindow(null, ControlPanel.instance.WindowBasisName);
        #endif
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string className, string windowName);
        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
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

        public static int ScreenHeight {
            get { return Screen.currentResolution.height; }
        }

        public static int ScreenWidth {
            get { return Screen.currentResolution.width; }
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

        public static void MoveWindowTo(int X, int Y) {
            Rect size = GetWindowRect();
            if (!Screen.fullScreen)
                MoveWindow(window, X, Screen.currentResolution.height - Y - (int)size.height, (int)size.width, (int)size.height, 1);
        }

        public static void MoveWindow(int X, int Y) {
            Rect size = GetWindowRect();
            if (!Screen.fullScreen)
                MoveWindow(window, (int)size.x + X, (int)size.y - Y, (int)size.width, (int)size.height, 1);
        }

        public static Rect GetWindowRect() {
            RECT r = new RECT();
            GetWindowRect(window, out r);
            return new Rect(r.Left, r.Top, Mathf.Abs(r.Right - r.Left), Mathf.Abs(r.Top - r.Bottom));
        }

        public static string WindowName {
            get {
                StringBuilder strbTitle = new StringBuilder(9999);
                GetWindowText(window, strbTitle, strbTitle.Capacity + 1);
                return strbTitle.ToString();
            }
            set { SetWindowText(window, value); }
        }
#else
        public static int WindowX { 
            get { 
                UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
                return 0;
            }
            set { UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }

        public static int WindowY {
            get { 
                UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
                return 0;
            }
            set { UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }
    
        public static int WindowWidth {
            get { 
                UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
                return 0;
            }
        }

        public static int WindowHeight {
            get { 
                UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
                return 0;
            }
        }

        public static void MoveWindowTo(int X, int Y) {
            UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
            return;
        }

        public static void MoveWindow(int X, int Y) {
            UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
            return;
        }

        public static Rect GetWindowRect() {
            UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
            return new Rect();
        }

        public static string WindowName {
            get { 
                UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); 
                return "";
            }
            set { UnitaleUtil.displayLuaError("Windows-only function", "This feature is Windows-only! Sorry, but you can't use it here."); }
        }
#endif

    public void ShakeScreen(float duration, float intensity = 3, bool isIntensityDecreasing = true) {
        if (UnitaleUtil.isOverworld())
            throw new CYFException("You can't use Misc.ScreenShake in the overworld. Use Screen.Rumble instead.");
        else
            Camera.main.GetComponent<GlobalControls>().ShakeScreen(duration, intensity, isIntensityDecreasing);
    }

    public void StopShake() {
        if (UnitaleUtil.isOverworld())
            throw new CYFException("You can't use Misc.StopShake in the overworld.");
        else
            GlobalControls.stopScreenShake = true;
    }

    public bool FullScreen {
        get { return Screen.fullScreen; }
        set { Screen.fullScreen = value; }
    }

    public string MachineName {
        get { return System.Environment.UserName; }
    }

    public static void DestroyWindow() { Application.Quit(); }
}