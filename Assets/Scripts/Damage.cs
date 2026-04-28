using UnityEngine;

public class Damage : MonoBehaviour
{
    enum damageType { bullet, stationary, shockwave }

    [SerializeField] damageType Type;
    [SerializeField] Rigidbody rb;
    [SerializeField] int damageAmount = 10;
    [SerializeField] float speed = 200;
    [SerializeField] int destroyTime = 5;
    [SerializeField] ParticleSystem hitEffect;

    bool hasHit = false;

    public void SetDamage(int amount)
    {
        damageAmount = amount;
    }

    void Start()
    {
        if (rb == null)

            rb = GetComponent<Rigidbody>();
        if (Type == damageType.bullet)
        {
            rb.AddForce(transform.forward * speed);
            Destroy(gameObject, destroyTime);
        }


    }
    public void Launch(Vector3 direction, float launchSpeed)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        speed = launchSpeed;

        rb.useGravity = false;
        rb.linearVelocity = direction.normalized * speed;

        transform.forward = direction.normalized;

        Debug.Log("Launch() called in Damage.cs");

    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        IDamage dmg = other.GetComponentInParent<IDamage>();

        if (dmg != null)
        {
            hasHit = true;
            dmg.TakeDamage(damageAmount);

            if (other.CompareTag("Enemy") && hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}