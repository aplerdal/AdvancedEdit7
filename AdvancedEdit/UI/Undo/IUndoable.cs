namespace AdvancedEdit.UI.Undo;

public interface IUndoable
{
    /// <summary>
    /// Execute the action
    /// </summary>
    public void Do();
    /// <summary>
    /// Reverse the action
    /// </summary>
    public void Undo();
}