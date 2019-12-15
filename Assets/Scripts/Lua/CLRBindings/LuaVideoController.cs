using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.UI;

public class LuaVideoController : MonoBehaviour {

    public UnityEngine.Video.VideoPlayer _vid;
    internal UnityEngine.Video.VideoPlayer vid { // A image that returns the real image. We use this to be able to detect if the real image is null, and if it is, throw an exception
        get
        {
            if (_vid == null) {
                throw new CYFException("Attempted to perform action on removed video.");
            }
            if (!_vid.gameObject.activeInHierarchy) {
                throw new CYFException("Attempted to perform action on removed video.");
            }
            return _vid;
        }
        set { _vid = value; }
    }

    private Hashtable aspect_ratios = new Hashtable();// = new Dictionary<string, UnityEngine.Video.VideoAspectRatio>();


    public static MoonSharp.Interpreter.Interop.IUserDataDescriptor data = UserData.GetDescriptorForType<LuaVideoController>(true);

    public LuaVideoController(UnityEngine.Video.VideoPlayer v)
    {
        vid = v;
        Start();
    }

    public bool playonawake {
        get {
            return vid.playOnAwake;
        }
        set {
            vid.playOnAwake = value;
        }
    }

    public bool isinfront {
        get {
            return vid.renderMode == UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        }
        set {
            vid.renderMode = (value ? UnityEngine.Video.VideoRenderMode.CameraNearPlane : UnityEngine.Video.VideoRenderMode.CameraFarPlane);
        }
    }

    public float alpha {
        get {
            return vid.targetCameraAlpha;
        }
        set {
            if (value >= 1)
            {
                vid.targetCameraAlpha = 1F;
            }
            else if (value <= 0)
            {
                vid.targetCameraAlpha = 0F;
            } else {
                vid.targetCameraAlpha = value;
            }
        }
    }

    public long currentframe {
        get {
            return vid.frame;
        }
        set {
            vid.frame = value;
        }
    }

    public bool islooping {
        get {
            return vid.isLooping;
        }
        set {
            vid.isLooping = value;
        }
    }

    public float speed {
        get {
            if (vid.canSetPlaybackSpeed)
            {
                return vid.targetCameraAlpha;
            }
            else {
                return 0;
            }
        }
        set {
            if (vid.canSetPlaybackSpeed) {
                if (value >= 3)
                {
                    vid.targetCameraAlpha = 3F;
                }
                else if (value <= -3)
                {
                    vid.targetCameraAlpha = -3F;
                }
                else
                {
                    vid.targetCameraAlpha = value;
                }
            }
        }
    }

    public void Prepare() {
        if (!isprepared) {
            vid.Prepare();
        }
    }

    public bool isprepared {
        get {
            return vid.isPrepared;
        }
    }

    public void Play() {
        vid.Play();
    }

    public void Pause() {
        vid.Pause();
    }

    /*
    public bool ispaused {
        get {
            return vid.isPaused;
        }
    }*/

    public bool isplaying {
        get {
            return vid.isPlaying;
        }
    }

    public bool isactive {
        get {
            return vid.gameObject.activeInHierarchy;
        }
    }

    public void Remove() {
        vid.Stop();
        Destroy(vid);
    }

    public string aspectratio {
        get {
            foreach (DictionaryEntry entry in aspect_ratios) {
                if (vid.aspectRatio == (UnityEngine.Video.VideoAspectRatio)entry.Value) {
                    return (string)entry.Key;
                }
            }
            throw new CYFException("Something went wrong while trying to read the video object's aspect ratio.");
        }
        set {
            if (aspect_ratios.ContainsKey(value)) {
                vid.aspectRatio = (UnityEngine.Video.VideoAspectRatio)aspect_ratios[(string)value];
            } else {
                throw new CYFException("Invalid video aspectratio value \"" + value + "\". See the documentation for valid values.");
            }
        }
    }

    // Use this for initialization
    void Start () {       
        aspect_ratios.Add("NoScaling", UnityEngine.Video.VideoAspectRatio.NoScaling);
        aspect_ratios.Add("FitVertically", UnityEngine.Video.VideoAspectRatio.FitVertically);
        aspect_ratios.Add("FitHorizontally", UnityEngine.Video.VideoAspectRatio.FitHorizontally);
        aspect_ratios.Add("FitInside", UnityEngine.Video.VideoAspectRatio.FitInside);
        aspect_ratios.Add("FitOutside", UnityEngine.Video.VideoAspectRatio.FitOutside);
        aspect_ratios.Add("Stretch", UnityEngine.Video.VideoAspectRatio.Stretch);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
