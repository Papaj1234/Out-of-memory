using UnityEngine;

using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 3f;

    public float MoveSpeed => moveSpeed;

    public Transform Player { get; private set; }

    private EnemyStateMachine stateMachine;

    private EnemyChaseState chaseState;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            Player = playerObject.transform;
        }

        stateMachine = new EnemyStateMachine();

        chaseState = new EnemyChaseState(this);

        stateMachine.ChangeState(chaseState);
    }

    private void Update()
    {
        stateMachine.Update();
    }
}
