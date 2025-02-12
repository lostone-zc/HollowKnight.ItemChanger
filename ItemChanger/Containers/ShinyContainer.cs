﻿using ItemChanger.Components;
using ItemChanger.Util;

namespace ItemChanger.Containers
{
    /// <summary>
    /// Container for creating and modifying shiny items.
    /// </summary>
    public class ShinyContainer : Container
    {
        public override string Name => Container.Shiny;
        public override bool SupportsCost => true;
        public override bool SupportsSceneChange => true;
        public override bool SupportsDrop => true;
        public override bool SupportsInstantiate => true;

        public override GameObject GetNewContainer(AbstractPlacement placement, IEnumerable<AbstractItem> items, FlingType flingType, Cost cost = null, Transition? changeSceneTo = null)
        {
            return ShinyUtility.MakeNewMultiItemShiny(placement, items, flingType, cost, changeSceneTo);
        }

        public override void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info)
        {
            if (fsm.FsmName != "Shiny Control") return;
            ShinyUtility.ModifyMultiShiny(fsm, info.flingType, info.placement, info.items);
        }

        public override void AddChangeSceneToFsm(PlayMakerFSM fsm, ChangeSceneInfo info)
        {
            if (fsm.FsmName != "Shiny Control") return;
            ShinyUtility.AddChangeSceneToShiny(fsm, info.transition);
        }

        public override void AddCostToFsm(PlayMakerFSM fsm, CostInfo info)
        {
            if (fsm.FsmName != "Shiny Control") return;
            ShinyUtility.AddYNDialogueToShiny(fsm, info.cost, info.placement, info.previewItems);
        }
    }
}
