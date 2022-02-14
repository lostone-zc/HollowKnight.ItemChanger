namespace ItemChanger.Items
{
    /// <summary>
    /// Item which directly adds geo to the inventory.
    /// </summary>
    public class AddGeoItem : AbstractItem
    {
        public int amount;

        public override void GiveImmediate(GiveInfo info)
        {
            HeroController.instance.AddGeo(amount);
        }
    }
}
