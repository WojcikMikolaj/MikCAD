namespace MikCAD;

public enum SimulatorErrorCode
{
    None = -1,
    WrongFileName = 0,
    WrongCutterType = 1,
    WrongCutterSize = 2,
    UnsupportedCommand = 3,
    MissingInstructions = 4,
    FlatHeadMoveDownWhileMilling = 5,
    MoveBelowSafeLimit = 5,
}