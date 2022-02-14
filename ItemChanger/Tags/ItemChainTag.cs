namespace ItemChanger.Tags
{
    /// <summary>
    /// Tag which triggers a recursive search through the AbstractItem.ModifyItem hook.
    /// <br />Recursion is by looking up the predecessor and successor items in Finder, and basing a search at their ItemChainTags.
    /// <br />Selected item is first nonredundant item in the sequence, or null (handled by AbstractItem) if all items are redundant.
    /// </summary>
    public class ItemChainTag : Tag, IItemModifierTag
    {
        public string predecessor;
        public string successor;

        public override void Load(object parent)
        {
            AbstractItem item = (AbstractItem)parent;
            item.ModifyItem += ModifyItem;
        }

        public override void Unload(object parent)
        {
            AbstractItem item = (AbstractItem)parent;
            item.ModifyItem -= ModifyItem;
        }


        protected virtual AbstractItem GetItem(string name)
        {
            return Finder.GetItem(name);
        }

        public void ModifyItem(GiveEventArgs args)
        {
            if (args.Item.Redundant())
            {
                while (args.Item?.GetTag<ItemChainTag>()?.successor is string succ && !string.IsNullOrEmpty(succ))
                {
                    args.Item = GetItem(succ);
                    if (!args.Item.Redundant())
                    {
                        return;
                    }
                }

                args.Item = null;
                return;
            }
            else
            {
                while (args.Item?.GetTag<ItemChainTag>()?.predecessor is string pred && !string.IsNullOrEmpty(pred))
                {
                    AbstractItem item = GetItem(pred);
                    if (item.Redundant())
                    {
                        return;
                    }
                    else args.Item = item;
                }
                return;
            }
        }
    }
}
