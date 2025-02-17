﻿using Database;
using HarmonyLib;
using Klei.AI;
using KSerialization;
using ProcGenGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UtilLibs;
using static SaveGameModLoader.ModAssets;
using Ionic.Zlib;
using Klei;
using static UnityEngine.UI.Image;
using YamlDotNet;
using static TextureAtlas.AtlasData;

namespace SaveGameModLoader
{
    class AllPatches
    {
        [HarmonyPatch(typeof(LoadScreen), "ShowColony")]
        public static class AddModSyncButtonLogic
        {
            public static void InsertModButtonCode(
                  RectTransform entry
                , object FileDetails
                )
            {

                var ContainerOpener = FileDetails.GetType().GetField("save").GetValue(FileDetails);
                string baseName = (string)ContainerOpener.GetType().GetField("BaseName").GetValue(ContainerOpener);
                string fileName = (string)ContainerOpener.GetType().GetField("FileName").GetValue(ContainerOpener);

                var btn = entry.Find("SyncButton").GetComponent<KButton>();

                if (btn != null)
                {
                    var colonyList = ModlistManager.Instance.TryGetColonyModlist(baseName);
                    var saveGameEntry = colonyList != null ? colonyList.TryGetModListEntry(fileName) : null;

                    btn.isInteractable = colonyList != null && saveGameEntry != null && App.GetCurrentSceneName() == "frontend";
                    btn.onClick += (() =>
                    {
                        ModlistManager.Instance.InstantiateModViewForPathOnly(fileName);
                    });
                }
            }
            public static readonly MethodInfo ButtonLogic = AccessTools.Method(
               typeof(AddModSyncButtonLogic),
               nameof(InsertModButtonCode));

            private static readonly MethodInfo SuitableMethodInfo = AccessTools.Method(
                    typeof(KButton),
                    nameof(KButton.ClearOnClick));

            private static readonly MethodInfo SaveGameFileIndexFinder = AccessTools.Method(
                    typeof(System.IO.Path),
                    nameof(System.IO.Path.GetFileNameWithoutExtension));

            //private static MethodInfo TransformIndexFinder =
            //    AccessTools.Method(
            //    typeof(UnityEngine.Object),
            //    nameof(UnityEngine.Object.Instantiate),
            //    new System.Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Transform)}); 


            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                ///
                var Methods = typeof(UnityEngine.Object).GetPublicStaticMethods();

                var GenericMethodInfo = Methods.FirstOrDefault(meth =>
                meth.Name.Contains("Instantiate")
                && meth.GetParameters().Length == 2
                && meth.GetParameters().Last().ParameterType == typeof(UnityEngine.Transform)
                && meth.GetParameters().First().ParameterType.IsGenericParameter
                ).MakeGenericMethod(typeof(RectTransform));
                
                
                //SgtLogger.l(GenericMethodInfo.Name + "::" + GenericMethodInfo, "postselect");
                ///


                var code = instructions.ToList();
                var insertionIndex = code.FindLastIndex(ci => ci.opcode == OpCodes.Callvirt && ci.operand is MethodInfo f && f == SuitableMethodInfo);


                var SaveGameFileIndexFinderStart = code.FindIndex(ci => ci.opcode == OpCodes.Call && ci.operand is MethodInfo f && f == SaveGameFileIndexFinder);
                var saveFileRootIndex = TranspilerHelper.FindIndexOfNextLocalIndex(code, SaveGameFileIndexFinderStart);

                var TransformIndexFinderStart = code.FindIndex(ci => ci.opcode == OpCodes.Call && ci.operand is MethodInfo f && f == GenericMethodInfo);// code.FindIndex(ci => ci.opcode == OpCodes.Call && ci.operand.ToString().Contains("Instantiate"));


                var TransformIndex = TranspilerHelper.FindIndexOfNextLocalIndex(code, TransformIndexFinderStart, false);
                //SgtLogger.l(TransformIndex + "", "TRANSFORMINDEX");

                //foreach (var v in code) { SgtLogger.log(v.opcode + " -> " + v.operand); };
                if (insertionIndex != -1)
                {
                    insertionIndex += 1;
                    code.Insert(insertionIndex, new CodeInstruction(OpCodes.Ldloc_S, TransformIndex));//7
                    code.Insert(++insertionIndex, new CodeInstruction(OpCodes.Ldloc_S, saveFileRootIndex));//6
                    code.Insert(++insertionIndex, new CodeInstruction(OpCodes.Call, ButtonLogic)); 

                    //TranspilerHelper.PrintInstructions(code,true);
                }
                //foreach (var v in code) { SgtLogger.log(v.opcode + " -> " + v.operand); };

                return code;
            }

            public struct SaveGameFileDetails
            {
                public string BaseName;
                public string FileName;
                public string UniqueID;
                public System.DateTime FileDate;
                public SaveGame.Header FileHeader;
                public SaveGame.GameInfo FileInfo;
                public long Size;
            }
        }

        [HarmonyPatch(typeof(ModsScreen), "Exit")]
        public static class SyncModeOff
        {
            public static void Postfix()
            {
                ModlistManager.Instance.IsSyncing = false;
            }
        }
        [HarmonyPatch(typeof(ModsScreen), "ShouldDisplayMod")]
        public static class ModsScreen_ShouldDisplayMod_Patch
        {
            public static void Postfix(KMod.Mod mod, ref bool __result)
            {
                if (__result && ModlistManager.Instance.IsSyncing)
                {
                    __result = ModlistManager.Instance.ModIsNotInSync(mod);
                }
            }
        }
        [HarmonyPatch(typeof(ModsScreen), "OnActivate")]
        public static class ModsScreen_AddModListButton
        {
            public  static void Postfix(ModsScreen __instance)
            {
                ///Add Modlist Button
                var workShopButton = __instance.transform.Find("Panel/DetailsView/WorkshopButton");
                var DetailsView = __instance.transform.Find("Panel/DetailsView").gameObject;
                if (__instance.gameObject.name == "SYNCSCREEN")
                    return;
                var modlistButtonGO = Util.KInstantiateUI<RectTransform>(workShopButton.gameObject, DetailsView, true);
                modlistButtonGO.name = "ModListsButton";
                var modlistButtonText = modlistButtonGO.Find("Text").GetComponent<LocText>();
                modlistButtonText.text = STRINGS.UI.FRONTEND.MODLISTVIEW.MODLISTSBUTTON;
                modlistButtonGO.FindOrAddUnityComponent<ToolTip>().SetSimpleTooltip(STRINGS.UI.FRONTEND.MODLISTVIEW.MODLISTSBUTTONINFO);
                var modlistButton = modlistButtonGO.GetComponent<KButton>();
                modlistButton.ClearOnClick();

#if DEBUG
               // UIUtils.ListAllChildren(__instance.transform);
#endif

                modlistButton.onClick += () =>
                {
                ///Util.KInstantiateUI(ScreenPrefabs.Instance.RailModUploadMenu.gameObject, modScreen.gameObject, true); ///HMMM; great if modified for modpack creation
                    var window = Util.KInstantiateUI(ScreenPrefabs.Instance.languageOptionsScreen.gameObject);
                    window.SetActive(false);
                    var copy = window.transform;
                    UnityEngine.Object.Destroy(window);
                    var newScreen = Util.KInstantiateUI(copy.gameObject, __instance.gameObject, true);
                    newScreen.name = "ModListView";
                    newScreen.AddComponent(typeof(ModListScreen));

                }; 
                var closeButton = __instance.transform.Find("Panel/DetailsView/CloseButton");
                //UIUtils.ListAllChildrenWithComponents(closeButton.transform);
                closeButton.SetAsLastSibling();
            }
        }
        [HarmonyPatch(typeof(LoadScreen), "OnActivate")]
        public static class AddModSyncButtonToLoadscreen
        {
            public static bool Prefix(LoadScreen __instance)
            {
                if( __instance.name == "NODONTDOTHAT") return false;

                ModlistManager.Instance.ParentObjectRef = __instance.transform.parent.gameObject;
                ModlistManager.Instance.GetAllStoredModlists();


                var ViewRootFinder = typeof(LoadScreen).GetField("colonyViewRoot", BindingFlags.NonPublic | BindingFlags.Instance);
                
                GameObject viewRoot = (GameObject)ViewRootFinder.GetValue(__instance);

                HierarchyReferences references = viewRoot.GetComponent<HierarchyReferences>();
                ///get ListEntryTemplate 
                RectTransform template = references.GetReference<RectTransform>("SaveTemplate");

                ///Get LoadButton and move it to the left 
                HierarchyReferences TemplateRefs = template.GetComponent<HierarchyReferences>();
                var SyncTemplate = TemplateRefs.GetReference<RectTransform>("LoadButton");
                SyncTemplate.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 65f, SyncTemplate.rect.width);
                ///Move Time&Date text to the left
                var date = TemplateRefs.GetReference<RectTransform>("DateText");
                date.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, SyncTemplate.rect.width + 85f, date.rect.width);


                ///Instantiate SyncButton
                RectTransform kbutton = Util.KInstantiateUI<RectTransform>(SyncTemplate.gameObject, template.gameObject, true);
                kbutton.rectTransform().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 10, 50);

                ///Add SyncButton to template and set Params
                kbutton.name = "SyncButton";
                var syncText = kbutton.GetComponentInChildren<LocText>(true);
                var btn = kbutton.GetComponentInChildren<KButton>(true);
                syncText.key = "STRINGS.UI.FRONTEND.MODSYNCING.SYNCMODSBUTTONBG";
                btn.bgImage.sprite = Assets.GetSprite("icon_thermal_conductivity");
                return true;
            }
        }

        /// <summary>
        /// Add a "Sync and Continue"-Button to the main menu prefab
        /// </summary>
        [HarmonyPatch(typeof(MainMenu), "OnPrefabInit")]
        public static class AddSyncContinueButton
        {
            public static void Prefix(MainMenu __instance)
            {
                string path;
                if (KPlayerPrefs.HasKey("AutoResumeSaveFile"))
                {
                    path = KPlayerPrefs.GetString("AutoResumeSaveFile");
                }
                else
                    path = string.IsNullOrEmpty(GenericGameSettings.instance.performanceCapture.saveGame) ? SaveLoader.GetLatestSaveForCurrentDLC() : GenericGameSettings.instance.performanceCapture.saveGame;
#if DEBUG
                //UIUtils.ListAllChildren(__instance.transform);
#endif
                if(path==null||path == string.Empty)
                {
                    return;
                }

                Transform parentBar;
                Transform contButton;

                if (DlcManager.IsExpansion1Active()) 
                { 
                    parentBar = __instance.transform.Find("MainMenuMenubar/MainMenuButtons");
                    contButton = __instance.transform.Find("MainMenuMenubar/MainMenuButtons/Button_ResumeGame");
                }
                else
                {
                    parentBar = __instance.transform.Find("UI Group/TopRight/MainMenuButtons");
                    contButton = __instance.transform.Find("UI Group/TopRight/MainMenuButtons/Button_ResumeGame");
                }

                    var bt = Util.KInstantiateUI(contButton.gameObject, parentBar.gameObject, true);

                    string colonyName = SaveGameModList.GetModListFileName(path);

                    var colony =  ModlistManager.Instance.TryGetColonyModlist(colonyName);
                    bool interactable = colony != null ? colony.TryGetModListEntry(path)!=null : false;

                    var button = bt.GetComponent<KButton>();
                    bt.name = "SyncAndContinue";
                    var internalText = bt.transform.Find("ResumeText").GetComponent<LocText>();

                    internalText.text = STRINGS.UI.FRONTEND.MODSYNCING.CONTINUEANDSYNC;

                    button.isInteractable = interactable;
                    button.ClearOnClick();
                    button.onClick +=
                    () =>
                    {
                        ModlistManager.Instance.InstantiateModViewForPathOnly(path);
                    };

                    ModlistManager.Instance.ParentObjectRef = __instance.gameObject;
                    var SaveGameName = button.transform.Find("SaveNameText").gameObject;
                    UnityEngine.Object.Destroy(SaveGameName);
            }
        }


        /// <summary>
        /// On loading a savegame, store the mod config in the modlist.
        /// </summary>
        [HarmonyPatch(typeof(SaveLoader), "Load")]
        [HarmonyPatch(new System.Type[] { typeof(IReader) })]
        public static class LoadModConfigPatch
        {
            internal class SaveFileRoot
            {
                public int WidthInCells;
                public int HeightInCells;
                public Dictionary<string, byte[]> streamed;
                public string clusterID;
                public List<ModInfo> requiredMods;
                public List<KMod.Label> active_mods;
                public SaveFileRoot() => this.streamed = new Dictionary<string, byte[]>();
            }
            public static SaveLoader instance;
            public static void Prefix(SaveLoader __instance)
            {
                instance = __instance;
            }
            public static void WriteModlistPatch(SaveFileRoot saveFileRoot)
            {
                var savedButNotEnabledMods = saveFileRoot.active_mods;
                savedButNotEnabledMods.Remove(savedButNotEnabledMods.Find(mod => mod.title == "SaveGameModLoader"));
                
                
                bool init = ModlistManager.Instance.CreateOrAddToModLists(
                    SaveLoader.GetActiveSaveFilePath(), 
                    saveFileRoot.active_mods
                    );
                
                KPlayerPrefs.DeleteKey("AutoResumeSaveFile");
            }

            public static readonly MethodInfo ScreenCreator = AccessTools.Method(
               typeof(LoadModConfigPatch),
               nameof(WriteModlistPatch));


            public static readonly MethodInfo SaveFileRootCallLocation = AccessTools.Method(
               typeof(Deserializer),
               nameof(Deserializer.Deserialize),
                new System.Type[] { typeof(object) });

            private static readonly MethodInfo SuitableMethodInfo = AccessTools.Method(
                    typeof(KMod.Manager),
                    "MatchFootprint");

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var code = instructions.ToList();
                var insertionIndex = code.FindIndex(ci => ci.opcode == OpCodes.Callvirt && ci.operand is MethodInfo f && f == SuitableMethodInfo);
                var deserializerSearchIndex = code.FindIndex(ci => ci.opcode == OpCodes.Callvirt && ci.operand is MethodInfo f && f == SaveFileRootCallLocation);

                var saveFileRootIndex = TranspilerHelper.FindIndexOfNextLocalIndex(code, deserializerSearchIndex);

                //foreach (var v in code) { SgtLogger.log(v.opcode + " -> " + v.operand); };
                if (insertionIndex != -1&& saveFileRootIndex!=-1)
                {
                     code.Insert(insertionIndex, new CodeInstruction(OpCodes.Ldloc_S, saveFileRootIndex));
                     code.Insert(++insertionIndex, new CodeInstruction(OpCodes.Call, ScreenCreator));
                }

                return code;
            }
        }

        /// <summary>
        /// On Saving, store the mod config to the modlist
        /// </summary>
        [HarmonyPatch(typeof(SaveLoader))]
        [HarmonyPatch(nameof(SaveLoader.Save))]
        [HarmonyPatch(new System.Type[] { typeof(string), typeof(bool), typeof(bool) })]
        public static class SaveModConfigOnSave
        {
            public static void Postfix(string filename, bool isAutoSave = false, bool updateSavePointer = true)
            {
                //SgtLogger.log(filename + isAutoSave + updateSavePointer);
                KMod.Manager modManager = Global.Instance.modManager;

                var enabledModLabels = modManager.mods.FindAll(mod => mod.IsActive() == true).Select(mod => mod.label).ToList();

                bool init = ModlistManager.Instance.CreateOrAddToModLists(
                    filename,
                    enabledModLabels
                    );
            }
        }

        ///// <summary>
        ///// Since the Mod patches the load method, it will recieve blame on ANY load crash.
        ///// This Patch keeps it enabled on a crash, so you dont need to reenable it for syncing,
        ///// </summary>
        //[HarmonyPatch(typeof(KMod.Mod))]
        //[HarmonyPatch(nameof(KMod.Mod.SetCrashed))]
        //public static class DontDisableModOnCrash
        //{
        //    public static bool Prefix(KMod.Mod __instance)
        //    {
        //        if (__instance.label.title == ModAssets.ThisModName)
        //            return false;
        //        return true;
        //    }
        //}
    }
}
