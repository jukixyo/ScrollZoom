using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

[BepInPlugin("com.jukixyo.scrollzoom", "Scroll Zoom", "1.0.1")]
public class ScrollZoomPlugin : BasePlugin
{
    private Harmony _harmony;

    public override void Load()
    {
        _harmony = new Harmony("com.jukixyo.scrollzoom");
        _harmony.PatchAll();

        Log.LogInfo("Thank you for using Scroll Zoom!");
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
        if (PlayerControl.LocalPlayer == null)
            return false;

        return ShipStatus.Instance != null;
    }

    private static void HandleScrollZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float current = Camera.main.orthographicSize;

        if (_defaultZoom < 0f)
            _defaultZoom = current;

        if (_targetZoom < 0f)
            _targetZoom = current;

        if (scroll != 0f)
        {
            float newSize = scroll > 0 ? _targetZoom / ZoomStep : _targetZoom * ZoomStep;
            _targetZoom = Mathf.Clamp(newSize, MinZoom, MaxZoom);

            ApplyZoom(_targetZoom, allowUIRefresh: true);
        }
    }

    private static void ResetZoomMeetingSafe()
    {
        if (_defaultZoom > 0f)
        {
            Camera.main.orthographicSize = _defaultZoom;
            foreach (var cam in Camera.allCameras)
                cam.orthographicSize = _defaultZoom;

            _targetZoom = _defaultZoom;
        }
    }

    private static void ResetAllZoomState()
    {
        if (_defaultZoom > 0f)
        {
            Camera.main.orthographicSize = _defaultZoom;
            foreach (var cam in Camera.allCameras)
                cam.orthographicSize = _defaultZoom;
        }

        _targetZoom = -1f;
        _defaultZoom = -1f;
    }

    private static void ApplyZoom(float size, bool allowUIRefresh)
    {
        Camera.main.orthographicSize = size;
        foreach (var cam in Camera.allCameras)
            cam.orthographicSize = size;

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

        if (!allowUIRefresh || MeetingHud.Instance != null)
            return;

        ResolutionManager.ResolutionChanged.Invoke(
            (float)Screen.width / Screen.height,
            Screen.width,
            Screen.height,
            Screen.fullScreen
        );

        foreach (var ap in Object.FindObjectsOfType<AspectPosition>())
            ap.AdjustPosition();
    }
}
