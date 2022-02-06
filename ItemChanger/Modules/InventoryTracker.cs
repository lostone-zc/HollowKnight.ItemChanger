using System.Text;
using ItemChanger.Internal;
using Newtonsoft.Json;

namespace ItemChanger.Modules
{
    /// <summary>
    /// Module which adds extra information to the inventory for grubs, dreamers, and similar things that cannot otherwise be checked.
    /// </summary>
    [DefaultModule]
    public class InventoryTracker : Module
    {
        public bool TrackGrimmkinFlames = true;
        [field: JsonIgnore] public event Action<StringBuilder> OnGenerateFocusDesc;

        public override void Initialize()
        {
            Events.AddLanguageEdit(new("UI", "INV_NAME_SPELL_FOCUS"), EditFocusName);
            Events.AddLanguageEdit(new("UI", "INV_DESC_SPELL_FOCUS"), EditFocusDesc);
            for (int i = 1; i <= 5; i++) Events.AddLanguageEdit(new("UI", "INV_DESC_NAIL" + i), EditNailDesc);
        }

        public override void Unload()
        {
            Events.RemoveLanguageEdit(new("UI", "INV_NAME_SPELL_FOCUS"), EditFocusName);
            Events.RemoveLanguageEdit(new("UI", "INV_DESC_SPELL_FOCUS"), EditFocusDesc);
            for (int i = 1; i <= 5; i++) Events.RemoveLanguageEdit(new("UI", "INV_DESC_NAIL" + i), EditNailDesc);
        }

        private void EditFocusName(ref string value) => value = "Tracker";

        private void EditFocusDesc(ref string value)
        {
            StringBuilder sb = new();
            ModuleCollection mods = ItemChangerMod.Modules;
            FocusSkill fs = mods.Get<FocusSkill>();
            SwimSkill ss = mods.Get<SwimSkill>();

            if (fs != null)
            {
                if (fs.canFocus) sb.Append("你能聚集。");
                else sb.Append("你不能聚集。");

                if (ss != null) sb.Append(' ');
                else sb.AppendLine();
            }

            if (ss != null)
            {
                if (ss.canSwim) sb.Append("你能游泳。");
                else sb.Append("你不能游泳。");
                sb.AppendLine();
            }

            if (!Ref.PD.GetBool(nameof(Ref.PD.hasDreamNail)))
            {
                int essence = Ref.PD.GetInt(nameof(Ref.PD.dreamOrbs));
                if (essence > 0) sb.AppendLine($"你有 {essence} 精华。");
            }

            if (TrackGrimmkinFlames && PlayerData.instance.GetInt(nameof(PlayerData.grimmChildLevel)) <= 3)
            {
                if (mods.Get<GrimmkinFlameManager>() is GrimmkinFlameManager gfm)
                {
                    sb.AppendLine($"你有 {gfm.flameBalance} 未使用的格林火焰。（总共：{gfm.cumulativeFlamesCollected}）");
                }
                else
                {
                    sb.AppendLine($"你有 {PlayerData.instance.GetInt(nameof(PlayerData.flamesCollected))} 未使用的格林火焰。");
                } 
            }

            sb.AppendLine($"你目前救了 {Ref.PD.GetInt(nameof(PlayerData.grubsCollected))} 幼虫。");
            int dreamers = Ref.PD.GetInt(nameof(PlayerData.guardiansDefeated));
            sb.Append($"你目前找到了 {dreamers} 守梦者。");
            if (dreamers > 0)
            {
                sb.AppendLine("，包括：");
                bool lurien = Ref.PD.GetBool(nameof(PlayerData.lurienDefeated));
                bool monomon = Ref.PD.GetBool(nameof(PlayerData.monomonDefeated));
                bool herrah = Ref.PD.GetBool(nameof(PlayerData.hegemolDefeated));

                if (lurien)
                {
                    sb.Append("卢瑞恩, ");
                    dreamers--;
                }
                if (monomon)
                {
                    sb.Append("莫诺蒙, ");
                    dreamers--;
                }
                if (herrah)
                {
                    sb.Append("赫拉, ");
                    dreamers--;
                }
                if (dreamers > 0)
                {
                    sb.Append("复制的守梦者");
                }
            }
            sb.AppendLine();

            OnGenerateFocusDesc?.Invoke(sb);

            value = sb.ToString();
        }

        private void EditNailDesc(ref string value)
        {
            SplitNail sn = ItemChangerMod.Modules.Get<SplitNail>();

            if (sn != null)
            {
                StringBuilder sb = new();

                string[] abilities = new[]
                {
                    (sn.canDownslash, "下"),
                    (sn.canUpslash, "上"),
                    (sn.canSideslashLeft, "左"),
                    (sn.canSideslashRight, "右"),
                }.Where(p => p.Item1).Select(p => p.Item2).ToArray();

                if (abilities.Length > 0)
                {
                    sb.Append("<br><br>可以向");
                    sb.Append(string.Join("、", abilities));
                    sb.Append("挥舞。");
                }
                else
                {
                    sb.Append("<br><br>不能被挥舞。");
                }

                value += sb.ToString();
            }
        }
    }
}
