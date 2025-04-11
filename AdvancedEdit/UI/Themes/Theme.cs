using System.Numerics;

namespace AdvancedEdit.UI.Themes;

public abstract class Theme
{
    public abstract Vector4 Text { get;}
    public abstract Vector4 WindowBg { get;}
    public abstract Vector4 ChildBg { get;}
    public abstract Vector4 Border { get;}
    public abstract Vector4 PopupBg { get;}
    public abstract Vector4 FrameBg { get;}
    public abstract Vector4 FrameBgHovered { get;}
    public abstract Vector4 FrameBgActive { get;}
    public abstract Vector4 TitleBg { get; }
    public abstract Vector4 TitleBgActive { get;}
    public abstract Vector4 CheckMark { get;}
    public abstract Vector4 ButtonActive { get; }
    public abstract Vector4 Button { get; }
    public abstract Vector4 Header { get; }
    public abstract Vector4 HeaderHovered { get;  }
    public abstract Vector4 HeaderActive { get;  }
    public abstract Vector4 SeparatorHovered { get;  }
    public abstract Vector4 SeparatorActive { get; }
    public abstract Vector4 Separator { get;  }
    public abstract Vector4 Tab { get;  }
    public abstract Vector4 TabHovered { get;  }
    public abstract Vector4 TabActive { get; }
    public abstract Vector4 TabDimmed { get; }
    public abstract Vector4 TabDimmedSelected { get;  }
    public abstract Vector4 DockingPreview { get;  }
    public abstract Vector4 DockingEmptyBg { get; }
    public abstract Vector4 TextSelectedBg { get;  }
    public abstract Vector4 NavWindowingHighlight { get; }
    public abstract Vector4 Error { get;  }
    public abstract Vector4 Ok { get;  }
    public abstract Vector4 Warning { get;  }
}