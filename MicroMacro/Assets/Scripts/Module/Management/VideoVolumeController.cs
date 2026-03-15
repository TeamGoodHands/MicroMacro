using UnityEngine;
using UnityEngine.Video;
using Module.Management;

public class VideoVolumeController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    void Start()
    {
        // VolumeManagerから現在のMaster音量（リニア値 0-1）を取得
        float masterVolume = VolumeManager.GetVolume(VolumeManager.ParamMaster, 1.0f);
        
        // VideoPlayerの0番目のトラックに音量を適用
        videoPlayer.SetDirectAudioVolume(0, masterVolume);
    }
}