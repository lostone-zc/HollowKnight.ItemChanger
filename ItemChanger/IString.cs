﻿using ItemChanger.Extensions;
using Newtonsoft.Json;

namespace ItemChanger
{
    public interface IString
    {
        string Value { get; }
        IString Clone();
    }

    public class LanguageString : IString
    {
        public string sheet;
        public string key;

        public LanguageString(string sheet, string key)
        {
            this.sheet = sheet;
            this.key = key;
        }

        [JsonIgnore]
        public string Value => Language.Language.Get(key, sheet)?.Replace("<br>", "\n");
        public IString Clone() => (IString)MemberwiseClone();
    }

    public class BoxedString : IString
    {
        public string Value { get; set; }

        public BoxedString(string Value) => this.Value = Value;
        
        public IString Clone() => (IString)MemberwiseClone();
    }

    public class PaywallString : IString
    {
        public string key;
        public string sheet;

        public PaywallString(string sheet, string key)
        {
            this.sheet = sheet;
            this.key = key;
        }

        [JsonIgnore]
        public string Value => Language.Language.Get(key, sheet)?.Replace("<br>", "\n").CapLength(125) 
            + "...\n\n你已到达每月阅读上限。若想继续阅读，请一键三连，送主播一个648!";
        public IString Clone() => (IString)MemberwiseClone();
    }

}
