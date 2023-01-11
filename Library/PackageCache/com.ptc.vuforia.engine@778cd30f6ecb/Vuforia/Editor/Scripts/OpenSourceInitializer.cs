/*===============================================================================
Copyright (c) 2017-2019 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
#if UNITY_2020_1_OR_NEWER
using UnityEngine.XR;
#endif
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vuforia;
using Vuforia.EditorClasses;
using Vuforia.UnityRuntimeCompiled;

/// <summary>
/// Creates connection between open source files and the Vuforia library.
/// Do not modify.
/// </summary>
[InitializeOnLoad]
public static class OpenSourceInitializer
{
    static IUnityEditorFacade sFacade;

    static OpenSourceInitializer()
    {
        GameObjectFactory.SetDefaultBehaviourTypeConfiguration(new DefaultBehaviourAttacher());
        ReplacePlaceHolders();

        InitializeFacade();
        ARFoundationInitializer.InitializeFacade();
    }

    static void ReplacePlaceHolders()
    {
        var observerPlaceholders = Object.FindObjectsOfType<DefaultObserverBehaviourPlaceholder>().ToList();
        var initErrorsPlaceholders = Object.FindObjectsOfType<DefaultInitializationErrorHandlerPlaceHolder>().ToList();

        observerPlaceholders.ForEach(ReplaceObserverPlaceHolder);
        initErrorsPlaceholders.ForEach(ReplaceInitErrorPlaceHolder);
    }
    
    static void ReplaceObserverPlaceHolder(DefaultObserverBehaviourPlaceholder placeHolder)
    {
        var go = placeHolder.gameObject;
        var doeh = go.AddComponent<DefaultObserverEventHandler>();
        SetDefaultObserverHandlerSettings(doeh);

        Object.DestroyImmediate(placeHolder);
    }

    static void ReplaceInitErrorPlaceHolder(DefaultInitializationErrorHandlerPlaceHolder placeHolder)
    {
        var go = placeHolder.gameObject;
        go.AddComponent<DefaultInitializationErrorHandler>();

        Object.DestroyImmediate(placeHolder);
    }

    class DefaultBehaviourAttacher : IDefaultBehaviourAttacher
    {
        public void AddDefaultObserverEventHandler(GameObject go)
        {
            var dteh = go.AddComponent<DefaultObserverEventHandler>();
            SetDefaultObserverHandlerSettings(dteh);
        }

        public void AddDefaultAreaTargetEventHandler(GameObject go)
        {
            var eventHandler = go.AddComponent<DefaultAreaTargetEventHandler>();
            SetDefaultObserverHandlerSettings(eventHandler);
        }
        
        public void AddDefaultInitializationErrorHandler(GameObject go)
        {
            go.AddComponent<DefaultInitializationErrorHandler>();
        }
    }

    static void SetDefaultObserverHandlerSettings(DefaultObserverEventHandler doeh)
    {
        if (doeh.gameObject.GetComponent<AnchorBehaviour>() != null)
        {
            // render anchors in LIMITED mode
            doeh.StatusFilter = DefaultObserverEventHandler.TrackingStatusFilter.Tracked_ExtendedTracked_Limited;
        }
        else
        {
            // the default for all other targets is not to consider LIMITED poses
            doeh.StatusFilter = DefaultObserverEventHandler.TrackingStatusFilter.Tracked_ExtendedTracked;
        }
    }
    
    static void InitializeFacade()
    {
        if (sFacade != null) return;

        sFacade = new OpenSourceUnityEditorFacade();
        UnityEditorFacade.Instance = sFacade;
    }

    class OpenSourceUnityEditorFacade : IUnityEditorFacade
    {
        public bool IsTargetingHoloLens()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WSAPlayer)
                return false;
#if UNITY_2020_1_OR_NEWER
            const string UNITY_WINDOWSMR_IDENTIFIER = "Windows Mixed Reality";
            return XRSettings.supportedDevices.Any(xrDevice => xrDevice.Contains(UNITY_WINDOWSMR_IDENTIFIER));
#else
            const string UNITY_WINDOWSMR_IDENTIFIER = "WindowsMR";
            if (!PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.WSA)) return false;
            foreach (var vrSdkName in PlayerSettings.GetVirtualRealitySDKs(BuildTargetGroup.WSA))
            {
                if (vrSdkName.Equals(UNITY_WINDOWSMR_IDENTIFIER))
                {
                    return true;
                }
            }
            return false;
#endif
        }
    }
}
