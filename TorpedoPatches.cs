using System.Collections.Generic;
using HarmonyLib;
using SeaPower;
using SeaPower.Torpedo_States;
using UnityEngine;

namespace TorpedoSearchModes;

internal static class TorpedoRules
{
    // Only touch torpedoes whose search mode the player can choose, so AI/default behaviour stays vanilla.
    public static bool HasSelectableModes(Torpedo t) =>
        t._ap?._availableSearchModes != null && t._ap._availableSearchModes.Count > 1;

    public static bool IsSearchStage(WeaponBase.FlightStage s) =>
        s == WeaponBase.FlightStage.EnterSearchMode
        || s == WeaponBase.FlightStage.PerformSnakeSearch
        || s == WeaponBase.FlightStage.PerformCircleSearch
        || s == WeaponBase.FlightStage.PerformLadderSearch
        || s == WeaponBase.FlightStage.SpiralSearchDown
        || s == WeaponBase.FlightStage.SpiralSearchUp
        || s == WeaponBase.FlightStage.Reattack;
}

// Search phases (and EnterSearchMode, which runs at MaxVelocity) ignore the launch speed, which burns
// the 30 kn fuel budget at the 45 kn rate. Cap them to the transit speed (chosen or auto-picked at launch)
// until a target is held.
[HarmonyPatch(typeof(Torpedo), nameof(Torpedo.moveForward))]
internal static class TorpedoSearchSpeedPatch
{
    private static void Prefix(Torpedo __instance, ref float velocityInKnots)
    {
        if (!TorpedoRules.HasSelectableModes(__instance)
            || !TorpedoRules.IsSearchStage(__instance._flightStage)
            || __instance._ap._torpedoSpeedSettings.Count == 0
            || __instance.CurrentStrongestTargetEcho != null)
            return;
        velocityInKnots = Mathf.Min(velocityInKnots, __instance.TransitVelocityInKnots);
    }
}

// "Straight" (SearchMode None) goes to TerminalApproach right at the aim point; with nothing acquired
// it passes the aim point and drops into the circling Reattack. Keep it running straight on its
// heading at transit speed until it acquires something or runs out of fuel.
// Once it has held a contact, it behaves like vanilla: on losing the contact it circles (Reattack).
[HarmonyPatch(typeof(TerminalApproach), nameof(TerminalApproach.onUpdate))]
internal static class StraightRunPatch
{
    // Torpedo instances are pooled, the flag is cleared on each launch (see TorpedoInitParamsPatch).
    internal static readonly HashSet<int> HadContact = new HashSet<int>();

    private static readonly AccessTools.FieldRef<TerminalApproach, Torpedo> TorpedoRef =
        AccessTools.FieldRefAccess<TerminalApproach, Torpedo>("torpedo");

    private const float AimLeadUnity = 27.559496f; // 1 nm

    private static bool Prefix(TerminalApproach __instance)
    {
        Torpedo t = TorpedoRef(__instance);
        if (t == null || !TorpedoRules.HasSelectableModes(t)
            || t._ap.SearchModeSetting.Value != AmmunitionParameters.SearchMode.None
            || t._onWire
            || t._seeker == null
            || t._seeker._guidanceType == AmmunitionParameters.GuidanceType.None
            || t._seeker._guidanceType == AmmunitionParameters.GuidanceType.WakeHoming)
            return true;

        int id = t.GetInstanceID();
        if (t._seeker._lockedOnTarget != null || t.CurrentStrongestTargetEcho != null)
        {
            HadContact.Add(id);
            return true;
        }
        if (HadContact.Contains(id))
            return true;

        t.moveForward(t.TransitVelocityInKnots);
        t.setHeight(t.SearchDepth, 60f, 1.1f);

        Vector3 forward = t.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 1e-6f)
        {
            Vector3 ahead = t.transform.position + forward.normalized * AimLeadUnity;
            ahead.y = t.transform.position.y;
            t.AimPointGeoPosition = new GeoPosition(ahead);
        }
        return false;
    }
}

[HarmonyPatch(typeof(Torpedo), "initParams")]
internal static class TorpedoInitParamsPatch
{
    private static void Postfix(Torpedo __instance) => StraightRunPatch.HadContact.Remove(__instance.GetInstanceID());
}
