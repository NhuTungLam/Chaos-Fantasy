using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{
    public EnemyData enemyData;

    private EnemyMovement movement;
    private EnemySpawner spawner;
    private DropRateManager drop;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public float currentDamage, currentHealth;
    private CircleCollider2D collide;
    private bool isDead = false;

    private GameObject player;

    HashSet<(Collider2D, Collider2D)> ProjectileCollide = new HashSet<(Collider2D, Collider2D)>();
    AudioManager audioManager;

    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        spawner = FindObjectOfType<EnemySpawner>();
        drop = GetComponent<DropRateManager>();
        collide = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentDamage = enemyData.Damage;
        currentHealth = enemyData.MaxHealth;
        movement.currentSpeed = enemyData.Speed;
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void FixedUpdate()
    {
        CheckDeathAnimation();
        LayerMask mask = LayerMask.GetMask("Objects");
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, collide.radius, mask);
        HashSet<(Collider2D, Collider2D)> processedPairs = new HashSet<(Collider2D, Collider2D)>();

        foreach (Collider2D obj1 in nearbyObjects)
        {
            foreach (Collider2D obj2 in nearbyObjects)
            {
                if (obj1 == obj2)
                    continue;

                bool isProjectile1 = obj1.CompareTag("Projectile");
                bool isProjectile2 = obj2.CompareTag("Projectile");
                
                if (isProjectile1 || isProjectile2)
                {
                    if (ProjectileCollide.Contains((obj1, obj2)) || ProjectileCollide.Contains((obj2, obj1)))
                        continue;
                    
                    HandleProjectile(obj1, obj2, isProjectile1);
                    ProjectileCollide.Add((obj1, obj2));
                    continue;
                }
                
                if (processedPairs.Contains((obj1, obj2)) || processedPairs.Contains((obj2, obj1)))
                    continue;

                processedPairs.Add((obj1, obj2));

                HandleOverlap(obj1, obj2);
            }
        }

        CheckPlayerDistance();
    }

    private void CheckPlayerDistance()
    {
        if (Vector2.Distance(transform.position, player.transform.position) >= 20f)
        {
            Relocate();
        }
    }

    private void Relocate()
    {
        transform.position = player.transform.position + spawner.spawnPoints[Random.Range(0, spawner.spawnPoints.Count)].position;
    }

    private int currentHitEnemies = 0;

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        movement.Knockback(5f, 0.2f);

        currentHitEnemies++;
        float volume = Mathf.Clamp(1f / currentHitEnemies, 0.2f, 1f);
        audioManager.PlaySFX(audioManager.hitenemyMusic, volume);

        DamagePopUp.Create(transform.position, dmg);
        ScoreBoard.Instance.totalDamage += dmg;
        if (currentHealth <= 0)
        {
            Die();
        }
        currentHitEnemies--;
    }


    private void Die()
    {
        isDead = true;
        currentHealth = 0;

        if (movement != null) movement.enabled = false;

        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
        
        ProjectileCollide.Clear(); 

        if (drop != null) drop.DropPickUp();

        if (collide != null) collide.enabled = false;

        ScoreBoard.Instance.enemyKilled++;
    }

    private void CheckDeathAnimation()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(enemyData.deathAnimName) && stateInfo.normalizedTime > 1 && !animator.IsInTransition(0)) // Hoạt ảnh "Die" kết thúc
            {
                ReturnToPool();
            }
        }
    }

    private void ReturnToPool()
    {

        ObjectPools.EnqueueObject(this, enemyData.name);
        spawner.enemiesAlive--;
        ResetEnemy();
    }

    void HandleOverlap(Collider2D obj1, Collider2D obj2)
    {
        bool isPlayer1 = obj1.CompareTag("Player");
        bool isPlayer2 = obj2.CompareTag("Player");

        Vector2 direction = (Vector2)(obj1.transform.position - obj2.transform.position);
        float distance = direction.magnitude;

        float combinedRadius = obj1.bounds.extents.x + obj2.bounds.extents.x;

        float overlap = combinedRadius - distance;

        if (overlap > 0) 
        {
            Vector3 halfOverlap = (overlap / 2) * direction.normalized;

            if (!isPlayer1 && !isPlayer2)
            {
                obj1.transform.position += halfOverlap;
                obj2.transform.position -= halfOverlap;
            }
            else if (isPlayer1)
            {
                obj1.GetComponent<CharacterHandler>().TakeDamage(currentDamage);
                obj2.transform.position -= (Vector3)(overlap * direction.normalized);
            }
            else
            {
                obj2.GetComponent<CharacterHandler>().TakeDamage(currentDamage);
                obj1.transform.position += (Vector3)(overlap * direction.normalized);
            }
        }
    }

    void HandleProjectile(Collider2D obj1, Collider2D obj2, bool isProjectile1)
    {
        if (isProjectile1)
        {
            Projectile projectile = obj1.GetComponent<Projectile>();
            TakeDamage((int)projectile.MightAppliedDamaged());
            projectile.DecreasePierce();
        }
        else
        {
            Projectile projectile = obj2.GetComponent<Projectile>();
            TakeDamage((int)projectile.MightAppliedDamaged());
            projectile.DecreasePierce();
        }
    }


    void ResetEnemy()
    {
        isDead = false;
        currentHealth = enemyData.MaxHealth;
        
        Color color = spriteRenderer.color;
        color = new Color(1, 1, 1); 
        spriteRenderer.color = color;

        if (collide != null) collide.enabled = true;

        if (movement != null) movement.enabled = true;

        if (animator != null) animator.SetBool("isDead", false);

    }
}


    

