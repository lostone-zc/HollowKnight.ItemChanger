using Modding;
using MenuEntry = Modding.IMenuMod.MenuEntry;

namespace ItemChanger.Internal.Menu
{
    public static class ItemChangerMenu
    {
        public readonly record struct SubpageDef(string Title, string Description, MenuEntry[] Entries);
        public static readonly List<SubpageDef> Subpages = new()
        {
            new() 
            {
                Title = "预加载设置", 
                Description = "设置在重启游戏后生效。", 
                Entries = ItemChangerMod.GS.PreloadSettings.GetMenuEntries(),
            },
            new() 
            {
                Title = "位置设置", 
                Description = "设置不会影响已有存档。", 
                Entries = ItemChangerMod.GS.LocationSettings.GetMenuEntries(),
            },
        };


        public static MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            ModMenuScreenBuilder builder = new("ItemChangerMod", modListMenu);
            foreach (SubpageDef def in Subpages) builder.AddSubpage(def.Title, def.Description, def.Entries);
            return builder.CreateMenuScreen();
        }
    }
}
