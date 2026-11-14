using UnityEngine;

public abstract class UltimateBase : EquippedActionBase
{
    [SerializeField, Min(0f)] private float cooldown = 15f;
    public sealed override EquipmentKind Kind => EquipmentKind.Ultimate;
    protected sealed override float Cooldown => cooldown;
}
