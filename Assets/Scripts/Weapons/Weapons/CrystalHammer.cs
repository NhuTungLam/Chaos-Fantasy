using UnityEngine;
using System.Collections;

public class CrystalHammer : Weapon
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();

        switch (currentLevel)
        {
            case 1:
                StartCoroutine(SpawnImpactsInLine(1.8f, pm.ShootDir,1)); 
                break;
            case 2:
                StartCoroutine(SpawnImpactsInLine(1.5f, pm.ShootDir,3));
                break;
            case 3:
                StartCoroutine(SpawnImpactsInLine(1.5f, pm.ShootDir,5));
                break;
            case 4:
                StartCoroutine(SpawnImpactsInLine(1.8f, pm.ShootDir, 8)); 
                break;
        }
    }

    private void SpawnImpact(float size, Vector2 direction)
    {
        GameObject impact = Instantiate(weaponData.prefab);
        CrystalHammerProjectile projectile = impact.GetComponent<CrystalHammerProjectile>();
        impact.transform.localScale = Vector3.one * size;
        if (projectile != null)
        {
            projectile.CheckDirection(direction);
        }
        impact.transform.position = this.transform.position + (Vector3)direction * 0.5f; // Điều chỉnh khoảng cách
    }

    // Coroutine để spawn các đòn đánh theo đường thẳng, dựa trên hướng người chơi
    private IEnumerator SpawnImpactsInLine(float size, Vector2 direction, int numImpacts = 4)
    {
        float distanceBetweenAttacks = 1f; 
        Transform previousImpactTransform = null;

        for (int i = 0; i < numImpacts; i++) 
        {
            GameObject impact = Instantiate(weaponData.prefab);
            CrystalHammerProjectile projectile = impact.GetComponent<CrystalHammerProjectile>();
            impact.transform.localScale = Vector3.one * size;

            if (projectile != null)
            {
                projectile.CheckDirection(direction);
            }

            if (i == 0)
            {
                impact.transform.position = this.transform.position + (Vector3)direction * 0.5f;
            }
            else if (previousImpactTransform != null)
            {
                impact.transform.position = previousImpactTransform.position + (Vector3)(direction.normalized * distanceBetweenAttacks);
            }

            previousImpactTransform = impact.transform;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public override bool LevelUp()
    {
        if (!base.LevelUp()) return false;
        return true;
    }
}
