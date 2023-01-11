/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Vuforia.ARFoundation;
using Vuforia.Internal.VuforiaDriver;

#if ARFOUNDATION_DEFINED
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace Vuforia.UnityRuntimeCompiled
{
    public static class ARFoundationInitializer
    {
        static OpenSourceARFoundationFacade sFacade;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnAfterAssembliesLoaded()
        {
            InitializeFacade();
        }

        public static void InitializeFacade()
        {
            if (sFacade != null) return;

            sFacade = new OpenSourceARFoundationFacade();
            ARFoundationFacade.Instance = sFacade;
        }
    }

    class OpenSourceARFoundationFacade : IARFoundationFacade
    {
        bool mIsAnchorSupported;
#if ARFOUNDATION_DEFINED
        ARCameraManager   mCameraManager;
        ARAnchorManager   mAnchorManager;
        ARSession         mSession;
        ARSessionOrigin   mSessionOrigin;
        ARRaycastManager  mRaycastManager;

        Dictionary<string, ARAnchor> mAnchors = new Dictionary<string, ARAnchor>();
#endif
        public event Action<ARFoundationImage> ARFoundationImageEvent = image => { };
        public event Action<Transform, long> ARFoundationPoseEvent = (pose, timestamp) => { };

        public event Action<List<Tuple<string, Transform>>, List<Tuple<string, Transform>>> AnchorsChangedEvent = (removed, updated) => {};

        public bool IsAnchorSupported => mIsAnchorSupported;

        public bool IsARFoundationScene()
        {
#if ARFOUNDATION_DEFINED
            var arSession = GameObject.FindObjectOfType<ARSession>();
            return arSession != null;
#else
            return false;
#endif
        }

        public IEnumerator CheckAvailability()
        {
#if ARFOUNDATION_DEFINED
            var anchorDescriptors = new List<XRAnchorSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(anchorDescriptors);
            mIsAnchorSupported = anchorDescriptors.Count > 0;

            yield return ARSession.CheckAvailability();
#else
            yield break;
#endif
        }

        public void Init()
        {
#if ARFOUNDATION_DEFINED
            mCameraManager = GameObject.FindObjectOfType<ARCameraManager>();
            mSession = GameObject.FindObjectOfType<ARSession>();
            mSessionOrigin = GameObject.FindObjectOfType<ARSessionOrigin>();
            mRaycastManager = GameObject.FindObjectOfType<ARRaycastManager>();

            if (mIsAnchorSupported)
            {
                mAnchorManager = mSessionOrigin.GetComponent<ARAnchorManager>();
                if (mAnchorManager == null)
                    mAnchorManager = mSessionOrigin.gameObject.AddComponent<ARAnchorManager>();
                mAnchorManager.anchorsChanged += OnAnchorsChanged;
            }
            UnityEngine.Application.onBeforeRender += UpdateStateFromARFoundationFrame;
#endif
        }

        public void Deinit()
        {
            // ClearAnchors();
#if ARFOUNDATION_DEFINED
            mSession.Reset();
            if (mIsAnchorSupported)
                mAnchorManager.anchorsChanged -= OnAnchorsChanged;
            UnityEngine.Application.onBeforeRender -= UpdateStateFromARFoundationFrame;
#endif
        }

        public IEnumerator WaitForCameraReady()
        {
#if ARFOUNDATION_DEFINED
            var waitForEndOfFrame = new WaitForEndOfFrame();
            while (mCameraManager == null || mCameraManager.subsystem == null || !mCameraManager.subsystem.running ||
                !mCameraManager.permissionGranted)
            {
                yield return waitForEndOfFrame;
            }
#else
            yield break;
#endif
        }

        public bool IsARFoundationReady()
        {
#if ARFOUNDATION_DEFINED
            return ARSession.state >= ARSessionState.Ready;
#else
            return false;
#endif
        }

        public Transform GetCameraTransform()
        {
#if ARFOUNDATION_DEFINED
            if (!mCameraManager)
                mCameraManager = GameObject.FindObjectOfType<ARCameraManager>();
            return mCameraManager.transform;
#else
            return null;
#endif
        }

        public List<DriverCameraMode> GetProfiles()
        {
            var profiles = new List<DriverCameraMode>();
#if ARFOUNDATION_DEFINED
            using (var configurations = mCameraManager.GetConfigurations(Allocator.Temp))
            {
                if (!configurations.IsCreated || configurations.Length <= 0)
                    return profiles;

                foreach (var configuration in configurations)
                {
                    profiles.Add(new DriverCameraMode
                    (
                        configuration.width,
                        configuration.height,
                        configuration.framerate ?? 30,
#if UNITY_IOS
                        DriverPixelFormat.NV12
#elif UNITY_ANDROID
                        DriverPixelFormat.NV21
#else
                        DriverPixelFormat.UNKNOWN
#endif
                    ));
                }
            }
#endif
            return profiles;
        }

        public bool SelectProfile(DriverCameraMode profile)
        {
#if ARFOUNDATION_DEFINED
            using (var configurations = mCameraManager.GetConfigurations(Allocator.Temp))
            {
                if (!configurations.IsCreated || configurations.Length <= 0)
                    return false;

                var configs = new SortedDictionary<int, List<XRCameraConfiguration>>();
                foreach (var configuration in configurations)
                {
                    var framerate = configuration.framerate ?? 30;
                    if (!configs.ContainsKey(framerate))
                        configs.Add(framerate, new List<XRCameraConfiguration>());
                    configs[framerate].Add(configuration);
                }

                var selectedConfiguration = configs[profile.Fps]
                    .First(x => x.width == profile.Width && x.height == profile.Height);

                if (mCameraManager.currentConfiguration != selectedConfiguration)
                    mCameraManager.currentConfiguration = selectedConfiguration;
            }
            return true;
#else
            return false;
#endif
        }

#if ARFOUNDATION_DEFINED
        private void UpdateStateFromARFoundationFrame()
        {
            if (!mCameraManager.TryGetIntrinsics(out var cameraIntrinsics))
                return;
            if (!mCameraManager.TryAcquireLatestCpuImage(out var cameraImage))
                return;

            var timestamp = (long)(cameraImage.timestamp * 1000000000);
            ARFoundationPoseEvent(mCameraManager.transform, timestamp);

            var image = new ARFoundationImage(
                cameraImage.dimensions,
                cameraImage.GetPlane(0).data,
                cameraImage.GetPlane(1).data,
#if UNITY_ANDROID
                cameraImage.GetPlane(2).data,
#else
                new NativeArray<byte>(new byte[0], Allocator.None),
#endif
                cameraImage.GetPlane(0).rowStride,
                cameraImage.GetPlane(1).rowStride,
                cameraImage.GetPlane(1).pixelStride,
                timestamp,
                cameraIntrinsics.principalPoint,
                cameraIntrinsics.focalLength
            );

            ARFoundationImageEvent.Invoke(image);
            cameraImage.Dispose();
        }

        void OnAnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
        {
            var removed = new List<Tuple<string, Transform>>();
            foreach (var anchor in eventArgs.removed)
            {
                var uuid = anchor.trackableId.ToString();
                if (mAnchors.ContainsKey(uuid))
                {
                    removed.Add(Tuple.Create(uuid,anchor.transform));
                    mAnchors.Remove(uuid);
                }
            }
            var updated = new List<Tuple<string, Transform>>();
            foreach (var anchor in eventArgs.updated)
            {
                var uuid = anchor.trackableId.ToString();
                if (mAnchors.ContainsKey(uuid))
                {
                    updated.Add(Tuple.Create(uuid,anchor.transform));
                }
            }
            AnchorsChangedEvent.Invoke(removed, updated);
        }
#endif

        public string AddAnchor(Pose pose)
        {
#if ARFOUNDATION_DEFINED
            var anchor = mAnchorManager.AddAnchor(pose);
            if (anchor == null) return null;

            var id = anchor.trackableId.ToString();
            mAnchors[id] = anchor;
            return id;
#else
            return null;
#endif
        }

        public bool RemoveAnchor(string uuid)
        {
#if ARFOUNDATION_DEFINED
            if (mAnchors.ContainsKey(uuid))
            {
                if (mAnchors[uuid] && mAnchorManager.subsystem != null && mAnchorManager.subsystem.running)
                    mAnchorManager.RemoveAnchor(mAnchors[uuid]);
                mAnchors.Remove(uuid);
            }
            return true;
#else
            return false;
#endif
        }

        public void ClearAnchors()
        {
#if ARFOUNDATION_DEFINED
            foreach (var anchor in mAnchors)
                mAnchorManager.RemoveAnchor(anchor.Value);
            mAnchors.Clear();
#endif
        }

        public bool HitTest(Vector2 screenPoint, out List<Pose> hitPoses)
        {
#if ARFOUNDATION_DEFINED
            var hits = new List<ARRaycastHit>();
            var hitSuccess = mRaycastManager.Raycast(screenPoint, hits, TrackableType.PlaneWithinPolygon);
            hitPoses = hits.ConvertAll(hit => hit.pose);
            return hitSuccess;
#else
            hitPoses = new List<Pose>();
            return false;
#endif
        }
    }
}

