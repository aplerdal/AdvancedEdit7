using System.Collections;
using System.Collections.Generic;

namespace AdvancedEdit.UI.Undo;

public class UndoManager
{
    private Stack<IUndoable> _undoStack = new();
    private Stack<IUndoable> _redoStack = new();

    public void Do(IUndoable action)
    {
        _undoStack.Push(action);
        action.Do();
        _redoStack.Clear();
    }

    public void Undo()
    {
        if (_undoStack.Count<=0) return;
        _undoStack.Peek().Undo();
        _redoStack.Push(_undoStack.Pop());
    }

    public void Redo()
    {
        if (_redoStack.Count <= 0) return;
        _redoStack.Peek().Do();
        _undoStack.Push(_redoStack.Pop());
    }
}