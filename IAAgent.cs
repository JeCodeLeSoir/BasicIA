using System;
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
        Attack,
        Flee
    }

    [SerializeField] Transform target; // Cible
    [SerializeField] float triggerDistance = 10f; // Distance de detection de la cible
    [SerializeField] float minStopChaseDistance = 1f; // Distance minimale pour arreter de poursuivre
    [SerializeField] float stopChaseDistance = 15f; // Distance pour arreter de poursuivre
    [SerializeField] float distanceAttackMin; // Distance minimale d'attaque
    [SerializeField] float targetDistance; // Distance par rapport a la cible
    [SerializeField] float secureDistance = 15f; // Distance de securite en fuyant
    [SerializeField] float speed = 5f; // Vitesse
    [SerializeField] State state = State.Idle; // Etat

    [SerializeField] bool isfleeing = false; // Fuir le combat

    [SerializeField] Spell[] spells; // Sorts disponibles

    private bool MoveToAttack; // Se deplacer pour attaquer
    private Spell spellForMove; // Sort utilise pour se deplacer vers la cible

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
            case State.Flee:
                Flee();
                break;
        }

        ResetCooldowns();
    }

    private void Flee()
    {
        Vector3 directionAwayFromTarget = (transform.position - target.position).normalized;
        transform.position += directionAwayFromTarget * speed * Time.deltaTime;
        if (targetDistance > secureDistance)
        {
            state = State.Idle;
        }
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
        if (isfleeing) 
        {
            if(targetDistance < triggerDistance)
                state = State.Flee;

            return;
        }

        if (state == State.Attack || state == State.Chase)
            return;
 
        if (targetDistance < triggerDistance && state != State.Chase)
            state = State.Chase;
    }
}