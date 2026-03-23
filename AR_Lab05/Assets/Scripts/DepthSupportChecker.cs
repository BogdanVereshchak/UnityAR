using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
/// Перевіряє підтримку Depth/Occlusion на поточному пристрої.
/// Додайте на Main Camera поруч з AROcclusionManager.
/// Сумісний з AR Foundation 5.x (Unity 2022) та 6.x (Unity 6).
public class DepthSupportChecker : MonoBehaviour
{
    [SerializeField] private AROcclusionManager occlusionManager;
    void Awake()
    {
        Debug.Log("[DEPTH] DepthSupportChecker Awake()");
    }
    IEnumerator Start()
    {
        Debug.Log($"[DEPTH] Start() — AR Session state: {ARSession.state}");
        while (ARSession.state < ARSessionState.Ready)
    {
            yield return null;
        }
        Debug.Log($"[DEPTH] AR Session Ready. Чекаємо subsystem...");
        yield return new WaitForSeconds(0.5f);
        if (occlusionManager == null)
        {
            Debug.LogError("[DEPTH] OcclusionManager не призначений в Inspector!");
            yield break;
        }
        var subsystem = occlusionManager.subsystem;
        if (subsystem == null)
        {
            Debug.LogWarning("[DEPTH] Occlusion subsystem not available (XR Sim або пристрій без Depth)");
        yield break;
        }
        Debug.Log($"[DEPTH] Subsystem: {subsystem.GetType().Name}");
        Debug.Log($"[DEPTH] Environment Depth Mode: {occlusionManager.currentEnvironmentDepthMode}");
        Debug.Log($"[DEPTH] Human Stencil: {occlusionManager.currentHumanStencilMode}");
        Debug.Log($"[DEPTH] Human Depth: {occlusionManager.currentHumanDepthMode}");
        if (occlusionManager.currentEnvironmentDepthMode ==
        UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Disabled
        && occlusionManager.currentHumanStencilMode ==
        UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Disabled
        && occlusionManager.currentHumanDepthMode ==
        UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Disabled)
        {
            Debug.LogWarning("[DEPTH] No occlusion mode active — device may not support it");
        }
        else
        {
            Debug.Log("[DEPTH] ✓ Occlusion is active!");
        }
    }
}