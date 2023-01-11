/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using UnityEngine;
using Vuforia;

/// <summary>
/// A custom handler that registers for Vuforia initialization errors
/// 
/// Changes made to this file could be overwritten when upgrading the Vuforia version. 
/// When implementing custom error handler behavior, consider inheriting from this class instead.
/// </summary>
public class DefaultInitializationErrorHandler : VuforiaMonoBehaviour
{
    public void OnVuforiaInitializationError(VuforiaInitError vuforiaInitError)
    {
        if (vuforiaInitError != VuforiaInitError.NONE)
        {
            SetErrorCode(vuforiaInitError);
            SetErrorOccurred(true);
        }
    }

    string mErrorText = "";
    bool mErrorOccurred;

    const string headerLabel = "Vuforia Engine Initialization Error";

    GUIStyle bodyStyle;
    GUIStyle headerStyle;
    GUIStyle footerStyle;

    Texture2D bodyTexture;
    Texture2D headerTexture;
    Texture2D footerTexture;
    
    void Awake()
    {
        // Check for an initialization error on start.
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitializationError;
    }

    void Start()
    {
        SetupGUIStyles();
    }

    void OnGUI()
    {
        // On error, create a full screen window.
        if (mErrorOccurred)
            GUI.Window(0, new Rect(0, 0, Screen.width, Screen.height), DrawWindowContent, "");
    }

    /// <summary>
    ///     When this game object is destroyed, it unregisters itself as event handler
    /// </summary>
    void OnDestroy()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized -= OnVuforiaInitializationError;
    }

    void DrawWindowContent(int id)
    {
        var headerRect = new Rect(0, 0, Screen.width, Screen.height / 8);
        var bodyRect = new Rect(0, Screen.height / 8, Screen.width, Screen.height / 8 * 6);
        var footerRect = new Rect(0, Screen.height - Screen.height / 8, Screen.width, Screen.height / 8);

        GUI.Label(headerRect, headerLabel, headerStyle);
        GUI.Label(bodyRect, mErrorText, bodyStyle);

        if (GUI.Button(footerRect, "Close", footerStyle))
        {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
#endif
        }
    }

    void SetErrorCode(VuforiaInitError initError)
    {
        switch (initError)
        {   
            // case InitCode.INIT_EXTERNAL_DEVICE_NOT_DETECTED:
            //     mErrorText =
            //         "Failed to initialize the Vuforia Engine because this " +
            //         "device is not docked with required external hardware.";
            //     break;
            case VuforiaInitError.LICENSE_CONFIG_MISSING_KEY:
                mErrorText =
                    "Vuforia Engine App key is missing. Please get a valid key " +
                    "by logging into your account at developer.vuforia.com " +
                    "and creating a new project.";
                break;
            case VuforiaInitError.LICENSE_CONFIG_INVALID_KEY:
                mErrorText =
                    "Vuforia Engine App key is invalid. " +
                    "Please get a valid key by logging into your account at " +
                    "developer.vuforia.com and creating a new project. \n\n" +
                    getKeyInfo();
                break;
            case VuforiaInitError.LICENSE_CONFIG_NO_NETWORK_TRANSIENT:
                mErrorText = "Unable to contact server. Please try again later.";
                break;
            case VuforiaInitError.LICENSE_CONFIG_NO_NETWORK_PERMANENT:
                mErrorText = "No network available. Please make sure you are connected to the Internet.";
                break;
            case VuforiaInitError.LICENSE_CONFIG_KEY_CANCELED:
                mErrorText =
                    "This App license key has been cancelled and may no longer be used. " +
                    "Please get a new license key. \n\n" +
                    getKeyInfo();
                break;
            case VuforiaInitError.LICENSE_CONFIG_PRODUCT_TYPE_MISMATCH:
                mErrorText =
                    "Vuforia Engine App key is not valid for this product. Please get a valid key " +
                    "by logging into your account at developer.vuforia.com and choosing the " +
                    "right product type during project creation. \n\n" +
                    getKeyInfo() + " \n\n" +
                    "Note that Universal Windows Platform (UWP) apps require " +
                    "a license key created on or after August 9th, 2016.";
                break;
            case VuforiaInitError.DEVICE_NOT_SUPPORTED:
                mErrorText = "Failed to initialize Vuforia Engine because this device is not supported.";
                break;
            case VuforiaInitError.PERMISSION_ERROR:
                mErrorText =
                    "One or more permissions required by Vuforia Engine are missing or not granted by user.\n" +
                    "For example, the user may have denied camera access to this app.\n" +
                    "In this case, you can enable camera access in Settings:\n" +
                    "Settings > Privacy > Camera > " + Application.productName + "\n" +
                    "Also verify that the camera is enabled in:\n" +
                    "Settings > General > Restrictions.";
                break;
            case VuforiaInitError.LICENSE_ERROR:
                mErrorText = "A valid license configuration is required.\n";
                break;
            case VuforiaInitError.INITIALIZATION:
            default:
                mErrorText = "Failed to initialize Vuforia Engine.";
                break;
        }

        // Prepend the error code in red
        mErrorText = "<color=red>" + initError.ToString().Replace("_", " ") + "</color>\n\n" + mErrorText;

        // Remove rich text tags for console logging
        var errorTextConsole = mErrorText.Replace("<color=red>", "").Replace("</color>", "");

        Debug.LogError("Vuforia Engine initialization failed: " + initError + "\n\n" + errorTextConsole);
    }

    void SetErrorOccurred(bool errorOccurred)
    {
        mErrorOccurred = errorOccurred;
    }

    string getKeyInfo()
    {
        string key = VuforiaConfiguration.Instance.Vuforia.LicenseKey;
        string keyInfo;
        if (key.Length > 10)
            keyInfo =
                "Your current key is <color=red>" + key.Length + "</color> characters in length. " +
                "It begins with <color=red>" + key.Substring(0, 5) + "</color> " +
                "and ends with <color=red>" + key.Substring(key.Length - 5, 5) + "</color>.";
        else
            keyInfo =
                "Your current key is <color=red>" + key.Length + "</color> characters in length. \n" +
                "The key is: <color=red>" + key + "</color>.";
        return keyInfo;
    }

    void SetupGUIStyles()
    {
        // Called from Start() to determine physical size of device for text sizing
        var shortSidePixels = Screen.width < Screen.height ? Screen.width : Screen.height;
        var shortSideInches = shortSidePixels / Screen.dpi;
        var physicalSizeMultiplier = shortSideInches > 4.0f ? 2 : 1;

        // Create 1x1 pixel background textures for body, header, and footer
        bodyTexture = CreateSinglePixelTexture(Color.white);
        headerTexture = CreateSinglePixelTexture(new Color(
            Mathf.InverseLerp(0, 255, 220),
            Mathf.InverseLerp(0, 255, 220),
            Mathf.InverseLerp(0, 255, 220))); // RGB(220)
        footerTexture = CreateSinglePixelTexture(new Color(
            Mathf.InverseLerp(0, 255, 35),
            Mathf.InverseLerp(0, 255, 178),
            Mathf.InverseLerp(0, 255, 0))); // RGB(35,178,0)

        // Create body style and set values
        bodyStyle = new GUIStyle();
        bodyStyle.normal.background = bodyTexture;
        bodyStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bodyStyle.fontSize = (int) (18 * physicalSizeMultiplier * Screen.dpi / 160);
        bodyStyle.normal.textColor = Color.black;
        bodyStyle.wordWrap = true;
        bodyStyle.alignment = TextAnchor.MiddleCenter;
        bodyStyle.padding = new RectOffset(40, 40, 0, 0);

        // Duplicate body style and change necessary values
        headerStyle = new GUIStyle(bodyStyle);
        headerStyle.normal.background = headerTexture;
        headerStyle.fontSize = (int) (24 * physicalSizeMultiplier * Screen.dpi / 160);

        // Duplicate body style and change necessary values
        footerStyle = new GUIStyle(bodyStyle);
        footerStyle.normal.background = footerTexture;
        footerStyle.normal.textColor = Color.white;
        footerStyle.fontSize = (int) (28 * physicalSizeMultiplier * Screen.dpi / 160);
    }

    Texture2D CreateSinglePixelTexture(Color color)
    {
        // Called by SetupGUIStyles() to create 1x1 texture
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
