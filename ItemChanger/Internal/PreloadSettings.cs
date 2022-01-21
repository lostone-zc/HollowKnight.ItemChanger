using System.Runtime.CompilerServices;
using ItemChanger.Extensions;
using Modding;
using Newtonsoft.Json;
using MenuEntry = Modding.IMenuMod.MenuEntry;

namespace ItemChanger.Internal
{
    public enum PreloadLevel
    {
        Full,
        Reduced,
        None,
    }

    public class PreloadSettings
    {
        [JsonProperty] private readonly Dictionary<string, PreloadLevel> PreloadLevels;
        [JsonIgnore] public PreloadLevel PreloadGeoRocks { get => Get(); set => Set(value); }
        [JsonIgnore] public PreloadLevel PreloadSoulTotems { get => Get(); set => Set(value); }
        [JsonIgnore] public PreloadLevel PreloadGrub { get => Get(); set => Set(value); }
        [JsonIgnore] public PreloadLevel PreloadMimic { get => Get(); set => Set(value); }
        //[JsonIgnore] public PreloadLevel PreloadBluggsac { get => Get(); set => Set(value); }

        public PreloadSettings()
        {
            PreloadLevels = new()
            {
                { nameof(PreloadGeoRocks), PreloadLevel.Full },
                { nameof(PreloadSoulTotems), PreloadLevel.Full },
                { nameof(PreloadGrub), PreloadLevel.Full },
                { nameof(PreloadMimic), PreloadLevel.Full },
                //{ nameof(PreloadBluggsac), PreloadLevel.Full },
            };
        }
        [JsonConstructor] public PreloadSettings(Dictionary<string, PreloadLevel> PreloadLevels) => this.PreloadLevels = PreloadLevels;

        private PreloadLevel Get([CallerMemberName] string caller = null)
        {
            if (_preloadOverrides[caller] is PreloadLevel pl) return pl;
            else return PreloadLevels[caller];
        }

        private void Set(PreloadLevel value, [CallerMemberName] string caller = null) => PreloadLevels[caller] = value;


        private static Dictionary<string, PreloadLevel?> _preloadOverrides = new()
        {
            { nameof(PreloadGeoRocks), null },
            { nameof(PreloadSoulTotems), null },
            { nameof(PreloadGrub), null },
            { nameof(PreloadMimic), null },
            //{ nameof(PreloadBluggsac), null },
        };

        public static void AddPreloadOverride(string propertyName, PreloadLevel value)
        {
            if (propertyName == null || !_preloadOverrides.TryGetValue(propertyName, out PreloadLevel? pl)) throw new ArgumentException("Invalid preload override name.");
            if (pl.HasValue && pl.Value != value)
            {
                LogWarn($"Incompatible overrides for preload setting {propertyName}. Replacing old override {pl} with new override {value}");
            }

            _preloadOverrides[propertyName] = value;
        }

        public static void RemovePreloadOverride(string propertyName)
        {
            if (propertyName == null || !_preloadOverrides.ContainsKey(propertyName)) throw new ArgumentException("Invalid preload override name.");
            _preloadOverrides[propertyName] = null;
        }

        internal MenuEntry[] GetMenuEntries()
        {
            PreloadLevel[] values = Enum.GetValues(typeof(PreloadLevel)).Cast<PreloadLevel>().ToArray();
            string[] names = values.Select((PreloadLevel p) => p.ToString().FromCamelCase()).ToArray();
            return ((IEnumerable<string>)PreloadLevels.Keys).Select((Func<string, MenuEntry>)((string key) => new MenuEntry(Localize(key), names, string.Empty, (Action<int>)delegate (int i)
            {
                PreloadLevels[key] = values[i];
            }, (Func<int>)(() => Array.IndexOf(values, PreloadLevels[key]))))).ToArray();
        }

        public string Localize(string orig)
        {
            return orig switch
            {
                "PreloadGeoRocks" => "钱堆预加载",
                "PreloadSoulTotems" => "灵魂图腾预加载",
                "PreloadGrub" => "幼虫预加载",
                "PreloadMimic" => "假虫预加载",
                _ => orig,
            };
        }
    }
}
