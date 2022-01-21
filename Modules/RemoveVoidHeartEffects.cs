﻿using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;

namespace ItemChanger.Modules
{
    /// <summary>
    /// Module which makes Void Heart unequippable and makes Siblings and the Shade hostile when Void Heart is not equipped.
    /// </summary>
    [DefaultModule]
    public class RemoveVoidHeartEffects : Module
    {
        public override void Initialize()
        {
            Events.AddFsmEdit(new("Charms", "UI Charms"), AllowVoidHeartUnequip);
            Events.AddFsmEdit(new("Shade Control"), PreventFriendlyShade);
            Events.AddFsmEdit(new("Control"), PreventFriendlySibling);
            Events.AddLanguageEdit(new("UI", "CHARM_DESC_36_C"), EditVoidHeartDescription);
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(new("Charms", "UI Charms"), AllowVoidHeartUnequip);
            Events.RemoveFsmEdit(new("Shade Control"), PreventFriendlyShade);
            Events.RemoveFsmEdit(new("Control"), PreventFriendlySibling);
            Events.RemoveLanguageEdit(new("UI", "CHARM_DESC_36_C"), EditVoidHeartDescription);
        }

        private void AllowVoidHeartUnequip(PlayMakerFSM fsm)
        {
            FsmState equipped = fsm.GetState("Equipped?");
            FsmState setCurrentItemNum = fsm.GetState("Set Current Item Num");

            equipped.RemoveTransitionsTo("Black Charm? 2");
            equipped.AddTransition("EQUIPPED", "Return Points");
            setCurrentItemNum.RemoveTransitionsTo("Black Charm?");
            setCurrentItemNum.AddTransition("FINISHED", "Return Points");
        }

        private bool TestVoidHeartEquipped()
        {
            return PlayerData.instance.GetInt(nameof(PlayerData.royalCharmState)) >= 4 && PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_36));
        }

        private void PreventFriendlyShade(PlayMakerFSM fsm)
        {
            if (fsm.gameObject != null && fsm.gameObject.name.StartsWith("Hollow Shade") && !fsm.gameObject.name.StartsWith("Hollow Shade Death"))
            {
                FsmState friendly = fsm.GetState("Friendly?");
                if (friendly == null) return;
                friendly.RemoveFirstActionOfType<IntCompare>();
                friendly.RemoveFirstActionOfType<GetPlayerDataInt>();
                friendly.AddFirstAction(new DelegateBoolTest(TestVoidHeartEquipped, null, FsmEvent.Finished));
            }
        }

        private void PreventFriendlySibling(PlayMakerFSM fsm)
        {
            if (fsm.gameObject != null && fsm.gameObject.name.StartsWith("Shade Sibling"))
            {
                FsmState friendly = fsm.GetState("Friendly?");
                if (friendly == null) return;

                friendly.RemoveFirstActionOfType<GetPlayerDataInt>();
                friendly.RemoveFirstActionOfType<IntCompare>();
                friendly.InsertAction(new DelegateBoolTest(TestVoidHeartEquipped, null, FsmEvent.Finished), 2);
            }
        }

        private void EditVoidHeartDescription(ref string value)
        {
            value = "隐藏在内部的空虚，现在不再受到约束。联合了持有者意志下的虚无存在。\n这个护符可以移除，以恢复兄弟和阴影的敌对性";
        }
    }
}
