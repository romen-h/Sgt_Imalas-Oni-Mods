﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ElementConsumer;

namespace Rockets_TinyYetBig.Patches
{
    public class InfinitePOIPatches
    {
        [HarmonyPatch(typeof(HarvestablePOIStates.Instance))]
        [HarmonyPatch(nameof(HarvestablePOIStates.Instance.DeltaPOICapacity))]
        public static class InstaRecharge
        {
            public static void Postfix(HarvestablePOIStates.Instance __instance, ref float delta)
            {
                if (Config.Instance.InfinitePOI)
                {
                    __instance.poiCapacity = __instance.configuration.GetMaxCapacity();
                }
                delta = Mathf.Max(0f, delta);
            }
        }
        

        [HarmonyPatch(typeof(SpacePOISimpleInfoPanel), "RefreshMassHeader")]
        public static class InstaRechargeStatusItem
        {
            public static void Postfix(HarvestablePOIStates.Instance harvestable, GameObject ___massHeader)
            {
                if (Config.Instance.InfinitePOI)
                {
                    HierarchyReferences component = ___massHeader.GetComponent<HierarchyReferences>();
                    component.GetReference<LocText>("ValueLabel").text = "<b>∞</b>";
                }
            }

        }
    }
}
