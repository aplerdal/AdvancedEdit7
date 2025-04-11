using System.Numerics;

namespace AdvancedEdit.UI.Themes;

public class LightTheme : Theme
{
    public override Vector4 Text => new Vector4(0, 0, 0, 1.00f);

    public override Vector4 WindowBg => new(0.91f, 0.91f, 0.91f, 0.94f);

    public override Vector4 ChildBg => new(0, 0, 0, 0.00f);

    public override Vector4 Border => new(0.80f, 0.80f, 0.80f, 0.50f);

    public override Vector4 PopupBg => new(1, 1, 1, 0.94f);

    public override Vector4 FrameBg => new(1, 1, 1, 1);

    public override Vector4 FrameBgHovered => new(0.4f, 0.4f, 0.4f, 0.67f);

    public override Vector4 FrameBgActive => new(0.5f, 0.5f, 0.5f, 0.67f);

    public override Vector4 TitleBg => new(0.85f, 0.85f, 0.85f, 1.000f);

    public override Vector4 TitleBgActive => new(0.84f, 0.84f, 0.84f, 1.00f);

    public override Vector4 CheckMark => new(0.37f, 0.53f, 0.71f, 1.00f);

    public override Vector4 ButtonActive => new(0.34f, 0.54f, 1, 1.00f);

    public override Vector4 Button => new(0.75f, 0.75f, 0.75f, 1.00f);

    public override Vector4 Header => new(0.7f, 0.7f, 0.7f, 0.31f);

    public override Vector4 HeaderHovered => new(0.7f, 0.7f, 0.7f, 0.80f);

    public override Vector4 HeaderActive => new(0.7f, 0.7f, 0.7f, 1.00f);

    public override Vector4 SeparatorHovered => new(0.82f, 0.82f, 0.82f, 0.78f);

    public override Vector4 SeparatorActive => new(0.53f, 0.53f, 0.53f, 1.00f);

    public override Vector4 Separator => new(0.85f, 0.85f, 0.85f, 1.00f);

    public override Vector4 Tab => new(1, 1, 1, 0.86f);

    public override Vector4 TabHovered => new(0.9f, 0.9f, 0.9f, 0.80f);

    public override Vector4 TabActive => new(0.9f, 0.9f, 0.9f, 1.00f);

    public override Vector4 TabDimmed => new(0.9f, 0.9f, 0.9f, 0.98f);

    public override Vector4 TabDimmedSelected => new(0.9f, 0.9f, 0.9f, 1.00f);

    public override Vector4 DockingPreview => new(0.6f, 0.6f, 0.6f, 0.70f);

    public override Vector4 DockingEmptyBg => new(0.65f, 0.65f, 0.65f, 0.70f);

    public override Vector4 TextSelectedBg => new(0.24f, 0.45f, 0.68f, 0.35f);

    public override Vector4 NavWindowingHighlight => new(0.4f, 0.4f, 0.4f, 0);

    public override Vector4 Error => new(1f, 0.3f, 0.3f, 1.0f);

    public override Vector4 Ok => new(0, 1, 0, 1);

    public override Vector4 Warning => new(1, 1, 0.3f, 1.0f);
}