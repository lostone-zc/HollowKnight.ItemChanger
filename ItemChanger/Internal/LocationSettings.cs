using MenuEntry = Modding.IMenuMod.MenuEntry;

namespace ItemChanger.Internal
{
    public class LocationSettings
    {
        public readonly record struct LocationSheetSetting(string Name, int SheetIndex);
        public static readonly LocationSheetSetting[] Settings = new LocationSheetSetting[]
        {
            new("跳过NPC对话", (int)Finder.FinderLocationSheets.AvoidNPCItemDialogue),
            new("用白点代替臭蛋口袋", (int)Finder.FinderLocationSheets.AvoidBluggsacs),
            new("保持碑文精灵图像", (int)Finder.FinderLocationSheets.RetainTabletsOnReplace),
        };

        public List<int> extraSheets = new();

        public void AddSheet(int id)
        {
            if (extraSheets.Contains(id)) return;
            extraSheets.Add(id);
        }

        public void RemoveSheet(int id)
        {
            extraSheets.Remove(id);
        }

        public void ToggleSheet(bool value, int id)
        {
            if (value) AddSheet(id);
            else RemoveSheet(id);
        }

        public bool HasSheet(int id)
        {
            return extraSheets.Contains(id);
        }

        public MenuEntry[] GetMenuEntries()
        {
            string[] bools = new string[] { bool.FalseString, bool.TrueString };
            return Settings.Select(s => new MenuEntry(
                name: s.Name,
                values: bools,
                description: string.Empty,
                saver: j => ToggleSheet(j == 1, s.SheetIndex),
                loader: () => HasSheet(s.SheetIndex) ? 1 : 0)
            ).ToArray();
        }
    }
}
