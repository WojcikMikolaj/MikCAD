namespace MikCAD.Objects;

public class SelectionBox
{
    public bool Draw { get; set; } = false;
    public float X1 { get; set; }
    public float X2 { get; set; }
    public float Y1 { get; set; }
    public float Y2 { get; set; }
}