using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

    // Legacy component kept only so old files still compile. Equipment no longer uses it.
[RequireComponent(typeof(AttackPickup))]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "AttackItemReference")]
public class AttackItemReference : MonoBehaviour
{
    [SerializeField] private AttackPickup worldPrefab;
    public AttackPickup WorldPrefab => worldPrefab;
}
