﻿using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using Modding;
#pragma warning disable IDE1006 // Naming Styles

namespace ItemChanger.Modules
{
    /// <summary>
    /// Module which implements the split cloak custom skills.
    /// </summary>
    public class SplitCloak : Module
    {
        public bool canDashLeft { get; set; }
        public bool canDashRight { get; set; }
        [Newtonsoft.Json.JsonIgnore] public bool hasDashAny => canDashLeft ^ canDashRight || PlayerData.instance.GetBoolInternal(nameof(PlayerData.hasDash));

        public override void Initialize()
        {
            Events.AddFsmEdit(new("Equipment", "Build Equipment List"), EditInventory);
            ModHooks.GetPlayerBoolHook += SkillBoolGetOverride;
            ModHooks.SetPlayerBoolHook += SkillBoolSetOverride;
            On.HeroController.CanDash += ModifyDash;
            Events.AddLanguageEdit(new("UI", "INV_NAME_DASH"), EditMothwingCloakName);
            Events.AddLanguageEdit(new("UI", "INV_NAME_SHADOWDASH"), EditShadeCloakName);
            Events.AddLanguageEdit(new("UI", "INV_DESC_DASH"), EditMothwingCloakDesc);
            Events.AddLanguageEdit(new("UI", "INV_DESC_SHADOWDASH"), EditShadeCloakDesc);
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(new("Equipment", "Build Equipment List"), EditInventory);
            ModHooks.GetPlayerBoolHook -= SkillBoolGetOverride;
            ModHooks.SetPlayerBoolHook -= SkillBoolSetOverride;
            On.HeroController.CanDash -= ModifyDash;
            Events.RemoveLanguageEdit(new("UI", "INV_NAME_DASH"), EditMothwingCloakName);
            Events.RemoveLanguageEdit(new("UI", "INV_NAME_SHADOWDASH"), EditShadeCloakName);
            Events.RemoveLanguageEdit(new("UI", "INV_DESC_DASH"), EditMothwingCloakDesc);
            Events.RemoveLanguageEdit(new("UI", "INV_DESC_SHADOWDASH"), EditShadeCloakDesc);
        }

        private void EditInventory(PlayMakerFSM fsm)
        {
            PlayerDataBoolTest[] dashChecks = fsm.GetState("Dash").GetActionsOfType<PlayerDataBoolTest>();
            dashChecks[0].boolName.Value = nameof(hasDashAny);
        }

        private bool SkillBoolGetOverride(string boolName, bool value)
        {
            return boolName switch
            {
                nameof(PlayerData.canDash) => canDashLeft ^ canDashRight || PlayerData.instance.GetBoolInternal(nameof(PlayerData.canDash)),// note canDash, not hasDash
                nameof(canDashLeft) => canDashLeft,
                nameof(canDashRight) => canDashRight,
                nameof(hasDashAny) => hasDashAny,
                _ => value,
            };
        }

        private bool SkillBoolSetOverride(string boolName, bool value)
        {
            switch (boolName)
            {
                // bools for left and right cloak
                case nameof(canDashLeft):
                    canDashLeft = value;
                    goto BoolChanged;
                case nameof(canDashRight):
                    canDashRight = value;
                    goto BoolChanged;
                BoolChanged:
                    if (canDashLeft && canDashRight)
                    {
                        PlayerData.instance.SetBool(nameof(PlayerData.hasDash), true);
                        PlayerData.instance.SetBool(nameof(PlayerData.canDash), true);
                    }
                    break;
            }

            return value;
        }
        

        private bool ModifyDash(On.HeroController.orig_CanDash orig, HeroController self)
        {
            // Only disable dash in a direction if they have it in the other direction. If they have both or neither dash
            // direction, then it will be handled by the original function.
            // We don't need to check if Split Cloak is active, because we only change the output if the player has exactly one cloak piece
            switch (GetDashDirection(self))
            {
                default:
                    return orig(self);
                case Direction.leftward:
                    return orig(self) && (!canDashRight || canDashLeft);
                case Direction.rightward:
                    return orig(self) && (!canDashLeft || canDashRight);
                case Direction.downward:
                    return orig(self);
            }
        }

        private static Direction GetDashDirection(HeroController hc)
        {
            InputHandler input = InputHandler.Instance;
            if (!hc.cState.onGround && input.inputActions.down.IsPressed && hc.playerData.GetBool("equippedCharm_31")
                    && !(input.inputActions.left.IsPressed || input.inputActions.right.IsPressed))
            {
                return Direction.downward;
            }
            if (hc.wallSlidingL) return Direction.rightward;
            else if (hc.wallSlidingR) return Direction.leftward;
            else if (input.inputActions.right.IsPressed) return Direction.rightward;
            else if (input.inputActions.left.IsPressed) return Direction.leftward;
            else if (hc.cState.facingRight) return Direction.rightward;
            else return Direction.leftward;
        }

        private enum Direction
        {
            upward,
            leftward,
            rightward,
            downward
        }

        private void EditMothwingCloakName(ref string value)
        {
            if (canDashLeft && !canDashRight)
            {
                value = "左蛾翼披风";
            }
            else if (!canDashLeft && canDashRight)
            {
                value = "右蛾翼披风";
            }
        }

        private void EditShadeCloakName(ref string value)
        {
            if (canDashLeft && !canDashRight)
            {
                value = "左暗影披风";
            }
            else if (!canDashLeft && canDashRight)
            {
                value = "右暗影披风";
            }
        }

        private void EditMothwingCloakDesc(ref string value)
        {
            if (canDashLeft && !canDashRight)
            {
                value = "蛾翅线编制的披风的一部分。让穿着者可在地面或空中向左冲刺。";
            }
            else if (!canDashLeft && canDashRight)
            {
                value = "蛾翅线编制的披风的一部分。让穿着者可在地面或空中向右冲刺。";
            }
        }

        private void EditShadeCloakDesc(ref string value)
        {
            if (canDashLeft && !canDashRight)
            {
                value = "由深渊的物质形成的披风的一部分。允许穿戴者向左冲刺时穿过敌人和他们的攻击而不受到伤害。";
            }
            else if (!canDashLeft && canDashRight)
            {
                value = "由深渊的物质形成的披风的一部分。允许穿戴者向右冲刺时穿过敌人和他们的攻击而不受到伤害。";
            }
        }
    }
}
