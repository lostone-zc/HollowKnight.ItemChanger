﻿using System.Text;
using ItemChanger.FsmStateActions;
using ItemChanger.Extensions;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Locations;
using ItemChanger.Util;

namespace ItemChanger.Placements
{
    /// <summary>
    /// Placement which allows shop-like behavior, with a tablet showing item names and costs near a chest. 
    /// <br />When the chest is opened, any costs that can be paid are paid and the corresponding items are spawned.
    /// </summary>
    public class CostChestPlacement : AbstractPlacement, IContainerPlacement, IMultiCostPlacement, IPrimaryLocationPlacement
    {
        public CostChestPlacement(string Name) : base(Name) { }

        public ContainerLocation chestLocation;
        public PlaceableLocation tabletLocation;
        AbstractLocation IPrimaryLocationPlacement.Location => chestLocation;

        protected override void OnLoad()
        {
            chestLocation.Placement = tabletLocation.Placement = this;
            chestLocation.Load();
            tabletLocation.Load();
        }

        protected override void OnUnload()
        {
            chestLocation.Unload();
            tabletLocation.Unload();
        }

        [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override void OnPreview(string previewText)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            LogError("OnPreview is not supported on CostChestPlacement.");
        }

        public void AddItem(AbstractItem item, Cost cost)
        {
            CostTag tag = item.GetTag<CostTag>() ?? item.AddTag<CostTag>();
            tag.Cost = cost;
            Items.Add(item);
        }

        public void GetContainer(AbstractLocation location, out GameObject obj, out string containerType)
        {
            if (location == chestLocation)
            {
                obj = ChestUtility.MakeNewChest(this);
                containerType = Container.Chest;
                EditChestFsm(obj);
            }
            else if (location == tabletLocation)
            {
                obj = TabletUtility.MakeNewTablet(this, BuildText);
                containerType = Container.Tablet;
            }
            else throw new ArgumentException($"Unknown location {location.name} found in GetContainer.");
        }

        public void EditChestFsm(GameObject chest)
        {
            PlayMakerFSM chestFsm = chest.LocateFSM("Chest Control");

            FsmState init = chestFsm.GetState("Init");
            FsmState spawnItems = chestFsm.GetState("Spawn Items");

            FsmStateAction checkAction = new Lambda(() => chestFsm.SendEvent(AllObtained() ? "ACTIVATE" : null));

            init.RemoveActionsOfType<BoolTest>();
            init.AddLastAction(checkAction);

            // Destroy any existing shinies in the chest
            // Moved to MakeNewChest, this code can likely be removed safely
            GameObject itemParent = chestFsm.gameObject.transform.Find("Item").gameObject;
            foreach (Transform t in itemParent.transform)
            {
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Remove pre-existing geo from chest
            foreach (FlingObjectsFromGlobalPool fling in spawnItems.GetActionsOfType<FlingObjectsFromGlobalPool>())
            {
                fling.spawnMin = 0;
                fling.spawnMax = 0;
            }

            // Need to check SpawnFromPool action too because of Mantis Lords chest
            foreach (SpawnFromPool spawn in spawnItems.GetActionsOfType<SpawnFromPool>())
            {
                spawn.spawnMin = 0;
                spawn.spawnMax = 0;
            }

            void OnOpenChest()
            {
                foreach (AbstractItem item in Items)
                {
                    Cost cost = item.GetTag<CostTag>()?.Cost;

                    if (!item.IsObtained())
                    {
                        if (cost != null && !cost.Paid && !cost.CanPay()) continue;
                        if (cost != null && !cost.Paid) cost.Pay();
                        if (item.GiveEarly(Container.Chest))
                        {
                            item.Give(this, new GiveInfo
                            {
                                Container = Container.Chest,
                                FlingType = chestLocation.flingType,
                                Transform = chestFsm.gameObject.transform,
                                MessageType = MessageType.Corner,
                            });
                        }
                        else
                        {
                            GameObject shiny = ShinyUtility.MakeNewShiny(this, item, chestLocation.flingType);
                            ShinyUtility.PutShinyInContainer(itemParent, shiny);
                            ShinyUtility.FlingShinyRandomly(shiny.LocateFSM("Shiny Control"));
                        }
                    }

                    foreach (Transform t in itemParent.transform) t.gameObject.SetActive(true);
                }
            }

            spawnItems.AddLastAction(new Lambda(OnOpenChest));
        }

        public string BuildText()
        {
            StringBuilder sb = new("Chest Contents<br>");
            Tags.MultiPreviewRecordTag recordTag = GetOrAddTag<Tags.MultiPreviewRecordTag>();
            recordTag.previewTexts = new string[Items.Count];
            for (int i = 0; i < Items.Count; i++)
            {
                AbstractItem item = Items[i];
                Cost cost = item.GetTag<CostTag>()?.Cost;

                sb.Append("<br>");
                string text = item.GetPreviewName(this) + "  -  ";
                if (item.IsObtained())
                {
                    text += "已获得";
                }
                else if (cost is null)
                {
                    text += "免费";
                }
                else if (cost.Paid)
                {
                    text += "已交易";
                }
                else if (HasTag<Tags.DisableCostPreviewTag>() || item.HasTag<Tags.DisableCostPreviewTag>())
                {
                    text += "???";
                }
                else
                {
                    text += cost.GetCostText();
                }
                recordTag.previewTexts[i] = text;
                sb.Append(text);
            }
            AddVisitFlag(VisitState.Previewed);
            return sb.ToString();
        }

        public override IEnumerable<Tag> GetPlacementAndLocationTags()
        {
            return base.GetPlacementAndLocationTags()
                .Concat(chestLocation.tags ?? Enumerable.Empty<Tag>())
                .Concat(tabletLocation.tags ?? Enumerable.Empty<Tag>());
        }
    }
}
