using UnityEngine;

public abstract class DashBase : EquippedActionBase
{
    [SerializeField, Min(0f)] private float cooldown = 2f;
    public sealed override EquipmentKind Kind => EquipmentKind.Dash;
    protected sealed override float Cooldown => cooldown;
}
