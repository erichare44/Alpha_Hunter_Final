using UnityEngine;

public class ReticleController : MonoBehaviour
{
    [SerializeField] private ThirdPersonMotor playerMotor;
    [SerializeField] private GameObject reticle;

    private void Update()
    {
        if (playerMotor == null)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                playerMotor = player.GetComponent<ThirdPersonMotor>();
            }
        }

        if (playerMotor == null || reticle == null)
            return;

        reticle.SetActive(playerMotor.IsAiming);
    }
}