using UnityEngine;
using HoloLensCameraStream;
using System.Linq;
using System.Collections.Generic;

public class WebCamStream : MonoBehaviour
{
    public GameObject cameraButton;
    public GameObject previewButton;

    private VideoCapture videoCapture;
    public List<HoloLensCameraStream.Resolution> supportedResolution;

    private void Start()
    {
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
    }

    private void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        this.videoCapture = videoCapture;

        GetSupportedResolutions();
    }

    private void GetSupportedResolutions()
    {
        if (videoCapture == null)
            return;

        supportedResolution = videoCapture.GetSupportedResolutions()
            .ToList<HoloLensCameraStream.Resolution>();

        foreach(var resolution in supportedResolution)
        {
            Debug.Log(resolution.ToString());
        }

    }

}
