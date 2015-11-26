using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    [SerializeField] protected float INITIALHP = 100;
    [SerializeField] protected float pushReloadTime = 3f;
    [SerializeField] protected float sonarReloadTime = 3f;
    [SerializeField] protected float sonarHealthCost = 0;
    [SerializeField] protected float absorbMultiplier = 1000;

    //FOR TESTING: TODO: REMOVE!
    [SerializeField] protected bool DEBUGMODE = false;
    
    private float pushReloadRemaining = 0;
    private float sonarReloadRemaining = 0;
    TwinStickController controller;

    public GameObject pushingParticles;
    public float currentHp;
    public GameData.Team team { get; private set; }
    public bool canShoot { get; private set; }
    public bool canPush { get; private set; }
    public bool canSonar { get; private set; }

    //Events that can be hooked into
    public delegate void PlayerShot(Transform shot);
    public static event PlayerShot OnPlayerShot;

    //health indic
    [SerializeField] Transform[] healthIndicatorsT;
    [SerializeField] bool isTutorialLevel = false;
    float alpha = 1;
    //

    // Use this for initialization
    void Start() {
        currentHp = INITIALHP;
        if (isTutorialLevel) {
            currentHp = 100;
        }
        team = GameData.Team.Player;
        canShoot = true;
        controller = GetComponent<TwinStickController>();
        
        foreach (Transform t in healthIndicatorsT) {
            Color finalColor = Color.white * Mathf.LinearToGammaSpace(0.003F);
            SkinnedMeshRenderer meshRend = t.GetComponent<SkinnedMeshRenderer>();
            foreach (Material m in meshRend.materials) {
                m.SetColor("_EmissionColor", finalColor);
            }
        }
        //
    }

    // Update is called once per frame
    void Update() {
        //if (!canShoot) shootReloadRemaining -= Time.deltaTime;
        if (!canPush) pushReloadRemaining -= Time.deltaTime;
        if (!canSonar) sonarReloadRemaining -= Time.deltaTime;
        /*
        if (shootReloadRemaining <= 0) {
            canShoot = true;
        }
        else {
            canShoot = false;
        }
        */
        if (pushReloadRemaining <= 0) {
            canPush = true;
        }
        else {
            canPush = false;
        }
        if (sonarReloadRemaining <= 0 && currentHp > sonarHealthCost + 1) {
            canSonar = true;
        }
        else {
            canSonar = false;
        }

        if (currentHp <= 0)
            Destroy(gameObject);
        else
            affectHealthIndicator();
    }

    public void Shoot(Transform shot) {
        if (OnPlayerShot != null) OnPlayerShot(shot);
        //Play Shoot animation
    }

    public void Push() {
        pushReloadRemaining = pushReloadTime;

        Quaternion particleRotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
        GameObject pa = Instantiate(pushingParticles, transform.position, particleRotation) as GameObject;

        //Play Push animation
    }

    public void Sonar() {
        sonarReloadRemaining = sonarReloadTime;
        currentHp -= sonarHealthCost;
        //Play Sonar animation
    }

    public bool Drain(float amount) {
        //Play Drain animation (Needs to be continous
        //TESTING REMOVE
        if (DEBUGMODE) return true;

        if (currentHp >= INITIALHP) return false;
        else {
            currentHp += (amount*100*(0.5F*INITIALHP));
            if (currentHp > INITIALHP)
            {
                currentHp = INITIALHP;
            }
            return true;
        }
    }

    public void StopDraining() {
        //Stop Drain animation
    }

    public void Use(InteractableObject iObj) {
        iObj.Use();
        //Play Use Animation
    }

    public void TakeDamage(float dmg) {
        //Play take damage animation
        currentHp -= dmg;
        controller.TakeDamage();
    }

    void Die() {
        //Play death animation
    }

    void affectHealthIndicator()
    {
        if (alpha != ((0.5f / INITIALHP) * currentHp) +0.5f)
        {
            alpha = ((0.5f / INITIALHP) * currentHp) + 0.5f;

            foreach (Transform t in healthIndicatorsT) {
                Color baseColor = Color.white;
                Color finalColor = baseColor * Mathf.LinearToGammaSpace(1-alpha);
                MeshRenderer meshRend = t.GetComponent<MeshRenderer>();
                foreach (Material m in meshRend.materials) {
                    m.SetColor("_EmissionColor", finalColor);
                }
            }
        }
    }
}
