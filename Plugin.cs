using System.Collections.Generic;
using System.Reflection;
using AnchorChain;
using HarmonyLib;
using SeaPower;
using SeapowerUI.ViewModels;
using UniRx;
using UnityEngine;

namespace TorpedoSearchModes;

[ACPlugin("local.torpedo-search-modes", "Torpedo Search Modes", "0.1.0", null, null)]
public sealed class Plugin : IAnchorChainMod
{
    private static bool initialized;

    public void TriggerEntryPoint()
    {
        if (initialized)
            return;
        new Harmony("local.torpedo-search-modes").PatchAll(typeof(Plugin).Assembly);
        initialized = true;
    }
}

// Adds a "Search mode" submenu to the torpedo weapon menu for ammo that lists AllowedSearchModes.
// The game already applies ObjectBase._searchModeSettings at launch (Torpedo), it only lacks the UI.
[HarmonyPatch]
internal static class WeaponEntrySalvoCommandsPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(WeaponEntry), "GetSalvoCommands",
            new[] { typeof(ObjectBase), typeof(AmmunitionParameters), typeof(ReactiveProperty<string>), typeof(bool) });

    private static void Postfix(ObjectBase obj, AmmunitionParameters ap, TrulyObservableCollection<ContextMenuItem> __result)
    {
        if (__result == null || obj == null || ap == null || ap._type != Ammunition.Type.Torpedo)
            return;
        List<AmmunitionParameters.SearchMode> modes = ap._availableSearchModes;
        if (modes == null || modes.Count < 2)
            return;

        ReactiveProperty<AmmunitionParameters.SearchMode> setting = GetSetting(obj, ap);
        var items = new TrulyObservableCollection<ContextMenuItem>();
        foreach (AmmunitionParameters.SearchMode mode in modes)
        {
            AmmunitionParameters.SearchMode m = mode;
            items.Add(new ContextMenuItem("ContextMenu", "SearchMode_" + m,
                command: new DelegateCommand(_ => setting.Value = m),
                isChecked: setting.Select(v => v == m).ToReadOnlyReactiveProperty()));
        }

        var menu = new ContextMenuItem("ContextMenu", "SearchMode", items,
            tooltip: Singleton<LanguageResourceHandler>.Instance.getText("ContextMenu", "SearchModeTooltip"));

        // Put it right before the "Single shot" entry, i.e. after depth/speed/seeker settings.
        string singleShot = Singleton<LanguageResourceHandler>.Instance.getText("ContextMenu", "SingleShot");
        int index = __result.Count;
        for (int i = 0; i < __result.Count; i++)
        {
            if (__result[i].Label as string == singleShot)
            {
                index = i;
                break;
            }
        }
        __result.Insert(index, menu);
    }

    private static ReactiveProperty<AmmunitionParameters.SearchMode> GetSetting(ObjectBase obj, AmmunitionParameters ap)
    {
        if (!obj._searchModeSettings.TryGetValue(ap._ammunitionFileName, out var setting))
        {
            setting = new ReactiveProperty<AmmunitionParameters.SearchMode>(ap._defaultSearchMode);
            obj._searchModeSettings[ap._ammunitionFileName] = setting;
        }
        return setting;
    }
}
