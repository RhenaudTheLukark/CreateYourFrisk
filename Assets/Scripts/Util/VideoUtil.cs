using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class VideoUtil : MonoBehaviour {



    public static DynValue MakeVideoPlayer(string filename, bool infront = true)
    {
       // Video v = GameObject.Instantiate<Video>(SpriteRegistry.GENERIC_VIDEO_PREFAB);
        if (string.IsNullOrEmpty(filename)) {
            throw new CYFException("You can't create a video object with a nil path!");
        }
        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = (infront ? UnityEngine.Video.VideoRenderMode.CameraNearPlane : UnityEngine.Video.VideoRenderMode.CameraFarPlane);

        // This will cause our Scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 1F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.

        //videoPlayer.url = "Mods/"+  +"Videos/" + filename + ".mp4";
        //videoPlayer.url = filename;

        // Nope, we are using assets.
        var res = Resources.Load<UnityEngine.Video.VideoClip>("Videos/" + filename + "");
        if (res == null) {
            throw new CYFException("The video at \"Videos/" + filename + "\" cannot be found!");
        }
        videoPlayer.clip = res;

        // Skip the first 100 frames.
        videoPlayer.frame = 0;

        // Restart from beginning when done.
        videoPlayer.isLooping = true;

        videoPlayer.errorReceived += onVideoError;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        //videoPlayer.loopPointReached += EndReached;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        //videoPlayer.Play();
        videoPlayer.Prepare();

        return UserData.Create(new LuaVideoController(videoPlayer), LuaVideoController.data);
    }

    public static void onVideoError(UnityEngine.Video.VideoPlayer source, string message) {
        throw new CYFException("Something went wrong with a video object! Message: " + message);
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
