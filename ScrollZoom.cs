using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

[BepInPlugin("com.jukixyo.scrollzoom", "Scroll Zoom", "1.0.0")]
public class ScrollZoomPlugin : BasePlugin
{
    private Harmony _harmony;

    public override void Load()
    {
        _harmony = new Harmony("com.jukixyo.scrollzoom");
        _harmony.PatchAll();

        Log.LogInfo("Scroll Zoom loaded (instant zoom, no easing).");
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
        if (PlayerControl.LocalPlayer == null || Camera.main == null)
            return;

        if (MeetingHud.Instance || Minigame.Instance || HudManager.Instance.GameMenu.IsOpen)
            return;

        HandleScrollZoom();
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

            ApplyZoom(_targetZoom);
        }
    }

    private static void ApplyZoom(float size)
    {
        Camera.main.orthographicSize = size;
        foreach (var cam in Camera.allCameras)
            cam.orthographicSize = size;

        bool zoomedOut = _defaultZoom > 0f && size > _defaultZoom;

        if (HudManager.Instance && HudManager.Instance.ShadowQuad)
            HudManager.Instance.ShadowQuad.gameObject.SetActive(!zoomedOut);

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
