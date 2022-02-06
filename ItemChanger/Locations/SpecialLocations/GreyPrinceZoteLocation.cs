namespace ItemChanger.Locations.SpecialLocations
{
    /// <summary>
    /// Boss Essence Location which supports a hint from reading Bretta's Diary.
    /// </summary>
    public class GreyPrinceZoteLocation : DreamBossLocation, ILocalHintLocation
    {
        public bool HintActive { get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();
            Events.AddLanguageEdit(new("Minor NPC", "BRETTA_DIARY_1"), AddHintToDiary);
            Events.AddLanguageEdit(new("Minor NPC", "BRETTA_DIARY_2"), AddHintToDiary);
            Events.AddLanguageEdit(new("Minor NPC", "BRETTA_DIARY_3"), AddHintToDiary);
            Events.AddLanguageEdit(new("Minor NPC", "BRETTA_DIARY_4"), AddHintToDiary);
            // Not critical, as this one shows only if Bretta has left
            Events.AddLanguageEdit(new("CP2", "BRETTA_DIARY_LEAVE"), AddHintToDiary);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            Events.RemoveLanguageEdit(new("Minor NPC", "BRETTA_DIARY_1"), AddHintToDiary);
            Events.RemoveLanguageEdit(new("Minor NPC", "BRETTA_DIARY_2"), AddHintToDiary);
            Events.RemoveLanguageEdit(new("Minor NPC", "BRETTA_DIARY_3"), AddHintToDiary);
            Events.RemoveLanguageEdit(new("Minor NPC", "BRETTA_DIARY_4"), AddHintToDiary);
            Events.RemoveLanguageEdit(new("CP2", "BRETTA_DIARY_LEAVE"), AddHintToDiary);
        }

        private void AddHintToDiary(ref string value)
        {
            if (this.GetItemHintActive())
            {
                if (Placement.AllObtained()) return;

                string item = Placement.GetUIName(40);
                if (string.IsNullOrEmpty(item)) return;

                value += $"<page>少女的回礼<br>少女想送点礼物给她的救主，她想到她房间下面的珍贵的 "
                    + item + " 。虽然少女与这件物品难舍难分，但是她也没有比这更好的礼物了。";
                Placement.OnPreview(item);
            }
        }
    }
}
