using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("com.jukixyo.scrollzoom", "Scroll Zoom", "1.0.3")]
public class ScrollZoomPlugin : BasePlugin
{
    private Harmony _harmony;

    public override void Load()
    {
        _harmony = new Harmony("com.jukixyo.scrollzoom");
        _harmony.PatchAll();
        Log.LogInfo("Scroll Zoom loaded.");
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update_Patch
{
    private const float MinZoom = 3f;
    private const float MaxZoom = 15f;
    private const float ZoomStep = 1.25f;

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
            ResetZoomMeetingSafe();
            return;
        }

        if (Minigame.Instance != null || HudManager.Instance.GameMenu.IsOpen)
            return;

        HandleScrollZoom();
    }

    private static bool IsInGameplay()
    {
        return PlayerControl.LocalPlayer != null && ShipStatus.Instance != null;
    }

    private static void HandleScrollZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float current = Camera.main.orthographicSize;

        if (_defaultZoom < 0f && PlayerControl.LocalPlayer != null && ShipStatus.Instance != null)
        {
            _defaultZoom = current;
            _targetZoom = current;
            return; // don't zoom on same frame as init
        }

        if (_targetZoom < 0f)
            _targetZoom = current;

        if (scroll != 0f)
        {
            float newSize = scroll > 0 ? _targetZoom / ZoomStep : _targetZoom * ZoomStep;
            _targetZoom = Mathf.Clamp(newSize, MinZoom, MaxZoom);
            ApplyZoom(_targetZoom);
        }
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
        {
            ap.AdjustPosition();
        }
    }
}
