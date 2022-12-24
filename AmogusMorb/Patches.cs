
using HarmonyLib;
using Klei.AI;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UtilLibs;
using static AmogusMorb.ModAssets;
using static KAnim;

namespace AmogusMorb
{
    internal class Patches
    {
        [HarmonyPatch(typeof(SuitFabricatorConfig))]
        [HarmonyPatch(nameof(SuitFabricatorConfig.CreateBuildingDef))]
        public static class CustomForgeAnimation_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                if(Config.Instance.OldSuitMaker == true)
                {
                    __result.AnimFiles = new KAnimFile[1] { Assets.GetAnim("old_suit_maker_kanim") };
                }
            }
        }
    }
}
