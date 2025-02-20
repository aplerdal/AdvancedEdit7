using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AdvancedEdit.UI.Settings;
public class SettingsManager{
    Dictionary<string, Setting> Settings = new();
    public SettingsManager() {

    }
    public bool Load(string path){
        try {
            XElement document = XElement.Load(path);
            var settings = document.Descendants("Settings");
            Settings = settings.Elements("Setting").ToDictionary(x=>x.Attribute("id")!.Value, x=>new Setting(x));
            return true;
        } catch {
            return false;
        }
    }
    public void Save(string path){
        XElement document = new XElement("Settings");
        foreach (var pair in Settings){
            document.Add(new XElement("Setting",
                new XAttribute("id", pair.Key),
                new XAttribute("type", Setting.TypeToString(pair.Value.Type)),
                new XAttribute("value", pair.Value.Value)
            ));
        }
        document.Save(path);
    }
}