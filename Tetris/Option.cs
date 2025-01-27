namespace Tetris;

public class Option
{
    public string Name { get; }
    public Action Selected { get; }

    public Option(string name, Action selected)
    {
        Name = name;
        Selected = selected;
    }

    // Override ToString to display the Name
    public override string ToString()
    {
        return Name;
    }
}