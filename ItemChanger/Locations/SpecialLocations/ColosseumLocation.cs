﻿using System.Text;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using ItemChanger.Extensions;

namespace ItemChanger.Locations.SpecialLocations
{
    /// <summary>
    /// FsmObjectLocation with various changes to support items at the end of a Colosseum trial.
    /// </summary>
    public class ColosseumLocation : FsmObjectLocation, ILocalHintLocation
    {
        public bool HintActive { get; set; } = true;

        protected override void OnLoad()
        {
            base.OnLoad();
            Events.AddFsmEdit(sceneName, new("Colosseum Manager", "Geo Pool"), ChangeColoEnd);
            Events.AddLanguageEdit(new("Prompts", GetTrialBoardConvo()), OnLanguageGet);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            Events.RemoveFsmEdit(sceneName, new("Colosseum Manager", "Geo Pool"), ChangeColoEnd);
            Events.RemoveLanguageEdit(new("Prompts", GetTrialBoardConvo()), OnLanguageGet);
        }

        private void ChangeColoEnd(PlayMakerFSM fsm)
        {
            FsmState openGates = fsm.GetState("Open Gates");
            openGates.AddFirstAction(new Lambda(SetCompletionBool));
            FsmState giveShiny = fsm.GetState("Give Shiny?");
            giveShiny.Actions = new[]
            {
                giveShiny.Actions[0], // CROWD IDLE
                // giveShiny.Actions[1], // bool test on FsmBool Shiny Item
                new DelegateBoolTest(Placement.AllObtained, (PlayerDataBoolTest)giveShiny.Actions[2]),
                // giveShiny.Actions[3], // find child
                giveShiny.Actions[4], // activate Shiny Obj
            };
            giveShiny.AddTransition("FINISHED", "Geo Given Pause");
        }

        private void SetCompletionBool()
        {
            PlayerData.instance.SetBool(GetCompletionBoolName(), true);
        }

        private string GetCompletionBoolName()
        {
            switch (sceneName)
            {
                default:
                case SceneNames.Room_Colosseum_Bronze:
                    return nameof(PlayerData.colosseumBronzeCompleted);
                case SceneNames.Room_Colosseum_Silver:
                    return nameof(PlayerData.colosseumSilverCompleted);
                case SceneNames.Room_Colosseum_Gold:
                    return nameof(PlayerData.colosseumGoldCompleted);
            }
        }

        private string GetTrialBoardConvo()
        {
            switch (sceneName)
            {
                default:
                case SceneNames.Room_Colosseum_Bronze:
                    return "TRIAL_BOARD_BRONZE";
                case SceneNames.Room_Colosseum_Silver:
                    return "TRIAL_BOARD_SILVER";
                case SceneNames.Room_Colosseum_Gold:
                    return "TRIAL_BOARD_GOLD";
            }
        }

        private string GetTrialBoardHint(string itemText)
        {
            StringBuilder sb = new();
            switch (sceneName)
            {
                default:
                case SceneNames.Room_Colosseum_Bronze:
                    sb.Append("勇士的试炼。");
                    break;
                case SceneNames.Room_Colosseum_Silver:
                    sb.Append("征服者的试炼。");
                    break;
                case SceneNames.Room_Colosseum_Gold:
                    sb.Append("愚人的试炼。");
                    break;
            }

            sb.AppendLine($"为吉欧和 {itemText} 而战。");
            sb.Append("放置标记，开始试炼？");
            return sb.ToString();
        }

        private string GetTrialBoardNullHint()
        {
            StringBuilder sb = new();
            switch (sceneName)
            {
                default:
                case SceneNames.Room_Colosseum_Bronze:
                    sb.Append("勇士的试炼。");
                    break;
                case SceneNames.Room_Colosseum_Silver:
                    sb.Append("征服者的试炼。");
                    break;
                case SceneNames.Room_Colosseum_Gold:
                    sb.Append("愚人的试炼。");
                    break;
            }

            sb.AppendLine($"为吉欧而战。");
            sb.Append("放置标记，开始试炼？");
            return sb.ToString();
        }

        private void OnLanguageGet(ref string value)
        {
            if (this.GetItemHintActive() && !Placement.AllObtained())
            {
                string text = Placement.GetUIName(75);
                value = GetTrialBoardHint(text);
                Placement.OnPreview(text);
            }
            else value = GetTrialBoardNullHint();
        }
    }
}
