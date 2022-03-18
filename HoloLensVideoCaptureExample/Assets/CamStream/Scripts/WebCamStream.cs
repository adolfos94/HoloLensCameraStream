using UnityEngine;
using HoloLensCameraStream;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.MixedReality.Toolkit.UI;

public class WebCamStream : MonoBehaviour
{
    public Interactable cameraButton;
    public Interactable previewButton;

    public GameObject previewQuadTexture;
    private Texture2D webcamTexture;

    private VideoCapture videoCapture;
    private CameraParameters cameraParameters = new CameraParameters();
    private HoloLensCameraStream.Resolution resolution;
    private float[] camera2WorldMatrix;
    private float[] projectionMatrix;
    private byte[] byteBuffer;
    private Matrix4x4 extrinsicsMatrix;
    private Matrix4x4 intrinsicsMatrix;

    private void Start()
    {
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
    }

    private void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        this.videoCapture = videoCapture;

        SetCameraParameters();
        SetNativeSpatialCoordinates();

        GetSupportedResolutions();
    }

    private void SetCameraParameters()
    {
        resolution = CameraStreamHelper.Instance.GetLowestResolution();
        cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
        cameraParameters.cameraResolutionWidth = resolution.width;
        cameraParameters.cameraResolutionHeight = resolution.height;
        cameraParameters.frameRate = resolution.frameRate;

        // Preview Texture.
        webcamTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false);
        previewQuadTexture.transform.localScale = new Vector3(resolution.aspectRatio, -1, 1);
        previewQuadTexture.GetComponent<Renderer>().material.mainTexture = webcamTexture;
    }

    [Obsolete]
    private void SetNativeSpatialCoordinates()
    {
#if UNITY_WSA
        IntPtr spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(spatialCoordinateSystemPtr);
#endif
    }

    private void GetSupportedResolutions()
    {
        if (videoCapture == null)
            return;

        foreach (var resolution in videoCapture.GetSupportedResolutions().ToList())
            Debug.Log(resolution.ToString());
    }

    public void StartWebCam()
    {
        if (videoCapture.IsStreaming)
            return;

        videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;
        videoCapture.StartVideoModeAsync(cameraParameters, OnVideoModeStarted);
    }

    private void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (!result.success)
            Debug.LogError("ERROR STARTING WEBCAM: " + result.resultType.ToString());

        cameraButton.IsToggled = result.success;
    }

    public void StopWebCam()
    {
        if (!videoCapture.IsStreaming)
            return;

        videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
        videoCapture.StopVideoModeAsync(OnVideoModeStopped);
    }

    private void OnVideoModeStopped(VideoCaptureResult result)
    {
        if (!result.success)
            Debug.LogError("ERROR STOPPING WEBCAM: " + result.resultType.ToString());

        cameraButton.IsToggled = !result.success;
    }

    public void ShowCameraStream()
    {
        previewQuadTexture.SetActive(previewButton.IsToggled);
    }

    private void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        if (byteBuffer == null || byteBuffer.Length < sample.dataLength)
            byteBuffer = new byte[sample.dataLength];

        if (!sample.CopyRawImageDataIntoBuffer(byteBuffer) ||
            !sample.TryGetCameraToWorldMatrix(out camera2WorldMatrix) ||
            !sample.TryGetProjectionMatrix(out projectionMatrix))
            return;

        intrinsicsMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionMatrix);
        extrinsicsMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(camera2WorldMatrix);

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            webcamTexture.LoadRawTextureData(byteBuffer);
            webcamTexture.Apply();
        }, true);

        sample.Dispose();
    }
}