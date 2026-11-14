using UnityEngine;

[RequireComponent(typeof(AttackPickup))]
public class AttackItemReference : MonoBehaviour
{
    [SerializeField] private AttackPickup worldPrefab;

    public AttackPickup WorldPrefab => worldPrefab;
}