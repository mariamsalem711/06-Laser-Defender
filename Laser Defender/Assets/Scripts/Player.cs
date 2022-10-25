using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // configuration parameters
    [Header("Player")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float padding = 1f;
    [SerializeField] int health = 200;
    [SerializeField] int healthRegen = 5;
    [SerializeField] AudioClip gameOverSound;
    [SerializeField] [Range(0, 1)]
    float gameOverSoundVolume = 0.75f;
    [SerializeField] AudioClip shootSound;
    [SerializeField] [Range(0, 1)]
    float shootSoundVolume = 0.25f;

    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileFiringPeriod = 0.1f;

    Coroutine firingCoroutine;

    float xMin;
    float xMax;
    float yMin;
    float yMax;

    float healTimer;
    int maxHealth;
    bool healing;

    // Use this for initialization
    void Start() {
        SetUpMoveBoundaries();
        SetUpHealing();
    }

    // Update is called once per frame
    void Update() {
        Move();
        Fire();
        Heal();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();

        if (!damageDealer)
        {
            return;
        }

        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();

        damageDealer.Hit();

        if (health <= 0)
        {
            healing = false;
            PlayerLose();
        }
    }

    private void PlayerLose()
    {
        FindObjectOfType<Level>().LoadGameOver();

        Destroy(gameObject);

        AudioSource.PlayClipAtPoint(gameOverSound, Camera.main.transform.position, gameOverSoundVolume);
    }

    public int GetHealth()
    {
        return health;
    }

    private void Fire()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            firingCoroutine = StartCoroutine(FireContinuously());
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);
        }
    }

    IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject laser = Instantiate(
                    laserPrefab,
                    transform.position,
                    Quaternion.identity) as GameObject;
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, shootSoundVolume);
            yield return new WaitForSeconds(projectileFiringPeriod);
        }
    }

    private void Move()
    {
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);
        transform.position = new Vector2(newXPos, newYPos);
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;
        xMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + padding;
        xMax = gameCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - padding;
        yMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + padding;
        yMax = gameCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - padding;
    }

    private void SetUpHealing()
    {
        healTimer = Time.time;
        maxHealth = health;
        healing = true;
    }

    private void Heal()
    {
        if (!healing)
            return;

        if(Time.time - healTimer > 1.0f )
        {
            health += healthRegen;

            if (health > maxHealth)
            {
                health = maxHealth;
            }

            healTimer = Time.time;
        }
    }
}
