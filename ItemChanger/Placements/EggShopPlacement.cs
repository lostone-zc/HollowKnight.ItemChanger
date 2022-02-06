﻿using System.Text;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;

namespace ItemChanger.Placements
{
    /// <summary>
    /// Placement which implements a shop at Jiji. 
    /// <br />Talking to Jiji gives the option to pay costs, and choosing to do so leads to Jiji spawning the corresponding items wrapped in individual containers.
    /// </summary>
    public class EggShopPlacement : AbstractPlacement, IMultiCostPlacement, IPrimaryLocationPlacement
    {
        public EggShopPlacement(string Name) : base(Name) { }

        public Dictionary<int, string> containers = new();

        public PlaceableLocation Location;
        AbstractLocation IPrimaryLocationPlacement.Location => Location;

        [Newtonsoft.Json.JsonProperty]
        public bool PurchasedAll { get; private set; }

        protected override void OnLoad()
        {
            Events.AddSceneChangeEdit(Location.sceneName, OnActiveSceneChanged);
            Events.AddFsmEdit(SceneNames.Room_Ouiji, new("Jiji NPC", "Conversation Control"), JijiConvoEdit);
            Events.AddLanguageEdit(new("Jiji", "GREET"), JijiGreet);
            Events.AddLanguageEdit(new("Jiji", "SHADE_OFFER"), JijiShadeOffer);
            Events.AddLanguageEdit(new("Jiji", "DECLINE"), JijiDecline);
            Events.AddLanguageEdit(new("Prompts", "JIJI_OFFER"), JijiYNOffer);
        }

        protected override void OnUnload()
        {
            Events.RemoveSceneChangeEdit(Location.sceneName, OnActiveSceneChanged);
            Events.RemoveFsmEdit(SceneNames.Room_Ouiji, new("Jiji NPC", "Conversation Control"), JijiConvoEdit);
            Events.RemoveLanguageEdit(new("Jiji", "GREET"), JijiGreet);
            Events.RemoveLanguageEdit(new("Jiji", "SHADE_OFFER"), JijiShadeOffer);
            Events.RemoveLanguageEdit(new("Jiji", "DECLINE"), JijiDecline);
            Events.RemoveLanguageEdit(new("Prompts", "JIJI_OFFER"), JijiYNOffer);
        }

        public bool CanPurchaseAny() => Items.Any(i => i.GetTag<CostTag>()?.Cost is Cost c && !c.Paid && c.CanPay());

        public void PurchaseAndSpawnNewItems()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (!Items[i].IsObtained() && Items[i].GetTag<CostTag>()?.Cost is Cost c && !c.Paid && c.CanPay())
                {
                    c.Pay();
                    SpawnItem(i);
                }
            }
        }

        public void SpawnItem(int i)
        {
            if (!containers.TryGetValue(i, out string container))
            {
                container = Items[i].GetPreferredContainer();
                if (string.IsNullOrEmpty(container) || container == Container.Unknown) container = Container.Shiny;
                containers[i] = container;
            }
            Container c = Container.GetContainer(container);
            if (c == null || !c.SupportsInstantiate)
            {
                containers[i] = Container.Shiny;
                c = Container.GetContainer(Container.Shiny);
            }

            GameObject obj = c.GetNewContainer(this, Items[i].Yield(), Location.flingType);
            Location.PlaceContainer(obj, container);
            obj.transform.Translate(new((i % 5) - 2f, 0));
            obj.SetActive(true);
        }

        private void OnActiveSceneChanged(Scene to)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (!Items[i].IsObtained() && (Items[i].GetTag<CostTag>()?.Cost?.Paid ?? true))
                {
                    SpawnItem(i);
                }
            }

            PurchasedAll = Items.All(i => i.GetTag<CostTag>()?.Cost?.Paid ?? true);
        }

        private void JijiConvoEdit(PlayMakerFSM jijiFsm)
        {
            if (PurchasedAll) return;

            // Set the "Black Wave" to white
            Transform bw = jijiFsm.transform.Find("Black Wave");
            bw.GetComponent<WaveEffectControl>().blackWave = false;
            bw.GetComponent<SpriteRenderer>().color = Color.white;

            FsmState convoChoice = jijiFsm.GetState("Convo Choice");
            FsmState greet = jijiFsm.GetState("Greet");
            FsmState offer = jijiFsm.GetState("Offer");
            FsmState haveEggs = jijiFsm.GetState("Have Eggs?");
            FsmState yes = jijiFsm.GetState("Yes");
            FsmState spawn = jijiFsm.GetState("Spawn");

            convoChoice.Actions = new FsmStateAction[]
            {
                    convoChoice.Actions[1], // AudioPlayerOneShot, all other actions trigger alternate conversations
            };
            convoChoice.AddTransition(FsmEvent.Finished, greet); // Always display the Jiji:GREET convo

            greet.ClearTransitions();
            greet.AddTransition("CONVO_FINISH", offer); // Always display the Jiji:SHADE_OFFER convo

            // replace IntCompare for rancid eggs with test based on item costs
            haveEggs.Actions = new FsmStateAction[]
            {
                new DelegateBoolTest(CanPurchaseAny, "YES", "NO"),
            };

            // remove shade info edits
            yes.RemoveActionsOfType<SetPlayerDataString>();
            yes.RemoveActionsOfType<SetPlayerDataInt>();
            yes.RemoveActionsOfType<SetPlayerDataFloat>();
            yes.RemoveActionsOfType<PlayerDataIntAdd>();

            // Jiji:RITUAL_BEGIN

            spawn.RemoveActionsOfType<CreateObject>();
            spawn.AddLastAction(new Lambda(() =>
            {
                PurchaseAndSpawnNewItems();
            }));
        }

        private void JijiGreet(ref string value)
        {
            if (!PurchasedAll)
            {
                value = "啊，你好。你最近怎么样？你给我带好吃的了吗？让我看看...";
            }
        }
        private void JijiShadeOffer(ref string value)
        {
            if (!PurchasedAll)
            {
                StringBuilder sb = new();
                Tags.MultiPreviewRecordTag recordTag = GetOrAddTag<Tags.MultiPreviewRecordTag>();
                recordTag.previewTexts = new string[Items.Count];
                for (int i = 0; i < Items.Count; i++)
                {
                    AbstractItem item = Items[i];
                    string text = item.GetPreviewName(this) + "  -  ";
                    Cost cost = item.GetTag<CostTag>()?.Cost;
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
                    sb.AppendLine(text);
                }
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length); // remove last line terminator
                }
                value = sb.ToString();
                AddVisitFlag(VisitState.Previewed);
            }
        }
        private void JijiDecline(ref string value)
        {
            if (!PurchasedAll)
            {
                value = "哦，我的老伙计，如果你看不上我这的东西，我就没啥能帮你的了";
            }
        }
        private void JijiYNOffer(ref string value)
        {
            if (!PurchasedAll)
            {
                value = "给吉吉你所有的腐臭蛋?";
            }
        }

        public override IEnumerable<Tag> GetPlacementAndLocationTags()
        {
            return base.GetPlacementAndLocationTags().Concat(Location.tags ?? Enumerable.Empty<Tag>());
        }
    }
}
