using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamage
{
    [SerializeField] public int maxHealth = 100;

    public float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        gameManager.instance.healthBar.UpdateHealthBar(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeHealing(float amount)

    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        gameManager.instance.healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    private void Die()
    {
        gameManager.instance.Death();

    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
