
using System;
using AdvancedEdit.Serialization;
using ImGuiNET;

namespace AdvancedEdit.UI.Windows;

public class TrackSelector : UiWindow {
    public override string Name => "Tracks";
    public override ImGuiWindowFlags Flags => ImGuiWindowFlags.None;
    public override string WindowId => "selector";

    public static readonly int[] TrackMapping = {
         0, 1, 2, 3, // SNES    Mushroom
         4, 5, 6, 7, //         Flower
         8, 9,10,11, //         Lightning
        12,13,14,15, //         Star
        16,17,18,19, //         Special
        20,21,22,23, //         Battle
        24,25,29,27, // MKSC    Mushroom
        32,37,38,31, //         Flower 
        28,40,33,26, //         Lightning
        36,34,30,35, //         Star
        43,41,42,39, //         Special
        44,45,46,47, //         Battle
    };

    public static readonly string[] PagesList = {
        "SNES Tracks",
        "MKSC Tracks"
    };

    public static readonly string[] CupsList = {
    // SNES
        "Mushroom Cup",
        "Flower Cup",
        "Lightning Cup",
        "Star Cup",
        "Special Cup",
        "Battle",
    // MKSC
        "Mushroom Cup",
        "Flower Cup",
        "Lightning Cup",
        "Star Cup",
        "Special Cup",
        "Battle",
    };

    public static readonly string[] TracksList = {
    // SNES Mushroom
        "Mario Circuit 1",
        "Donut Plains 1",
        "Ghost Valley 1",
        "Bowser Castle 1",
    // SNES Flower
        "Mario Circuit 2",
        "Choco Island 1",
        "Ghost Valley 2",
        "Donut Plains 2",
    // SNES Lightning
        "Bowser Castle 2",
        "Mario Circuit 3",
        "Koopa Beach 1",
        "Choco Island 2",
    // SNES Star
        "Vanilla Lake 1",
        "Bowser Castle 3",
        "Mario Circuit 4",
        "Donut Plains 3",
    // SNES Special
        "Koopa Beach 2",
        "Ghost Valley 3",
        "Vanilla Lake 2",
        "Rainbow Road",
    // SNES Battle
        "Battle Course 1",
        "Battle Course 2",
        "Battle Course 3",
        "Battle Course 4",
    // Mushroom Cup
        "Peach Circuit",
        "Shy Guy Beach",
        "Riverside Park",
        "Bowser Castle 1",
    // Flower
        "Mario Circuit",
        "Boo Lake",
        "Cheese Land",
        "Bowser Castle 2",
    // Lightning
        "Luigi Circuit",
        "Sky Garden",
        "Cheep Cheep Island",
        "Sunset Wilds",
    // Star
        "Snow Land",
        "Ribbon Road",
        "Yoshi Desert",
        "Bowser Castle 3",
    // Special
        "Lakeside Park",
        "Broken Pier",
        "Bowser Castle 4",
        "Rainbow Road",
    // Battle
        "Battle Course 1",
        "Battle Course 2",
        "Battle Course 3",
        "Battle Course 4",
    };

    public static string GetTrackName(int id)
    {
        return TracksList[Array.FindIndex(TrackSelector.TrackMapping, x => x == id)];
    }
    public override void Draw()
    {
        if (AdvancedEdit.Instance.TrackManager is null)
        {
            ImGui.Text("No ROM Loaded");
            return;
        }
        if (TrackManager.Tracks is null) {
            ImGui.Text("Failed to load ROM");
            return;
        }
        for (int page = 0; page < PagesList.Length; page++){
            ImGui.PushID(page);
            if (ImGui.TreeNode(PagesList[page])){
                for (int cup = 0; cup < CupsList.Length/PagesList.Length; cup++){
                    ImGui.PushID(cup);
                    if (ImGui.TreeNode(CupsList[cup+page*(CupsList.Length/PagesList.Length)])){
                        for (int track = 0; track < TracksList.Length/CupsList.Length; track++){
                            ImGui.PushID(track);
                            if (ImGui.Button(TracksList[track+(cup+page*(CupsList.Length/PagesList.Length))*TracksList.Length/CupsList.Length]))
                            {
                                var t = TrackManager.Tracks[
                                    TrackMapping[track+(cup+page*(CupsList.Length/PagesList.Length))*TracksList.Length/CupsList.Length]
                                ];
                                AdvancedEdit.Instance.UiManager.AddTrack(t);
                            }
                            ImGui.PopID();
                        }
                        ImGui.TreePop();
                    }
                    ImGui.PopID();
                }
                ImGui.TreePop();
            }
            ImGui.PopID();
        }
    }
}