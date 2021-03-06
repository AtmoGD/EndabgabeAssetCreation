using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] PlayerController player = null;
    [SerializeField] CharacterController controller = null;
    [SerializeField] int velocityHash = 0;
    [SerializeField] int attackHash = 0;
    [SerializeField] int dieHash = 0;
    [SerializeField] int attackCancleHash = 0;
    [SerializeField] new Collider collider = null;
    [SerializeField] public GameObject portalPrefab = null;
    [SerializeField] int pointsWoth = 20;
    [SerializeField] Animator anim;
    [SerializeField] Transform attackPosition = null;
    [SerializeField] float attackRadius = 1f;
    [SerializeField] float stopDistance = 2f;
    [SerializeField] float movementThreshold = 0.1f;
    [SerializeField] float smoothSpeedTime;
    [SerializeField] float turnSmoothTime;
    [SerializeField] float speed = 10f;
    [SerializeField] float attackTime = 1f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float attackReset = 0.5f;
    [SerializeField] float damage = 10f;
    bool isActive = false;
    Vector3 lastMoveDir;
    float turnSmoothVelocity;
    bool isAttacking = false;
    float actualAttackCastTime = 0f;
    float actualCooldown = 0f;
    bool isResetting = false;
    float actualResetCooldown = 0f;

    void Start()
    {
        velocityHash = Animator.StringToHash("velocity");
        attackHash = Animator.StringToHash("attack");
        dieHash = Animator.StringToHash("die");
        attackCancleHash = Animator.StringToHash("attackCanceled");
    }

    void FixedUpdate()
    {
        if (!isActive) return;
        if (isAttacking)
        {
            actualAttackCastTime += Time.deltaTime;
            if (actualAttackCastTime >= attackTime)
                Attack();
            return;
        }

        if (isResetting)
        {
            actualResetCooldown -= Time.deltaTime;

            if (actualResetCooldown <= 0f)
            {
                ResetAttack();

                return;
            }

        }
        controller.Move(Physics.gravity * Time.deltaTime);

        // if(GameManager.instance.gamePaused)  {
        //     anim.SetFloat("velocity", 0f);
        //     return;
        // }

        Vector3 dir = player.transform.position - transform.position;

        if (dir.magnitude > stopDistance)
        {
            Move((player.transform.position - transform.position).normalized);
        }
        else if (actualCooldown <= 0f)
        {
            StartAttack();
        }

        actualCooldown -= Time.deltaTime;
        actualResetCooldown -= Time.deltaTime;

    }

    public void TakePlayer(PlayerController _player)
    {
        player = _player;
        isActive = true;
    }

    public void Move(Vector3 _dir, bool _animate = true)
    {
        if (GameManager.instance.gamePaused) _dir = Vector3.zero;
        
        Vector3 desiredMoveDir = _dir;
        if (_dir.magnitude >= movementThreshold)
        {
            float targetAngle = Mathf.Atan2(_dir.x, _dir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            desiredMoveDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        }

        desiredMoveDir = Vector3.Lerp(lastMoveDir, desiredMoveDir, smoothSpeedTime);
        controller.Move(transform.forward * desiredMoveDir.magnitude * speed * Time.deltaTime);
        if (_animate)
            anim.SetFloat(velocityHash, desiredMoveDir.magnitude * speed);

        lastMoveDir = desiredMoveDir;
    }

    public void StartAttack()
    {
        anim.SetTrigger(attackHash);
        anim.SetFloat(velocityHash, 0f);
        isAttacking = true;
    }

    public void Attack()
    {
        isAttacking = false;
        actualAttackCastTime = 0f;
        actualCooldown = attackCooldown;

        // if(Physics.SphereCast(attackPosition.position, attackRadius, Vector3.one, int))

        RaycastHit[] hit = Physics.SphereCastAll(attackPosition.position, attackRadius, Vector3.one);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider.gameObject == player.gameObject)
            {
                player.GetHit();
                return;
            }
        }
        // if (Physics.SphereCast(attackPosition.position, attackRadius, Vector3.zero, out RaycastHit hit))
        // {
        //     if (hit.collider.gameObject == player.gameObject)
        //     {
        //         player.GetHit();
        //     }
        // }

        isResetting = true;
        actualResetCooldown = attackReset;
    }

    public void Die()
    {
        player.AddScore(pointsWoth);
        anim.SetTrigger(dieHash);
        isActive = false;
        gameObject.tag = "Dead";
        GameManager.instance.EnemyDied();
        Destroy(controller);
        Destroy(collider);
        Destroy(this);
    }

    public void ResetAttack()
    {
        isResetting = false;
        anim.SetTrigger(attackCancleHash);
        actualCooldown = attackCooldown;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(attackPosition.position, attackRadius);
    }
}
