using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;

[BepInPlugin("com.jukixyo.scrollzoom", "Scroll Zoom", "1.1.0")]
public class ScrollZoomPlugin : BasePlugin
{
    private Harmony _harmony;

    public static ConfigEntry<bool> EnableInterpolation;
    public static ConfigEntry<float> InterpolationSpeed;
    public static ConfigEntry<float> ZoomStepConfig;
    public static ConfigEntry<float> MaxZoomConfig;

    public static ConfigEntry<bool> InvertScroll;
    public static ConfigEntry<string> ResetZoomKey;

    public static ConfigEntry<bool> ResetZoomOnMeeting;

    public override void Load()
    {
        EnableInterpolation = Config.Bind(
            "Zoom",
            "EnableInterpolation",
            true,
            "Enable smooth zoom animation"
        );

        InterpolationSpeed = Config.Bind(
            "Zoom",
            "InterpolationSpeed",
            30f,
            "How fast zoom interpolation reaches the target"
        );

        ZoomStepConfig = Config.Bind(
            "Zoom",
            "ZoomStep",
            1.25f,
            "Zoom multiplier applied per scroll"
        );

        MaxZoomConfig = Config.Bind(
            "Zoom",
            "MaxZoom",
            15f,
            "Maximum zoom level"
        );

        InvertScroll = Config.Bind(
            "Controls",
            "InvertScroll",
            false,
            "Invert mouse scroll direction"
        );

        ResetZoomKey = Config.Bind(
            "Controls",
            "ResetZoomKey",
            "Null",
            "Key used to reset zoom back to default (3). Set to Null to disable."
        );

        ResetZoomOnMeeting = Config.Bind(
            "Gameplay",
            "ResetZoomOnMeeting",
            true,
            "Reset zoom when meetings start"
        );

        _harmony = new Harmony("com.jukixyo.scrollzoom");
        _harmony.PatchAll();

        Log.LogInfo("Scroll Zoom loaded.");
        ModManager.Instance.ShowModStamp();
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update_Patch
{
    private const float MinZoom = 3f;

    private static float _targetZoom = -1f;
    private static float _defaultZoom = -1f;

    static void Postfix()
    {
        if (Camera.main == null)
            return;

        if (!IsInGameplay())
        {
            ResetAllZoomState();
            return;
        }

        if (PlayerControl.LocalPlayer == null)
            return;

        if (MeetingHud.Instance != null)
        {
            if (ScrollZoomPlugin.ResetZoomOnMeeting.Value)
                ResetZoomMeetingSafe();
            return;
        }

        if (Minigame.Instance != null || HudManager.Instance.GameMenu.IsOpen)
            return;

        // chat scroll fix
        if (HudManager.Instance?.Chat != null && HudManager.Instance.Chat.IsOpenOrOpening)
            return;

        HandleResetKey();
        HandleScrollZoom();

        if (ScrollZoomPlugin.EnableInterpolation.Value)
            SmoothZoomStep();
        else
            ApplyZoom(_targetZoom);
    }

    private static bool IsInGameplay()
    {
        return PlayerControl.LocalPlayer != null && ShipStatus.Instance != null;
    }

    private static void HandleResetKey()
    {
        if (ScrollZoomPlugin.ResetZoomKey.Value == "Null")
            return;

        try
        {
            KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), ScrollZoomPlugin.ResetZoomKey.Value);

            if (Input.GetKeyDown(key))
                _targetZoom = MinZoom;
        }
        catch { }
    }

    private static void HandleScrollZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (ScrollZoomPlugin.InvertScroll.Value)
            scroll *= -1f;

        float current = Camera.main.orthographicSize;

        if (_defaultZoom < 0f)
        {
            _defaultZoom = current;
            _targetZoom = current;
            return;
        }

        if (_targetZoom < 0f)
            _targetZoom = current;

        if (scroll != 0f)
        {
            float step = ScrollZoomPlugin.ZoomStepConfig.Value;

            float newSize = scroll > 0
                ? _targetZoom / step
                : _targetZoom * step;

            _targetZoom = Mathf.Clamp(newSize, MinZoom, ScrollZoomPlugin.MaxZoomConfig.Value);
        }
    }

    private static void SmoothZoomStep()
    {
        if (_targetZoom < 0f)
            return;

        float current = Camera.main.orthographicSize;

        float newZoom = Mathf.Lerp(
            current,
            _targetZoom,
            Time.deltaTime * ScrollZoomPlugin.InterpolationSpeed.Value
        );

        // snap to exact step to avoid shadow issues
        if (Mathf.Abs(newZoom - _targetZoom) < 0.02f)
            newZoom = _targetZoom;

        ApplyZoom(newZoom);
    }

    private static void ResetZoomMeetingSafe()
    {
        if (_defaultZoom > 0f)
        {
            if (Camera.main != null)
                Camera.main.orthographicSize = _defaultZoom;

            if (HudManager.Instance != null && HudManager.Instance.UICamera != null)
                HudManager.Instance.UICamera.orthographicSize = _defaultZoom;

            _targetZoom = _defaultZoom;
        }

        if (HudManager.Instance && HudManager.Instance.ShadowQuad)
        {
            bool isDead = PlayerControl.LocalPlayer != null
                        && PlayerControl.LocalPlayer.Data != null
                        && PlayerControl.LocalPlayer.Data.IsDead;

            HudManager.Instance.ShadowQuad.gameObject.SetActive(!isDead);
        }

        ReanchorHud();
    }

    private static void ResetAllZoomState()
    {
        if (_defaultZoom > 0f)
        {
            if (Camera.main != null)
                Camera.main.orthographicSize = _defaultZoom;

            if (HudManager.Instance != null && HudManager.Instance.UICamera != null)
                HudManager.Instance.UICamera.orthographicSize = _defaultZoom;
        }

        _targetZoom = -1f;
        _defaultZoom = -1f;
    }

    private static void ApplyZoom(float size)
    {
        if (Camera.main != null)
            Camera.main.orthographicSize = size;

        if (HudManager.Instance != null && HudManager.Instance.UICamera != null)
            HudManager.Instance.UICamera.orthographicSize = size;

        bool isDead = PlayerControl.LocalPlayer != null
                      && PlayerControl.LocalPlayer.Data != null
                      && PlayerControl.LocalPlayer.Data.IsDead;

        if (HudManager.Instance && HudManager.Instance.ShadowQuad)
        {
            if (isDead)
            {
                HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            }
            else
            {
                bool zoomedOut = _defaultZoom > 0f && size > _defaultZoom;
                HudManager.Instance.ShadowQuad.gameObject.SetActive(!zoomedOut);
            }
        }

        ReanchorHud();
    }

    private static void ReanchorHud()
    {
        if (HudManager.Instance == null)
            return;

        var aspects = HudManager.Instance.GetComponentsInChildren<AspectPosition>(true);

        foreach (var ap in aspects)
            ap.AdjustPosition();
    }
}
