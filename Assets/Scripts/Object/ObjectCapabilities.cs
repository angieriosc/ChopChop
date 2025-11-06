using System;

/// <summary>
/// Enum de capacidades posibles para los objetos interactuables.
/// Se pueden combinar usando Flags.
/// </summary>
[Flags]
public enum ObjectCapabilities
{
    None        = 0,
    Grabbable   = 1 << 0,
    Droppable   = 1 << 1,
    Cuttable    = 1 << 2,
    Pourable    = 1 << 3,
    Heatable    = 1 << 4,
    Mixable     = 1 << 5,
    Bakeable    = 1 << 6,
    Liquefiable = 1 << 7,
    Deliverable = 1 << 8,
    Buyable     = 1 << 9,
    PourableAllOnce = 1 << 10,
    Toppingable  = 1 << 11
}
