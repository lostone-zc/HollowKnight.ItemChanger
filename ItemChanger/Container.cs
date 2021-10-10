﻿using ItemChanger.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemChanger.Containers;

namespace ItemChanger
{
    public abstract class Container
    {
        static Container()
        {
            ResetContainers();
        }

        public const string Unknown = "Unknown";
        public const string Shiny = "Shiny";
        public const string GrubJar = "GrubJar";
        public const string GeoRock = "GeoRock";
        public const string Chest = "Chest";
        public const string Tablet = "Tablet";
        public const string Shop = "Shop";
        public const string Enemy = "Enemy";
        public const string Mimic = "Mimic";

        public static Container GetContainer(string containerType)
        {
            if (string.IsNullOrEmpty(containerType)) return null;
            if (_containers.TryGetValue(containerType, out Container value)) return value;
            return null;
        }

        public static void DefineContainer(string containerType, Container container)
        {
            _containers[containerType] = container;
        }

        private static Dictionary<string, Container> _containers;
        internal static void ResetContainers()
        {
            _containers = new Dictionary<string, Container>
            {
                { Shiny, new ShinyContainer() },
                { GrubJar, new GrubJarContainer() },
                { GeoRock, new GeoRockContainer() },
                { Chest, new ChestContainer() },
                { Tablet, new TabletContainer() },
                { Mimic, new MimicContainer() }
            };
        }

        public abstract string Name { get; }
        public virtual bool SupportsCost => false;
        public virtual bool SupportsSceneChange => false;
        public virtual bool SupportsDrop => false;
        public virtual bool SupportsInstantiate => false;
        public abstract GameObject GetNewContainer(AbstractPlacement placement, IEnumerable<AbstractItem> items, FlingType flingType, Cost cost = null);
        public virtual void ApplyTargetContext(GameObject obj, GameObject target, float elevation)
        {
            if (target.transform.parent != null)
            {
                obj.transform.SetParent(target.transform.parent);
            }

            obj.transform.position = target.transform.position;
            obj.transform.localPosition = target.transform.localPosition;
            obj.transform.Translate(new(0, -elevation));
            obj.SetActive(target.activeSelf);
            obj.transform.SetPositionZ(0);
        }

        public virtual void ApplyTargetContext(GameObject obj, float x, float y, float elevation)
        {
            obj.transform.position = new Vector2(x, y - elevation);
        }

        public static void OnEnable(PlayMakerFSM fsm)
        {
            var info = fsm.gameObject.GetComponent<ContainerInfo>();
            if (info != null)
            {
                var container = GetContainer(info.containerType);
                if (container == null)
                {
                    ItemChangerMod.instance.LogError($"Unable to find Container definition for {info.containerType}!");
                    return;
                }
                // TODO: add fsmName check here

                var give = info.giveInfo;
                var scene = info.changeSceneInfo;
                var cost = info.costInfo;

                if (give != null && !give.applied)
                {
                    container.AddGiveEffectToFsm(fsm, give);
                    give.applied = true;
                }

                if (scene != null && !scene.applied)
                {
                    container.AddChangeSceneToFsm(fsm, scene);
                    scene.applied = true;
                }

                if (cost != null && !cost.applied)
                {
                    container.AddCostToFsm(fsm, cost);
                    cost.applied = true;
                }
            }
        }

        public virtual void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info) { }

        public virtual void AddChangeSceneToFsm(PlayMakerFSM fsm, ChangeSceneInfo info) { }

        public virtual void AddCostToFsm(PlayMakerFSM fsm, CostInfo info) { }
    }
}
