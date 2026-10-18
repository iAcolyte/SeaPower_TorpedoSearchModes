using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SeaPower;
using UnityEngine;

namespace TorpedoSearchModes;

// Vanilla hardcodes the release range of torpedoes from aircraft/helicopters to 1 nm in three places.
// For player units carrying torpedoes with selectable search modes, use the torpedo's own
// max range for the chosen launch speed instead (8 nm at 30 kn for Mk 46 Mod 5), minus a fuel reserve
// for the search and the 45 kn terminal run.
internal static class AirDropRange
{
    public const float OneNm = 27.559496f;

    // Fraction of the torpedo's range kept in reserve: 8 nm at 30 kn -> release from 7.2 nm.
    public const float Reserve = 0.10f;

    private static readonly AccessTools.FieldRef<AlignUnitToTarget, ObjectBase> AlignBaseObject =
        AccessTools.FieldRefAccess<AlignUnitToTarget, ObjectBase>("_baseObject");

    public static float For(AmmunitionParameters ap, ObjectBase launcher)
    {
        if (ap == null || launcher == null || !launcher.IsPlayerObject
            || ap._availableSearchModes == null || ap._availableSearchModes.Count < 2)
            return OneNm;
        return Mathf.Max(OneNm, ap.EffectiveMaxLaunchRange(launcher) * (1f - Reserve));
    }

    public static float ForAlign(AlignUnitToTarget align) => For(align._ap, AlignBaseObject(align));

    public static float ForLauncher(WeaponSystem ws) => For(ws._ammoForEngage?._ap, ws._baseObject);

    // Replaces the first `ldc.r4 1nm` in a method with `ldarg.0; call helper`.
    public static IEnumerable<CodeInstruction> ReplaceFirstOneNm(IEnumerable<CodeInstruction> instructions, MethodInfo helper, string where)
    {
        bool done = false;
        foreach (CodeInstruction ci in instructions)
        {
            if (!done && ci.opcode == OpCodes.Ldc_R4 && ci.operand is float f && Mathf.Approximately(f, OneNm))
            {
                done = true;
                yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(ci);
                yield return new CodeInstruction(OpCodes.Call, helper);
                continue;
            }
            yield return ci;
        }
        if (!done)
            Debug.LogWarning("[TorpedoSearchModes] 1 nm constant not found in " + where + ", air drop range left vanilla there.");
    }
}

// Range rings / attack preview.
[HarmonyPatch(typeof(ObjectBase), nameof(ObjectBase.GetKinematicRange))]
internal static class AirTorpedoKinematicRangePatch
{
    private static void Postfix(ObjectBase __instance, Ammunition ammo, ref MissileSimulator.KinematicRangeResult __result)
    {
        if (__instance != null && ammo?._ap != null && ammo._ap._type == Ammunition.Type.Torpedo && __instance.IsAirUnit)
            __result.MaxRange = AirDropRange.For(ammo._ap, __instance);
    }
}

// How close the aircraft flies before it considers itself in firing range.
[HarmonyPatch(typeof(AlignUnitToTarget), MethodType.Constructor, typeof(ObjectBase), typeof(EngageTask), typeof(WeaponSystem))]
internal static class AlignUnitToTargetRangePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
        AirDropRange.ReplaceFirstOneNm(instructions, AccessTools.Method(typeof(AirDropRange), nameof(AirDropRange.ForAlign)), "AlignUnitToTarget..ctor");
}

// Launcher refuses to release an air-dropped torpedo farther than 1 nm from the aim point.
[HarmonyPatch(typeof(WeaponSystemLauncher), nameof(WeaponSystemLauncher.OnUpdate))]
internal static class LauncherAirDropRangePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
        AirDropRange.ReplaceFirstOneNm(instructions, AccessTools.Method(typeof(AirDropRange), nameof(AirDropRange.ForLauncher)), "WeaponSystemLauncher.OnUpdate");
}
