using UnityEngine;
using System.Collections;

public class Dron2 : Enemy
{
    [Header("Mejoras Dron 2")]
    public float tiempoEntreBalas = 0.2f;
    protected override void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || !canShoot) return;

        // Iniciamos una ráfaga especial
        StartCoroutine(RafagaDoble());
    }

    private IEnumerator RafagaDoble()
    {
        // Dispara la primera bala usando la lógica del padre (Enemy.cs)
        base.Shoot();

        // Espera una fracción de segundo
        yield return new WaitForSeconds(tiempoEntreBalas);

        // Dispara la segunda bala
        base.Shoot();
    }
}

