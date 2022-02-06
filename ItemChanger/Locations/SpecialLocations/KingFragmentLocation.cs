namespace ItemChanger.Locations.SpecialLocations
{
    /// <summary>
    /// ObjectLocation which supports a hint at the Kingsmould corpse in Abyss_05 and triggers a scene change to Abyss_05 when its items are obtained.
    /// </summary>
    public class KingFragmentLocation : ObjectLocation, ILocalHintLocation
    {
        public bool HintActive { get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();
            Events.AddLanguageEdit(new("Lore Tablets", "DUSK_KNIGHT_CORPSE"), OnLanguageGet);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            Events.RemoveLanguageEdit(new("Lore Tablets", "DUSK_KNIGHT_CORPSE"), OnLanguageGet);
        }

        private void OnLanguageGet(ref string value)
        {
            if (this.GetItemHintActive())
            {
                if (!Placement.AllObtained())
                {
                    string text = Placement.GetUIName();
                    value = "身穿白色盔甲的尸体。 你清晰地看到它正拿着 "
                                + text + " ，" +
                                "但出于一些未知的原因，你知道你必须" +
                                "要去穿过那些尖刺和电剧，要去获得它。";
                    Placement.OnPreview(text);
                }
                else
                {
                    value = "身穿白色盔甲的尸体。 你已经获得它的物品。";
                }
            }
        }
    }
}
