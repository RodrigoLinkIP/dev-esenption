using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bulletDefault;
    public Transform firePoint;

    public float fireRate = 0.2f;

    public int cantidadBalas = 1;
    public float dispersion = 0f;

    private float nextFireTime = 0f;
    private GameObject weaponSprite;

    void Start()
    {
        Transform spriteTransform = transform.Find("WeaponSprite");

        if (spriteTransform != null)
        {
            weaponSprite = spriteTransform.gameObject;

            // El arma empieza oculta
            weaponSprite.SetActive(false);
        }
    }

    public void SetVisible(bool visible)
    {
        if (weaponSprite != null)
        {
            weaponSprite.SetActive(visible);
        }
    }

    public void Shoot(Vector2 direction)
    {
        if (Time.time < nextFireTime)
            return;

        for (int i = 0; i < cantidadBalas; i++)
        {
            float angulo = Random.Range(-dispersion, dispersion);

            Vector2 direccionFinal = Quaternion.Euler(
                0f,
                0f,
                angulo
            ) * direction;

            GameObject bullet = Instantiate(
                bulletDefault,
                firePoint.position,
                Quaternion.identity
            );

            bullet.GetComponent<Bullet>().SetDirection(direccionFinal);
        }

        nextFireTime = Time.time + fireRate;
    }
}
