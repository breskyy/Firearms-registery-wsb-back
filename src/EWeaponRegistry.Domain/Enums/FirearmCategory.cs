namespace EWeaponRegistry.Domain.Enums;

/// <summary>
/// Firearm categories according to simplified classification.
/// Sport permits allow A, B.
/// Collection permits allow A, B, C.
/// Protection permits allow B.
/// Hunting permits allow C.
/// </summary>
public enum FirearmCategory
{
    A = 0,  // Sport/Competition firearms
    B = 1,  // Standard firearms (handguns, semi-automatic)
    C = 2   // Hunting firearms (rifles, shotguns)
}
