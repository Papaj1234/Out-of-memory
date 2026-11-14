using UnityEngine;

public abstract class AbilityBase : EquippedActionBase
{
    [SerializeField, Min(0f)] private float cooldown = 8f;
    public sealed override EquipmentKind Kind => EquipmentKind.Ability;
    protected sealed override float Cooldown => cooldown;
}
