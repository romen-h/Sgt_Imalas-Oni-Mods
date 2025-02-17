﻿using Rockets_TinyYetBig.Behaviours;
using Rockets_TinyYetBig.Buildings.CargoBays;
using Rockets_TinyYetBig.Buildings.Utility;
using Rockets_TinyYetBig.NonRocketBuildings;
using Rockets_TinyYetBig.RocketFueling;
using Rockets_TinyYetBig.SpaceStations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;
using UtilLibs;
using static Rockets_TinyYetBig.RocketFueling.FuelLoaderComponent;
using static Rockets_TinyYetBig.STRINGS.BUILDING.STATUSITEMS;
using static StateMachine<LaunchPadMaterialDistributor, LaunchPadMaterialDistributor.Instance, IStateMachineTarget, LaunchPadMaterialDistributor.Def>;

namespace Rockets_TinyYetBig
{
    public class ModAssets
    {
        public const float SmallCargoBayUnits = 9;
        public const float MediumCargoBayUnits = 28;
        public const float CollossalCargoBayUnits = 64;

        /// <summary>
        /// Second value is when rebalance is on, first is when off
        /// </summary>
        public static Dictionary<string, Tuple<float, float>> CargoBaySizes = new Dictionary<string, Tuple<float, float>>()
        {
            {
                SolidCargoBaySmallConfig.ID,
                new Tuple<float, float>(1200f * ROCKETRY.CARGO_CAPACITY_SCALE,Config.Instance.SolidCargoBayUnits * SmallCargoBayUnits)
            },
            {
                SolidCargoBayClusterConfig.ID,
                new Tuple<float, float>(ROCKETRY.SOLID_CARGO_BAY_CLUSTER_CAPACITY * ROCKETRY.CARGO_CAPACITY_SCALE,Config.Instance.SolidCargoBayUnits * MediumCargoBayUnits)
            },
            {
                SolidCargoBayClusterLargeConfig.ID,
                new Tuple<float, float>(SolidCargoBayClusterLargeConfig.CAPACITY_OFF,SolidCargoBayClusterLargeConfig.CAPACITY_ON)
            },

            {
                LiquidCargoBaySmallConfig.ID,
                new Tuple<float, float>(900f * ROCKETRY.CARGO_CAPACITY_SCALE,Config.Instance.LiquidCargoBayUnits * SmallCargoBayUnits)
            },
            {
                LiquidCargoBayClusterConfig.ID,
                new Tuple<float, float>(ROCKETRY.LIQUID_CARGO_BAY_CLUSTER_CAPACITY * ROCKETRY.CARGO_CAPACITY_SCALE,Config.Instance.LiquidCargoBayUnits * MediumCargoBayUnits)
            },
            {
                LiquidCargoBayClusterLargeConfig.ID,
                new Tuple<float, float>(LiquidCargoBayClusterLargeConfig.CAPACITY_OFF,LiquidCargoBayClusterLargeConfig.CAPACITY_ON)
            },

            {
                GasCargoBaySmallConfig.ID,
                new Tuple<float, float>(360f * ROCKETRY.CARGO_CAPACITY_SCALE,Config.Instance.GasCargoBayUnits * SmallCargoBayUnits)
            },
            {
                GasCargoBayClusterConfig.ID,
                new Tuple<float, float>(ROCKETRY.GAS_CARGO_BAY_CLUSTER_CAPACITY * ROCKETRY.CARGO_CAPACITY_SCALE,Config.Instance.GasCargoBayUnits * MediumCargoBayUnits)
            },
            {
                GasCargoBayClusterLargeConfig.ID,
                new Tuple<float, float>(GasCargoBayClusterLargeConfig.CAPACITY_OFF,GasCargoBayClusterLargeConfig.CAPACITY_ON)
            },

        };

        public static bool GetCargoBayCapacity(string id, out float cargoCapacity)
        {
            cargoCapacity = 0;
            if (!CargoBaySizes.ContainsKey(id))
            {
                return false;
            }
            else
            {
                cargoCapacity = Config.Instance.RebalancedCargoCapacity ? CargoBaySizes[id].second : CargoBaySizes[id].first;
                return true;
            }
        }
        public static float TotalMassStoredOfItems(IEnumerable<GameObject> items)
        {
            float num = 0f;
            foreach (var item in items)
            {
                if (!(item == null))
                {
                    PrimaryElement component = item.GetComponent<PrimaryElement>();
                    if (component != null)
                    {
                        num += component.Units * component.MassPerUnit;
                    }
                }
            }

            return Mathf.RoundToInt(num * 1000f) / 1000f;
        }
        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] arr)
        {
            foreach (IEnumerable col in arr)
                foreach (T item in col)
                    yield return item;
        }

        public static void ReplacedCargoLoadingMethod(CraftModuleInterface craftInterface, HashSetPool<ChainedBuilding.StatesInstance, ChainedBuilding.StatesInstance>.PooledHashSet chain, System.Action<bool> SetCompleteAction)
        {
            bool HasLoadingProcess = false;
            DictionaryPool<CargoBay.CargoType, ListPool<CargoBayCluster, CraftModuleInterface>.PooledList, CraftModuleInterface>.PooledDictionary pooledDictionary
                = DictionaryPool<CargoBay.CargoType, ListPool<CargoBayCluster, CraftModuleInterface>.PooledList, CraftModuleInterface>.Allocate();
            pooledDictionary[CargoBay.CargoType.Solids] = ListPool<CargoBayCluster, CraftModuleInterface>.Allocate();
            pooledDictionary[CargoBay.CargoType.Liquids] = ListPool<CargoBayCluster, CraftModuleInterface>.Allocate();
            pooledDictionary[CargoBay.CargoType.Gasses] = ListPool<CargoBayCluster, CraftModuleInterface>.Allocate();


            var FuelTanks = ListPool<FuelTank, CraftModuleInterface>.Allocate();
            var OxidizerTanks = ListPool<OxidizerTank, CraftModuleInterface>.Allocate();
            var HEPStorages = ListPool<HighEnergyParticleStorage, CraftModuleInterface>.Allocate();
            var DrillConeStorages = ListPool<Storage, CraftModuleInterface>.Allocate();


            Tag FuelTag = SimHashes.Void.CreateTag();

            bool hasOxidizer;

            foreach (Ref<RocketModuleCluster> clusterModule in (IEnumerable<Ref<RocketModuleCluster>>)craftInterface.ClusterModules)
            {
                if (clusterModule.Get().TryGetComponent<RocketEngineCluster>(out var engine))
                {
                    FuelTag = engine.fuelTag;
                    hasOxidizer = engine.requireOxidizer;
                }

                if (clusterModule.Get().TryGetComponent<FuelTank>(out var fueltank))
                    FuelTanks.Add(fueltank);

                if (clusterModule.Get().TryGetComponent<HighEnergyParticleStorage>(out var hepStorage))
                    HEPStorages.Add(hepStorage);

                if (clusterModule.Get().TryGetComponent<OxidizerTank>(out var oxTank))
                {
                    hasOxidizer = true;
                    OxidizerTanks.Add(oxTank);
                }

                if (clusterModule.Get().TryGetComponent<CargoBayCluster>(out var cargoBay) && cargoBay.storageType != CargoBay.CargoType.Entities && cargoBay.RemainingCapacity > 0f)
                {
                    pooledDictionary[cargoBay.storageType].Add(cargoBay);
                }

                //if (clusterModule.Get().GetSMI<ResourceHarvestModule.StatesInstance>() != null)
                //{
                //    if (clusterModule.Get().TryGetComponent<Storage>(out var DrillConeStorage))
                //        DrillConeStorages.Add(DrillConeStorage);
                //}
                //if (clusterModule.Get().TryGetComponent<DrillConeAssistentModule>(out var helperModule))
                //{
                //    DrillConeStorages.Add(helperModule.DiamondStorage);
                //}

                if (clusterModule.Get().TryGetComponent<DrillConeModeHandler>(out var Handler))
                {
                    if (Handler.LoadingAllowed)
                        DrillConeStorages.Add(Handler.DiamondStorage);
                }
            }

            foreach (ChainedBuilding.StatesInstance smi1 in (HashSet<ChainedBuilding.StatesInstance>)chain)
            {
                ModularConduitPortController.Instance modularConduitPortController = smi1.GetSMI<ModularConduitPortController.Instance>();
                FuelLoaderComponent fuelLoader = smi1.GetComponent<FuelLoaderComponent>();
                IConduitConsumer NormalLoaderComponent = smi1.GetComponent<IConduitConsumer>();
                bool isLoading = false;
                if (fuelLoader != null && (modularConduitPortController == null || modularConduitPortController.SelectedMode == ModularConduitPortController.Mode.Load))
                {
                    //shouldDoNormal = false;
                    modularConduitPortController.SetRocket(true);
                    if (fuelLoader.loaderType == LoaderType.Fuel)
                    {
                        GameObject[] AllItems = Concat(fuelLoader.solidStorage.items, fuelLoader.liquidStorage.items, fuelLoader.gasStorage.items).ToArray();
                        for (int index = AllItems.Count() - 1; index >= 0; --index)
                        {
                            GameObject storageItem = AllItems[index];
                            foreach (FuelTank fueltank in FuelTanks)
                            {
                                float remainingCapacity = fueltank.Storage.RemainingCapacity();
                                float num1 = TotalMassStoredOfItems(AllItems);
                                if ((double)remainingCapacity > 0.0 && (double)num1 > 0.0 && storageItem.HasTag(FuelTag))
                                {
                                    isLoading = true;
                                    HasLoadingProcess = true;
                                    Pickupable pickupable = storageItem.GetComponent<Pickupable>().Take(remainingCapacity);
                                    if (pickupable != null)
                                    {
                                        fueltank.storage.Store(pickupable.gameObject, true);
                                        //float num2 = remainingCapacity - pickupable.PrimaryElement.Mass;
                                    }
                                }
                            }
                        }
                    }
                    else if (fuelLoader.loaderType == LoaderType.HEP)
                    {
                        foreach (HighEnergyParticleStorage hepTank in HEPStorages)
                        {
                            float remainingCapacity = hepTank.RemainingCapacity();
                            float SourceAmount = fuelLoader.HEPStorage.Particles;
                            if ((double)remainingCapacity > 0.0 && (double)SourceAmount > 0.0)
                            {
                                isLoading = true;
                                HasLoadingProcess = true;
                                float ParticlesTaken = fuelLoader.HEPStorage.ConsumeAndGet(remainingCapacity);
                                if (ParticlesTaken > 0.0f)
                                {
                                    hepTank.Store(ParticlesTaken);
                                }
                            }
                        }
                    }
                    else if (fuelLoader.loaderType == LoaderType.Oxidizer)
                    {
                        GameObject[] AllOXItems = Concat(fuelLoader.solidStorage.items, fuelLoader.liquidStorage.items).ToArray();
                        for (int index = AllOXItems.Count() - 1; index >= 0; --index)
                        {
                            GameObject storageItem = AllOXItems[index];
                            foreach (OxidizerTank oxTank in OxidizerTanks)
                            {
                                float remainingCapacity = oxTank.storage.RemainingCapacity();
                                float num1 = oxTank.supportsMultipleOxidizers ? fuelLoader.solidStorage.MassStored() : fuelLoader.liquidStorage.MassStored();
                                bool tagAllowed = oxTank.supportsMultipleOxidizers
                                    ? storageItem.GetComponent<KPrefabID>().HasAnyTags(oxTank.GetComponent<FlatTagFilterable>().selectedTags)
                                    : storageItem.HasTag(oxTank.GetComponent<ConduitConsumer>().capacityTag);
                                if ((double)remainingCapacity > 0.0 && (double)num1 > 0.0 && tagAllowed)
                                {
                                    isLoading = true;
                                    HasLoadingProcess = true;
                                    Pickupable pickupable = storageItem.GetComponent<Pickupable>().Take(remainingCapacity);
                                    if (pickupable != null)
                                    {
                                        oxTank.storage.Store(pickupable.gameObject, true);
                                        //float num2 = remainingCapacity - pickupable.PrimaryElement.Mass;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (NormalLoaderComponent != null &&
                    (modularConduitPortController == null || modularConduitPortController.SelectedMode == ModularConduitPortController.Mode.Load || modularConduitPortController.SelectedMode == ModularConduitPortController.Mode.Both))
                {
                    modularConduitPortController.SetRocket(true);
                    for (int num = NormalLoaderComponent.Storage.items.Count - 1; num >= 0; num--)
                    {
                        GameObject gameObject = NormalLoaderComponent.Storage.items[num];
                        foreach (var diamondStorage in DrillConeStorages)
                        {
                            float remainingCapacity = diamondStorage.RemainingCapacity();
                            float num2 = NormalLoaderComponent.Storage.MassStored();
                            bool filterable = diamondStorage.storageFilters != null && diamondStorage.storageFilters.Count > 0;
                            if (remainingCapacity > 0f && num2 > 0f && (filterable ? diamondStorage.storageFilters.Contains(gameObject.PrefabID()) : true))
                            {
                               // SgtLogger.debuglog(DrillConeStorages.Count() + "x items, " + diamondStorage.storageFilters.First() + " Fildersssss, " + diamondStorage.RemainingCapacity());
                                isLoading = true;
                                HasLoadingProcess = true;
                                Pickupable pickupable = gameObject.GetComponent<Pickupable>().Take(remainingCapacity);
                                if (pickupable != null)
                                {
                                    diamondStorage.Store(pickupable.gameObject);
                                    remainingCapacity -= pickupable.PrimaryElement.Mass;
                                }
                            }
                        }

                        foreach (CargoBayCluster cargoBayCluster in pooledDictionary[CargoBayConduit.ElementToCargoMap[NormalLoaderComponent.ConduitType]])
                        {
                            float remainingCapacity = cargoBayCluster.RemainingCapacity;
                            float num2 = NormalLoaderComponent.Storage.MassStored();

                            if (remainingCapacity > 0f && num2 > 0f && cargoBayCluster.GetComponent<TreeFilterable>().AcceptedTags.Contains(gameObject.PrefabID()))
                            {
                                isLoading = true;
                                HasLoadingProcess = true;
                                Pickupable pickupable = gameObject.GetComponent<Pickupable>().Take(remainingCapacity);
                                if (pickupable != null)
                                {
                                    cargoBayCluster.storage.Store(pickupable.gameObject);
                                    remainingCapacity -= pickupable.PrimaryElement.Mass;
                                }
                            }
                        }
                    }
                }
                modularConduitPortController?.SetLoading(isLoading);
            }

            chain.Recycle();
            pooledDictionary[CargoBay.CargoType.Solids].Recycle();
            pooledDictionary[CargoBay.CargoType.Liquids].Recycle();
            pooledDictionary[CargoBay.CargoType.Gasses].Recycle();
            pooledDictionary.Recycle();

            DrillConeStorages.Recycle();
            FuelTanks.Recycle();
            HEPStorages.Recycle();
            OxidizerTanks.Recycle();

            SetCompleteAction.Invoke(!HasLoadingProcess);
        }


        public static string DeepSpaceScienceID = "rtb_deepspace";
        public class Techs
        {
            public static string FuelLoaderTechID = "RTB_FuelLoadersTech";
            public static Tech FuelLoaderTech;
            public static string DockingTechID = "RTB_DockingTech";
            public static Tech DockingTech;
            public static string LargerRocketLivingSpaceTechID = "RTB_LargerRocketLivingSpaceTech";
            public static Tech LargerRocketLivingSpaceTech;
            public static string SpaceStationTechID = "RTB_SpaceStationTech";
            public static Tech SpaceStationTech;
            public static string SpaceStationTechMediumID = "RTB_MediumSpaceStationTech";
            public static Tech SpaceStationTechMedium;
            public static string SpaceStationTechLargeID = "RTB_LargeSpaceStationTech";
            public static Tech SpaceStationTechLarge;
            public static string HugeCargoBayTechID = "RTB_HugeCargoBayTech";
            public static Tech HugeCargoBayTech;
        }
        public class Tags
        {
            public static Tag IsSpaceStation = TagManager.Create("RTB_isSpaceStationInteriorWorld");
            public static Tag SpaceStationOnlyInteriorBuilding = TagManager.Create("RTB_SpaceStationInteriorOnly");
            public static Tag RocketInteriorOnlyBuilding = TagManager.Create("RTB_RocketInteriorOnly");
            public static Tag RocketPlatformTag = TagManager.Create("RTB_RocketPlatformTag");
            public static Tag RadiationShielding = TagManager.Create("RadiationShieldingMaterial");
            public static Tag NeutroniumAlloy = TagManager.Create("RTB_NeutroniumAlloyMaterial");
        }

        public enum SpaceStationType
        {
            small = 0,
            medium = 1,
            large = 2,
            jumpBeacon = 3,
            jumpGate = 4,

        }


        public static Dictionary<int, SpaceStationWithStats> SpaceStationTypes = new Dictionary<int, SpaceStationWithStats>()
        {
            {
                (int)SpaceStationType.small,

                new SpaceStationWithStats(
                "RTB_SpaceStationSmall",
                "Small Space Station",
                "a tiny space station",
                new Vector2I (30,30),
                new Dictionary<string,float> { [SimHashes.Steel.CreateTag().ToString()]= 500f },
                "space_station_small_kanim",
                20f,//150f,
                Techs.SpaceStationTechID
                )
            },
            {
                (int)SpaceStationType.medium,

                new SpaceStationWithStats(
                    "RTB_SpaceStationMedium",
                    "Medium Space Station",
                    "a medium sized space station",
                    new Vector2I (45,45),
                    new Dictionary<string,float> { [SimHashes.Steel.CreateTag().ToString()]= 750f,
                                                   [SimHashes.Niobium.CreateTag().ToString()]= 500f },
                    "space_station_medium_kanim",
                    20f,//300f
                    Techs.SpaceStationTechMediumID
                )
            },
            {
                (int)SpaceStationType.large,

            new SpaceStationWithStats(
                "RTB_SpaceStationLarge",
                "Large Space Station",
                "a large space station",
                new Vector2I (60,60),
                new Dictionary<string,float> { [SimHashes.Steel.CreateTag().ToString()]= 1000f,
                                               [SimHashes.TempConductorSolid.CreateTag().ToString()]= 500f,
                                               [SimHashes.Isoresin.CreateTag().ToString()]= 300f ,
                                               [SimHashes.Graphite.CreateTag().ToString()]= 200f },
                "space_station_large_kanim",
                20f,//600f
                Techs.SpaceStationTechLargeID
                )
            }
        };

        public static Components.Cmps<FridgeModuleHatchGrabber> FridgeModuleGrabbers = new Components.Cmps<FridgeModuleHatchGrabber>();
        public static Components.Cmps<DockingManager> Dockables = new Components.Cmps<DockingManager>();

        public static Dictionary<Tuple<BuildingDef, int>, GameObject> CategorizedButtons = new Dictionary<Tuple<BuildingDef, int>, GameObject>();

        public static readonly CellOffset PLUG_OFFSET_SMALL = new CellOffset(-1, 0);
        public static readonly CellOffset PLUG_OFFSET_MEDIUM = new CellOffset(-2, 0);

        public static int InnerLimit = 0;
        public static int Rings = 0;
        public class StatusItems
        {
            public static StatusItem RTB_ModuleGeneratorNotPowered;
            public static StatusItem RTB_ModuleGeneratorPowered;
            public static StatusItem RTB_ModuleGeneratorFuelStatus;
            public static StatusItem RTB_ModuleGeneratorLandedEnabled;
            public static StatusItem RTB_RocketBatteryStatus;
            public static StatusItem RTB_AlwaysActiveOn;
            public static StatusItem RTB_AlwaysActiveOff;
            public static StatusItem RTB_CritterModuleContent;
            public static StatusItem RTB_AccessHatchStorage;


            public static StatusItem RTB_SpaceStationConstruction_Status;

            public static StatusItem RTB_SpaceStation_DeploymentState;
            public static StatusItem RTB_SpaceStation_FreshlyDeployed;
            public static StatusItem RTB_SpaceStation_OrbitHealth;
            public static StatusItem RTB_DockingActive;



            public static void Register()
            {
                RTB_RocketBatteryStatus = new StatusItem(
                      "RTB_ROCKETBATTERYSTATUS",
                      "BUILDING",
                      string.Empty,
                      StatusItem.IconType.Info,
                      NotificationType.Neutral,
                      false,
                      OverlayModes.Power.ID
                      );

                RTB_ModuleGeneratorNotPowered = new StatusItem(
                      "RTB_MODULEGENERATORNOTPOWERED",
                      "BUILDING",
                      string.Empty,
                      StatusItem.IconType.Info,
                      NotificationType.Neutral,
                      false,
                      OverlayModes.Power.ID
                      );

                RTB_ModuleGeneratorPowered = new StatusItem(
                   "RTB_MODULEGENERATORPOWERED",
                   "BUILDING",
                   string.Empty,
                   StatusItem.IconType.Info,
                   NotificationType.Neutral,
                   false,
                   OverlayModes.Power.ID);

                RTB_AlwaysActiveOn = new StatusItem(
                    "RTB_MODULEGENERATORALWAYSACTIVEPOWERED",
                    "BUILDING",
                    string.Empty,
                    StatusItem.IconType.Info,
                    NotificationType.Neutral,
                    false,
                    OverlayModes.Power.ID);

                RTB_AlwaysActiveOff = new StatusItem(
                     "RTB_MODULEGENERATORALWAYSACTIVENOTPOWERED",
                     "BUILDING",
                     string.Empty,
                     StatusItem.IconType.Info,
                     NotificationType.Neutral,
                     false,
                     OverlayModes.Power.ID);

                RTB_ModuleGeneratorFuelStatus = new StatusItem(
                     "RTB_MODULEGENERATORFUELSTATUS",
                     "BUILDING",
                     string.Empty,
                     StatusItem.IconType.Info,
                     NotificationType.Neutral,
                     false,
                     OverlayModes.Power.ID);

                RTB_ModuleGeneratorLandedEnabled = new StatusItem(
                     "RTB_ROCKETGENERATORLANDEDACTIVE",
                     "BUILDING",
                     string.Empty,
                     StatusItem.IconType.Info,
                     NotificationType.Neutral,
                     false,
                     OverlayModes.Power.ID);
                


                RTB_CritterModuleContent = new StatusItem(
                     "RTB_CRITTERMODULECONTENT",
                     "BUILDING",
                     string.Empty,
                     StatusItem.IconType.Info,
                     NotificationType.Neutral,
                false,
                     OverlayModes.None.ID);

                RTB_AccessHatchStorage = new StatusItem(
                     "RTB_FOODSTORAGESTATUS",
                     "BUILDING",
                     string.Empty,
                     StatusItem.IconType.Info,
                     NotificationType.Neutral,
                false,
                     OverlayModes.None.ID);

                RTB_SpaceStationConstruction_Status = new StatusItem(
                     "RTB_STATIONCONSTRUCTORSTATUS",
                     "BUILDING",
                     string.Empty,
                     StatusItem.IconType.Info,
                     NotificationType.Neutral,
                false,
                     OverlayModes.None.ID);

                RTB_DockingActive = new StatusItem(
                     "RTB_DOCKEDSTATUS",
                     "BUILDING",
                     string.Empty,
                     StatusItem.IconType.Info,
                     NotificationType.Neutral,
                false,
                     OverlayModes.None.ID);



                RTB_SpaceStationConstruction_Status.resolveStringCallback = ((str, data) =>
                {
                    var StationConstructior = (SpaceStationBuilder)data;
                    float remainingTime = StationConstructior.RemainingTime();
                    if (remainingTime > 0)
                    {
                        str = str.Replace("{TOOLTIP}", RTB_STATIONCONSTRUCTORSTATUS.TIMEREMAINING);
                        str = str.Replace("{TIME}", GameUtil.GetFormattedTime(remainingTime));
                    }
                    else
                    {
                        str = str.Replace("{TOOLTIP}", RTB_STATIONCONSTRUCTORSTATUS.NONEQUEUED);
                    }

                    if (StationConstructior.Constructing())
                    {
                        str = str.Replace("{STATUS}", RTB_STATIONCONSTRUCTORSTATUS.CONSTRUCTING);
                        str = str.Replace("{TIME}", GameUtil.GetFormattedTime(remainingTime));

                    }
                    else if (StationConstructior.Demolishing())
                    {
                        str = str.Replace("{STATUS}", RTB_STATIONCONSTRUCTORSTATUS.DECONSTRUCTING);
                        str = str.Replace("{TIME}", GameUtil.GetFormattedTime(remainingTime));
                    }
                    else
                    {
                        str = str.Replace("{STATUS}", RTB_STATIONCONSTRUCTORSTATUS.IDLE);
                    }
                    return str;
                });


                RTB_CritterModuleContent.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    var CritterStorage = (CritterStasisChamberModule)data;
                    //var stats = generator.GetConsumptionStatusStats();
                    //str = str.Replace("{GeneratorType}", generator.GetProperName());

                    string newValue1 = Util.FormatWholeNumber(CritterStorage.CurrentCapacity);
                    string newValue2 = Util.FormatWholeNumber(CritterStorage.CurrentMaxCapacity);
                    string CritterData = CritterStorage.GetStatusItem();

                    str = str.Replace("{0}", newValue1);
                    str = str.Replace("{1}", newValue2);
                    str = str.Replace("{CritterContentStatus}", CritterData);
                    return str;
                });

                RTB_RocketBatteryStatus.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    var stats = (Tuple<float, float>)data;
                    //var stats = generator.GetConsumptionStatusStats();
                    //str = str.Replace("{GeneratorType}", generator.GetProperName());
                    str = str.Replace("{CurrentCharge}", GameUtil.GetFormattedJoules(stats.first));
                    str = str.Replace("{MaxCharge}", GameUtil.GetFormattedJoules(stats.second));
                    return str;
                });

                RTB_DockingActive.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    var worldIDs = (List<int>)data;

                    string worldsList=string.Empty;

                    foreach(var id in worldIDs)
                    {
                        var world = ClusterManager.Instance.GetWorld(id);
                        if (world != null)
                        {
                            worldsList += string.Format(RTB_DOCKEDSTATUS.DOCKEDINFO, world.GetProperName());
                        }
                        if(worldIDs.Count  == 1)
                        {
                            str = str.Replace("{SINGLEDOCKED}", world.GetProperName());
                        }
                        else
                        {
                            str = str.Replace("{SINGLEDOCKED}", RTB_DOCKEDSTATUS.MULTIPLES);
                        }
                    }
                    str = str.Replace("{DOCKINGLIST}", worldsList);
                    return str;
                });

                RTB_ModuleGeneratorFuelStatus.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    var stats = (Tuple<float, float>)data;
                    //var stats = generator.GetConsumptionStatusStats();
                    //str = str.Replace("{GeneratorType}", generator.GetProperName());
                    str = str.Replace("{CurrentFuelStorage}", GameUtil.GetFormattedMass(stats.first));
                    str = str.Replace("{MaxFuelStorage}", GameUtil.GetFormattedMass(stats.second));
                    return str;
                });

                RTB_ModuleGeneratorNotPowered.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    Generator generator = (RTB_ModuleGenerator)data;
                    str = str.Replace("{ActiveWattage}", GameUtil.GetFormattedWattage(0.0f));
                    str = str.Replace("{MaxWattage}", GameUtil.GetFormattedWattage(generator.WattageRating));
                    return str;
                });
                RTB_ModuleGeneratorPowered.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    Generator generator = (RTB_ModuleGenerator)data;
                    str = str.Replace("{ActiveWattage}", GameUtil.GetFormattedWattage(generator.WattageRating));
                    str = str.Replace("{MaxWattage}", GameUtil.GetFormattedWattage(generator.WattageRating));
                    return str;
                });
                RTB_AlwaysActiveOff.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    Generator generator = (RTB_ModuleGenerator)data;
                    str = str.Replace("{ActiveWattage}", GameUtil.GetFormattedWattage(0.0f));
                    str = str.Replace("{MaxWattage}", GameUtil.GetFormattedWattage(generator.WattageRating));
                    return str;
                });
                RTB_AlwaysActiveOn.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    Generator generator = (RTB_ModuleGenerator)data;
                    str = str.Replace("{ActiveWattage}", GameUtil.GetFormattedWattage(generator.WattageRating));
                    str = str.Replace("{MaxWattage}", GameUtil.GetFormattedWattage(generator.WattageRating));
                    return str;
                });
                RTB_AccessHatchStorage.resolveStringCallback = (Func<string, object, string>)((str, data) =>
                {
                    FridgeModuleHatchGrabber hatch = (FridgeModuleHatchGrabber)data;
                    str = str.Replace("{FOODLIST}", hatch.GetAllMassDesc());
                    str = str.Replace("{REMAININGMASS}", hatch.TotalKCAL.ToString());
                    return str;
                });
                
                SgtLogger.debuglog("[Rocketry Expanden] Status items initialized");

            }
        }

        public struct SpaceStationWithStats
        {
            public string ID;
            public string Name;
            public string Description;
            public Vector2I InteriorSize;
            public Dictionary<string, float> materials;
            public float constructionTime;
            public float demolishingTime;
            public string Kanim;
            public string requiredTechID;
            public bool HasInterior;
            public SpaceStationWithStats(string _id, string _name, string _description, Vector2I _size, Dictionary<string, float> _mats, string _kanim, float _constructionTime, string _reqTech = "", bool _hasInterior = true)
            {
                ID = _id;
                Name = _name;
                Description = _description;
                InteriorSize = _size;
                materials = _mats;
                Kanim = _kanim;
                requiredTechID = _reqTech == "" ? Techs.SpaceStationTechID : _reqTech;
                constructionTime = _constructionTime;
                demolishingTime = _constructionTime / 4;
                HasInterior = _hasInterior;
            }

        }
    }
}
