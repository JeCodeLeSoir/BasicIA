using UnityEngine;

public class IAAgent : MonoBehaviour
{
    [System.Serializable]
    public class Spell
    {
        public float range;
        public float damage;
        public float cooldown;
        public float time;
    }

    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    [SerializeField] Transform target;
    [SerializeField] float triggerDistance = 10f;
    [SerializeField] float minStopChaseDistance = 1f;
    [SerializeField] float stopChaseDistance = 15f;
    [SerializeField] float distanceAttackMin;
    [SerializeField] float targetDistance;

    [SerializeField] float speed = 5f;
    [SerializeField] State state = State.Idle;

    [SerializeField] Spell[] spells;

    private bool MoveToAttack;
    private Spell spellForMove;

    void Update()
    {
        targetDistance = Vector3.Distance(transform.position, target.position);

        TargetTrigger();

        switch (state)
        {
            case State.Idle:
                state = State.Patrol;
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }

        ResetCooldowns();
    }

    private void ResetCooldowns()
    {
        for (int i = 0; i < spells.Length; i++)
        {
            if (spells[i].time > 0f)
                spells[i].time -= Time.deltaTime;
        }

        if (patrolTimer > 0f)
            patrolTimer -= Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minStopChaseDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanceAttackMin);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }
     
    private void Start()
    {
        spells = new Spell[3];
        
        spells[0] = new Spell() { range = 1f, damage = 10f, cooldown = 2f, time = 0f };
        spells[1] = new Spell() { range = 3f, damage = 20f, cooldown = 5f, time = 0f };
        spells[2] = new Spell() { range = 4f, damage = 30f, cooldown = 10f, time = 0f };

        float min = 0;
        float max = 0;

        for (int i = 0; i < spells.Length; i++)
        {
            if (spells[i].range > max)
                max = spells[i].range;
            if (spells[i].range < min || min == 0)
                min = spells[i].range;
        }

        distanceAttackMin = Mathf.Max(min, max);
    }

    private void Attack()
    {
        void runAttack(Spell spell)
        {
            MoveToAttack = false;
            spell.time = spell.cooldown;
            Debug.Log("Attack with spell of range: " + spell.range + " and damage: " + spell.damage);
            state = State.Chase;
        }

        if (MoveToAttack)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                      target.position, speed * Time.deltaTime);
            if (targetDistance <= spellForMove.range)
            {
                runAttack(spellForMove);
            }
            return;
        }

        for (int i = 0; i < spells.Length; i++)
        {
            if (spells[i].time <= 0f)
            {
                if (targetDistance > spells[i].range)
                {
                    MoveToAttack = true;
                    spellForMove = spells[i];
                    break;
                }
                else
                {
                    runAttack(spells[i]);
                    break;
                }
            }
        }        
    }

    float patrolTimer = 0f;
    float patrolCooldown = 3f;
    Vector3 patrolPoint;
    private void Patrol()
    {
        if (patrolTimer <= 0)
        {
            Vector3 randomDir = UnityEngine.Random.insideUnitSphere * 5f;
            randomDir.y = 0;

            patrolPoint = transform.position + randomDir;
            patrolTimer = patrolCooldown;
        }

        transform.position = Vector3.MoveTowards(transform.position,
            patrolPoint, speed * Time.deltaTime);
    }

    private void Chase()
    {
        if (targetDistance > minStopChaseDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                target.position, speed * Time.deltaTime);
        }
        
        if (targetDistance < distanceAttackMin)
            state = State.Attack;

        if (targetDistance > stopChaseDistance)
            state = State.Idle;
    }

    private void TargetTrigger()
    {
        if(state == State.Attack || state == State.Chase)
            return;

        float distanceToTarget = targetDistance;
        if (distanceToTarget < triggerDistance && state != State.Chase)
            state = State.Chase;
    }
}