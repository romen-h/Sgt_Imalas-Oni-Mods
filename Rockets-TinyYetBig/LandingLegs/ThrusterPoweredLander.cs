﻿using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UtilLibs;
using static Components;

namespace Rockets_TinyYetBig.LandingLegs
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ThrusterPoweredLander : GameStateMachine<ThrusterPoweredLander, ThrusterPoweredLander.StatesInstance, IStateMachineTarget, ThrusterPoweredLander.Def>
    {
        public class Def : BaseDef
        {
            public Tag previewTag;

            public bool deployOnLanding = true;
            public List<Type> cmpsToEnable;
        }

        public class CrashedStates : State
        {
            public State loaded;

            public State emptying;

            public State empty;
        }

        public class StatesInstance : GameInstance
        {
            [Serialize]
            public float flightAnimOffset = 50f;

            public float exhaustEmitRate = 50f;

            public float exhaustTemperature = 1263.15f;

            public SimHashes exhaustElement = SimHashes.CarbonDioxide;

            public float maxAccelerationDistance = 25f;
            public float takeoffAccelPowerInv = 1f/4f;
            public float takeoffAccelPower = 4f;
            public float heightLaunchSpeedRatio = 1;

            public float topSpeed = 16f;

            public GameObject landingPreview;

            public StatesInstance(IStateMachineTarget master, Def def)
                : base(master, def)
            {
            }

            public void ResetAnimPosition()
            {
                GetComponent<KBatchedAnimController>().Offset = Vector3.up * flightAnimOffset;
            }

            public void OnJettisoned()
            {
                int cell = Grid.PosToCell(this.gameObject);
                flightAnimOffset = (float)(ClusterManager.Instance.GetWorld((int)Grid.WorldIdx[cell]).maximumBounds.y - (double)this.gameObject.transform.GetPosition().y + 100.0f);
            }

            public void ShowLandingPreview(bool show)
            {
                if (show)
                {
                    landingPreview = Util.KInstantiate(Assets.GetPrefab(base.def.previewTag), base.transform.GetPosition(), Quaternion.identity, base.gameObject);
                    landingPreview.SetActive(value: true);
                }
                else
                {
                    landingPreview.DeleteObject();
                    landingPreview = null;
                }
            }


            float SpeedFunc(float HeightValue) => Mathf.Min(HeightValue * 0.25f, topSpeed)+0.4f;
            //float SpeedFunc(float HeightValue) => Mathf.Min(1.6f*Mathf.Pow(1.84f, (HeightValue/14f ))-0.4f, topSpeed);
            //float SpeedFunc(float HeightValue) => Mathf.Min(4*Mathf.Pow(1.1f, HeightValue - 15f), topSpeed) - 0.0f;
            //float SpeedFunc(float HeightValue) => Mathf.Min(0.3f*Mathf.Pow( HeightValue ,2f)+0.25f, topSpeed);
            public void LandingUpdate(float dt)
            {
                var DeltaVdifference = SpeedFunc(flightAnimOffset);

                SgtLogger.l(flightAnimOffset.ToString(), DeltaVdifference.ToString());
                flightAnimOffset -= DeltaVdifference * dt;
                ResetAnimPosition();
                int num = Grid.PosToCell(base.gameObject.transform.GetPosition() + new Vector3(0f, flightAnimOffset, 0f));
                if (Grid.IsValidCell(num))
                {
                    SimMessages.EmitMass(num, ElementLoader.GetElementIndex(exhaustElement), dt * exhaustEmitRate, exhaustTemperature, 0, 0);
                }
            }

            public void DoLand()
            {
                base.smi.master.GetComponent<KBatchedAnimController>().Offset = Vector3.zero;
                OccupyArea occupy = base.smi.GetComponent<OccupyArea>();
                if (occupy != null)
                {
                    occupy.ApplyToCells = true;
                }

                if (base.def.deployOnLanding && CheckIfLoaded())
                {
                    base.sm.emptyCargo.Trigger(this);
                }
                SgtLogger.l("Landed");
                if (def.cmpsToEnable!=null && def.cmpsToEnable.Count > 0)
                {
                    foreach(var cmp in def.cmpsToEnable)
                    {
                        SgtLogger.l("Trying to enable " + cmp.ToString());
                        var component = gameObject.GetComponent(cmp);
                        if (component!=null && component is KMonoBehaviour)
                        {
                            SgtLogger.l(component.ToString(), "Enabled");
                            (component as KMonoBehaviour).enabled = true;
                        }
                    }
                }

                base.smi.master.gameObject.Trigger(1591811118, this);
            }

            public bool CheckIfLoaded()
            {
                bool flag = false;
                MinionStorage component = GetComponent<MinionStorage>();
                if (component != null)
                {
                    flag |= component.GetStoredMinionInfo().Count > 0;
                }

                Storage component2 = GetComponent<Storage>();
                if (component2 != null && !component2.IsEmpty())
                {
                    flag = true;
                }

                if (flag != base.sm.hasCargo.Get(this))
                {
                    base.sm.hasCargo.Set(flag, this);
                }

                return flag;
            }
        }

        public BoolParameter hasCargo;

        public Signal emptyCargo;

        public State init;

        public State stored;

        public State landing;

        public State land;

        public CrashedStates grounded;

        public BoolParameter isLanded = new BoolParameter(default_value: false);

        public override void InitializeStates(out BaseState default_state)
        {
            default_state = init;
            base.serializable = SerializeType.ParamsOnly;
            root.InitializeOperationalFlag(RocketModule.landedFlag).Enter(delegate (StatesInstance smi)
            {
                smi.CheckIfLoaded();
            }).EventHandler(GameHashes.OnStorageChange, delegate (StatesInstance smi)
            {
                smi.CheckIfLoaded();
            });
            init.ParamTransition(isLanded, grounded, GameStateMachine<ThrusterPoweredLander, StatesInstance, IStateMachineTarget, Def>.IsTrue).GoTo(stored);
            stored.TagTransition(GameTags.Stored, landing, on_remove: true).EventHandler(GameHashes.JettisonedLander, delegate (StatesInstance smi)
            {
                smi.OnJettisoned();
            });
            landing.PlayAnim("landing", KAnim.PlayMode.Loop).Enter(delegate (StatesInstance smi)
            {
                smi.ShowLandingPreview(show: true);
            }).Exit(delegate (StatesInstance smi)
            {
                smi.ShowLandingPreview(show: false);
            })
                .Enter(delegate (StatesInstance smi)
                {
                    smi.ResetAnimPosition();
                })
                .Update(delegate (StatesInstance smi, float dt)
                {
                    smi.LandingUpdate(dt);
                }, UpdateRate.SIM_33ms)
                .UpdateTransition(land, ( smi,dt) => smi.flightAnimOffset <= 0.1f);
            land.PlayAnim("grounded_pre").OnAnimQueueComplete(grounded);
            grounded.DefaultState(grounded.loaded)
                .ToggleOperationalFlag(RocketModule.landedFlag)
                .Enter(delegate (StatesInstance smi)
                {
                    smi.CheckIfLoaded();
                })
                .Enter(delegate (StatesInstance smi)
                {
                    smi.sm.isLanded.Set(value: true, smi);
                });
            grounded.loaded.PlayAnim("grounded")
                .ParamTransition(hasCargo, grounded.empty, GameStateMachine<ThrusterPoweredLander, StatesInstance, IStateMachineTarget, Def>.IsFalse)
                .OnSignal(emptyCargo, grounded.emptying)
                .Enter(delegate (StatesInstance smi)
                {
                    smi.DoLand();
                });
            grounded.emptying.PlayAnim("deploying").TriggerOnEnter(GameHashes.JettisonCargo).OnAnimQueueComplete(grounded.empty);
            grounded.empty.PlayAnim("deployed").ParamTransition(hasCargo, grounded.loaded, GameStateMachine<ThrusterPoweredLander, StatesInstance, IStateMachineTarget, Def>.IsTrue);
        }
    }
}
