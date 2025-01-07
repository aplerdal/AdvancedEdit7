using System.Collections;
using System.Collections.Generic;

namespace AdvancedEdit.UI.Undo;

public class UndoManager
{
    private Stack<IUndoable> _undoStack = new();
    private Stack<IUndoable> _redoStack = new();
    /// <summary>
    /// Run and add the given action to the undo stack
    /// </summary>
    /// <param name="action">The action to be run</param>
    public void Do(IUndoable action)
    {
        _undoStack.Push(action);
        action.Do();
        _redoStack.Clear();
    }
    /// <summary>
    /// Undo the previous action
    /// </summary>
    public void Undo()
    {
        if (_undoStack.Count<=0) return;
        _undoStack.Peek().Undo();
        _redoStack.Push(_undoStack.Pop());
    }
    /// <summary>
    /// Redo the previous undone action
    /// </summary>
    public void Redo()
    {
        if (_redoStack.Count <= 0) return;
        _redoStack.Peek().Do();
        _undoStack.Push(_redoStack.Pop());
    }
}