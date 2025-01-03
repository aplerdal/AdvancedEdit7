namespace AdvancedEdit.UI;

public abstract class UiWindow
{
    public abstract string Name { get; }
    public bool IsOpen = true;
    public int Id { get; set; }

    public abstract void Draw(AdvancedEdit ae);
    public abstract void Input(AdvancedEdit ae);
}