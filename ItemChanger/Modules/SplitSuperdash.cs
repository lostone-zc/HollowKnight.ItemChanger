using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
#pragma warning disable IDE1006 // Naming Styles

namespace ItemChanger.Modules
{
    /// <summary>
    /// Module which implements the split superdash custom skills.
    /// </summary>
    public class SplitSuperdash : Module
    {
        public bool hasSuperdashLeft { get; set; }
        public bool hasSuperdashRight { get; set; }
        public MylaDeathCondition MylaDeathHandling = MylaDeathCondition.DieAfterFullSuperdash;
        [Newtonsoft.Json.JsonIgnore] public bool hasSuperdashAny => hasSuperdashLeft ^ hasSuperdashRight || PlayerData.instance.GetBoolInternal(nameof(PlayerData.hasSuperDash));
        [Newtonsoft.Json.JsonIgnore] public bool hasSuperdashBoth => (hasSuperdashLeft && hasSuperdashRight) || PlayerData.instance.GetBoolInternal(nameof(PlayerData.hasSuperDash));

        public override void Initialize()
        {
            Events.AddFsmEdit(new("Equipment", "Build Equipment List"), EditInventory);
            Events.AddFsmEdit(SceneNames.Mines_31, new("mine_break_bridge", "Bridge Control"), PreventBridgeBreak);
            Events.AddLanguageEdit(new("UI", "INV_NAME_SUPERDASH"), EditSuperdashName);
            Events.AddLanguageEdit(new("UI", "INV_DESC_SUPERDASH"), EditSuperdashDesc);
            Modding.ModHooks.GetPlayerBoolHook += SkillBoolGetOverride;
            Modding.ModHooks.SetPlayerBoolHook += SkillBoolSetOverride;
            On.DeactivateIfPlayerdataTrue.OnEnable += SetMylaDeathCondition;
            On.DeactivateIfPlayerdataFalse.OnEnable += SetMylaDeathCondition;
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(new("Equipment", "Build Equipment List"), EditInventory);
            Events.RemoveFsmEdit(SceneNames.Mines_31, new("mine_break_bridge", "Bridge Control"), PreventBridgeBreak);
            Events.RemoveLanguageEdit(new("UI", "INV_NAME_SUPERDASH"), EditSuperdashName);
            Events.RemoveLanguageEdit(new("UI", "INV_DESC_SUPERDASH"), EditSuperdashDesc);
            Modding.ModHooks.GetPlayerBoolHook -= SkillBoolGetOverride;
            Modding.ModHooks.SetPlayerBoolHook -= SkillBoolSetOverride;
            On.DeactivateIfPlayerdataTrue.OnEnable -= SetMylaDeathCondition;
            On.DeactivateIfPlayerdataFalse.OnEnable -= SetMylaDeathCondition;
        }

        private void EditInventory(PlayMakerFSM fsm)
        {
            fsm.GetState("Super Dash").GetFirstActionOfType<PlayerDataBoolTest>().boolName.Value = nameof(hasSuperdashAny);
        }

        private bool SkillBoolGetOverride(string boolName, bool value)
        {
            return boolName switch
            {
                nameof(hasSuperdashLeft) => hasSuperdashLeft,
                nameof(hasSuperdashRight) => hasSuperdashRight,
                nameof(PlayerData.hasSuperDash) => value
                    || HeroController.instance.cState.onGround && (HeroController.instance.cState.facingRight ? hasSuperdashRight : hasSuperdashLeft)
                    || HeroController.instance.cState.wallSliding && (HeroController.instance.cState.facingRight ? hasSuperdashLeft : hasSuperdashRight),
                nameof(hasSuperdashAny) => hasSuperdashAny,
                nameof(hasSuperdashBoth) => hasSuperdashBoth,
                _ => value,
            };
        }

        private bool SkillBoolSetOverride(string boolName, bool value)
        {
            switch (boolName)
            {
                case nameof(hasSuperdashLeft):
                    hasSuperdashLeft = value;
                    goto BoolChanged;

                case nameof(hasSuperdashRight):
                    hasSuperdashRight = value;
                    goto BoolChanged;

                BoolChanged:
                    if (hasSuperdashLeft && hasSuperdashRight)
                    {
                        PlayerData.instance.SetBool(nameof(PlayerData.hasSuperDash), true);
                        PlayerData.instance.SetBool(nameof(PlayerData.canSuperDash), true);
                    }
                    break;
            }
            return value;
        }

        private void EditSuperdashName(ref string value)
        {
            if (hasSuperdashLeft && !hasSuperdashRight)
            {
                value = "左水晶之心";
            }
            else if (!hasSuperdashLeft && hasSuperdashRight)
            {
                value = "右水晶之心";
            }
        }

        private void EditSuperdashDesc(ref string value)
        {
            if (hasSuperdashLeft && !hasSuperdashRight)
            {
                value = "一个古老挖矿魔像的能量核心的一部分，以一块强力的水晶塑成。水晶的能量可以被引导，能将持有者以危险的速度向左发射出去。";
            }
            else if (!hasSuperdashLeft && hasSuperdashRight)
            {
                value = "一个古老挖矿魔像的能量核心的一部分，以一块强力的水晶塑成。水晶的能量可以被引导，能将持有者以危险的速度向右发射出去。";
            }
        }

        private void PreventBridgeBreak(PlayMakerFSM fsm)
        {
            fsm.GetState("Idle").GetFirstActionOfType<PlayerDataBoolTest>().boolName = nameof(hasSuperdashBoth);
        }       

        public enum MylaDeathCondition
        {
            Vanilla,
            DieAfterFullSuperdash = Vanilla,
            DieAfterEitherSuperdash,
            DieAfterLeftSuperdash,
            DieAfterRightSuperdash,
        }

        private void SetMylaDeathCondition(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
        {
            if (self.gameObject.scene.name == SceneNames.Crossroads_45 && self.boolName == nameof(PlayerData.hasSuperDash))
            {
                self.boolName = MylaDeathHandling switch
                {
                    MylaDeathCondition.DieAfterFullSuperdash => nameof(hasSuperdashBoth),
                    MylaDeathCondition.DieAfterRightSuperdash => nameof(hasSuperdashRight),
                    MylaDeathCondition.DieAfterLeftSuperdash => nameof(hasSuperdashLeft),
                    MylaDeathCondition.DieAfterEitherSuperdash => nameof(hasSuperdashAny),
                    _ => nameof(PlayerData.canSuperDash),
                };
            }
            orig(self);
        }
        private void SetMylaDeathCondition(On.DeactivateIfPlayerdataFalse.orig_OnEnable orig, DeactivateIfPlayerdataFalse self)
        {
            if (self.gameObject.scene.name == SceneNames.Crossroads_45 && self.boolName == nameof(PlayerData.hasSuperDash))
            {
                self.boolName = MylaDeathHandling switch
                {
                    MylaDeathCondition.DieAfterFullSuperdash => nameof(hasSuperdashBoth),
                    MylaDeathCondition.DieAfterRightSuperdash => nameof(hasSuperdashRight),
                    MylaDeathCondition.DieAfterLeftSuperdash => nameof(hasSuperdashLeft),
                    MylaDeathCondition.DieAfterEitherSuperdash => nameof(hasSuperdashAny),
                    _ => nameof(PlayerData.canSuperDash),
                };
            }
            orig(self);
        }
    }
}