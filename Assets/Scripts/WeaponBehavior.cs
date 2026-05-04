using UnityEngine;

public class WeaponBehaviour : MonoBehaviour
{
    [SerializeField] public WeaponData weaponData;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    private float nextUseTime;

 

    public void UseWeapon(Vector3 targetPoint)
    {
        
        if (weaponData == null)
            return;

        if (Time.time < nextUseTime)
            return;

        switch (weaponData.weaponType)
        {
            case WeaponType.Ranged:
               
                FireBullet(targetPoint);
                if (weaponData.fireRate > 0f)
                {
                    nextUseTime = Time.time + (1f / weaponData.fireRate);
                }
                break;

            case WeaponType.Melee:
                
                PerformMelee();
                if (weaponData.attackSpeed > 0f)
                {
                    nextUseTime = Time.time + (1f / weaponData.attackSpeed);
                }
                break;
        }
    }

    private void FireBullet(Vector3 targetPoint)
    {
        
   
        if (weaponData.projectilePrefab == null || muzzlePoint == null)
            return;

        Vector3 direction = (targetPoint - muzzlePoint.position).normalized;

        Vector3 camForward = Camera.main.transform.forward;

        // Ensure it never flips or locks to wrong point
        if (Vector3.Dot(direction, camForward) < 0f)
        {
            direction = camForward;
        }


        if (audioSource != null && fireClip != null)
        {
            audioSource.PlayOneShot(fireClip);
        }

        GameObject bullet = Instantiate(
            weaponData.projectilePrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(direction)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * weaponData.projectileSpeed;
        }

        Damage damageScript = bullet.GetComponent<Damage>();
        if (damageScript != null)
        {
            damageScript.SetDamage(weaponData.damage);
            damageScript.Launch(direction, weaponData.projectileSpeed);
        }
        else
        {

            if (rb != null)
            {
                rb.linearVelocity = direction * weaponData.projectileSpeed;
            }
        }
            ThirdPersonCameraController cam = FindFirstObjectByType<ThirdPersonCameraController>();
        if (cam != null)
        {
            cam.TriggerShake(0.1f, 0.2f);
        }

      

    }

    private void PerformMelee()
    {
        
    }
}