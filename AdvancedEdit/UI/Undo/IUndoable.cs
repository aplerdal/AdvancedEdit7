namespace AdvancedEdit.UI.Undo;

public interface IUndoable
{
    public void Do();
    public void Undo();
}