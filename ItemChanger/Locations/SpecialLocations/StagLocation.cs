using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using ItemChanger.Extensions;

namespace ItemChanger.Locations.SpecialLocations
{
    /// <summary>
    /// CoordinateLocation which also edits the stag by removing the toll and allowing use of the stag without unlocking the station.
    /// </summary>
    public class StagLocation : CoordinateLocation
    {
        protected override void OnLoad()
        {
            base.OnLoad();
            Events.AddFsmEdit(sceneName, new("Stag", "Stag Control"), EditStagControl);
            Events.AddFsmEdit(sceneName, new("UI List Stag", "ui_list"), EditUIList);
            if (sceneName == SceneNames.RestingGrounds_09)
            {
                Events.AddFsmEdit(sceneName, new("Station Bell Lever", "Stag Bell"), EditStationBell);
            }
            else
            {
                Events.AddFsmEdit(sceneName, new("Station Bell", "Stag Bell"), EditStationBell);
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            Events.RemoveFsmEdit(sceneName, new("Stag", "Stag Control"), EditStagControl);
            Events.RemoveFsmEdit(sceneName, new("UI List Stag", "ui_list"), EditUIList);
            if (sceneName == SceneNames.RestingGrounds_09)
            {
                Events.RemoveFsmEdit(sceneName, new("Station Bell Lever", "Stag Bell"), EditStationBell);
            }
            else
            {
                Events.RemoveFsmEdit(sceneName, new("Station Bell", "Stag Bell"), EditStationBell);
            }
        }

        private void EditStagControl(PlayMakerFSM fsm)
        {
            FsmState hsprompt = fsm.GetState("Hidden Station?");
            FsmState openGrate = fsm.GetState("Open Grate");
            FsmState currentLocationCheck = fsm.GetState("Current Location Check");
            FsmState checkResult = fsm.GetState("Check Result");
            FsmState hudreturn = fsm.GetState("HUD Return");

            if (sceneName == "Abyss_22") hsprompt.RemoveActionsOfType<IntCompare>();

            openGrate.RemoveActionsOfType<SetPlayerDataBool>();
            openGrate.RemoveActionsOfType<SetBoolValue>();

            FsmBool cancelTravel = fsm.AddFsmBool("Cancel Travel", false);

            if (!PlayerData.instance.GetBool(fsm.FsmVariables.StringVariables.First(v => v.Name == "Station Opened Bool").Value))
            {
                fsm.FsmVariables.IntVariables.First(v => v.Name == "Station Position Number").Value = 0;
                currentLocationCheck.RemoveActionsOfType<IntCompare>();

                checkResult.AddFirstAction(new Lambda(() =>
                {
                    if (cancelTravel.Value)
                    {
                        fsm.SendEvent("CANCEL");
                    }
                }));
                checkResult.AddTransition("CANCEL", "HUD Return");
            }

            fsm.GetState("HUD Return").AddFirstAction(new SetBoolValue
            {
                boolVariable = cancelTravel,
                boolValue = false
            });
        }

        private void EditUIList(PlayMakerFSM fsm)
        {
            fsm.GetState("Selection Made Cancel").AddFirstAction(new Lambda(() =>
            {
                GameObject.Find("Stag").LocateMyFSM("Stag Control").FsmVariables
                    .BoolVariables.First(v => v.Name == "Cancel Travel").Value = true;
            }));
        }

        private void EditStationBell(PlayMakerFSM fsm)
        {
            FsmState init = fsm.GetState("Init");
            init.RemoveActionsOfType<PlayerDataBoolTest>();
            init.AddTransition("FINISHED", "Opened");
        }
    }
}
