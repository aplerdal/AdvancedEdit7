using System;
using ImGuiNET;
using System.Xml.Linq;
using System.IO;

namespace AdvancedEdit.UI.Settings;
public enum SettingType {
    Int,
    Bool,
    Key,
    String,
}
public struct Setting {
    public string Value {get; set;}
    public SettingType Type {get; set;}

    public Setting(int value){
        Value = value.ToString();
        Type = SettingType.Int;
    }
    public Setting(bool value){
        Value = value?"true":"false";
        Type = SettingType.Bool;
    }
    public Setting(ImGuiKey value){
        Value = ((int)value).ToString();
        Type = SettingType.Key;
    }
    public Setting(string value){
        Value = value;
        Type = SettingType.Key;
    }
    public Setting(XElement element){
        var type = element.Attribute("type");
        var value = element.Attribute("value");
        if (type is not null && value is not null){
            Value = value.Value;
            Type = StringToType(type.Value);
        } else throw new InvalidDataException("Error reading settings: Setting Attributes null");
    }
    public static SettingType StringToType(string type){
        return type switch {
            "int" => SettingType.Int,
            "bool" => SettingType.Bool,
            "key" => SettingType.Key,
            _ => SettingType.String,
        };
    }
    public static string TypeToString(SettingType type) {
        return type switch {
            SettingType.Int => "int",
            SettingType.Bool => "bool",
            SettingType.Key => "key",
            _ => "string",
        };
    }
}