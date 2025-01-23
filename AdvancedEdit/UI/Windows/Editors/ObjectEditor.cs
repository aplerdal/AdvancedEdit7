namespace AdvancedEdit.UI.Windows.Editors;

public class ObjectEditor(TilemapWindow window) : TrackEditor(window)
{
    public override string Name => "Object Editor";
    public override string Id => "objeditor";
    public override void Update(bool hasFocus)
    {
        throw new System.NotImplementedException();
    }

    public override void DrawInspector()
    {
        throw new System.NotImplementedException();
    }
}