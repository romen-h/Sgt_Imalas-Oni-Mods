﻿using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Rockets_TinyYetBig.SpaceStations
{
    class SpaceStation : Clustercraft
    {

        [Serialize]
        private string m_name = "SpaceStation";

        [Serialize]
        public int SpaceStationInteriorId = -1;


        [Serialize]
        public int IsOrbitalSpaceStationWorldId = -1;
        [Serialize]
        public bool IsDeconstructable = true;
        [Serialize]
        public bool BuildableInterior = true;

        public Vector2I InteriorSize = new Vector2I(30, 30);
        public string InteriorTemplate = "emptySpaceStationPrefab";
        public string IconAnimName = "station_1";

        public override List<ClusterGridEntity.AnimConfig> AnimConfigs => new List<ClusterGridEntity.AnimConfig>()
        {
            new ClusterGridEntity.AnimConfig()
            {
                animFile = Assets.GetAnim((HashedString) "gravitas_space_poi_kanim"),
                initialAnim =  IconAnimName
            }
        };

        public override string Name => this.m_name;
        //public override bool IsVisible => true;
        public override EntityLayer Layer => EntityLayer.Craft;
        public override bool SpaceOutInSameHex() => true;
        public override ClusterRevealLevel IsVisibleInFOW => ClusterRevealLevel.Visible;
        public override Sprite GetUISprite() => Assets.GetSprite("rocket_landing"); //Def.GetUISprite((object)this.gameObject).first;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            Debug.Log("MY WorldID:" + SpaceStationInteriorId);
            if (SpaceStationInteriorId < 0)
            {
                var interiorWorld = SpaceStationManager.Instance.CreateSpaceStationInteriorWorld(gameObject, "interiors/"+InteriorTemplate, InteriorSize, BuildableInterior, null);
                SpaceStationInteriorId = interiorWorld.id;
                Debug.Log("new WorldID:" + SpaceStationInteriorId);
                Debug.Log("ADDED NEW SPACE STATION INTERIOR");
            }
            ClusterManager.Instance.GetWorld(SpaceStationInteriorId).AddTag(ModAssets.Tags.IsSpaceStation);


            this.SetCraftStatus(CraftStatus.InFlight);
            var destinationSelector = gameObject.GetComponent<RocketClusterDestinationSelector>();
            destinationSelector.SetDestination(this.Location);
            var planet = ClusterGrid.Instance.GetVisibleEntityOfLayerAtAdjacentCell(this.Location, EntityLayer.Asteroid);
            if (planet != null)
            {
                IsOrbitalSpaceStationWorldId = planet.GetComponent<WorldContainer>().id;
            }

            var m_clusterTraveler = gameObject.GetComponent<ClusterTraveler>();
            m_clusterTraveler.getSpeedCB = new Func<float>(this.GetSpeed);
            m_clusterTraveler.getCanTravelCB = new Func<bool, bool>(this.CanTravel);
            m_clusterTraveler.onTravelCB = (System.Action)null;
            m_clusterTraveler.validateTravelCB = null;


        }

        private bool CanTravel(bool tryingToLand) => true;
        private float GetSpeed() => 1f;
        public void DestroySpaceStation()
        {
            this.SetExploding();
            SpaceStationManager.Instance.DestroySpaceStationInteriorWorld(this.SpaceStationInteriorId);
            UnityEngine.Object.Destroy(this.gameObject);
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
        }

    }
}
