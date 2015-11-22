using UnityEngine;
using System.Collections;

public class Activator_GroundButton : Activator {

    [SerializeField] MeshRenderer[] activationCrystals;
    [SerializeField] AudioClip whenPlayerInside, onActivate;
    [SerializeField] GameObject activationParticle;
    [SerializeField] ParticleSystem playerNearbyParticle;
    bool emitNearbyParticle = true;

    CapsuleCollider playerDetector;
    Player player;
    bool crystalsRevealed = false;
    bool playerNearby = false;
    AudioSource audioSource;

    // Use this for initialization
    protected override void Start () {
        base.Start();
        playerDetector = GetComponent<CapsuleCollider>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        audioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        bool test = true;
        foreach (MeshRenderer m in activationCrystals) {
            if (!m.enabled) test = false;
        }
        crystalsRevealed = test;
        Bounds playerCheck = playerDetector.bounds;
        PlayerNearby(playerCheck.Contains(player.transform.position));
        if (playerNearby && !audioSource.isPlaying) {
            audioSource.PlayOneShot(whenPlayerInside);
        }

        if (crystalsRevealed && playerNearby && emitNearbyParticle)
        {
            if (!playerNearbyParticle.isPlaying) playerNearbyParticle.Play();
        }
        else
        {
            if (playerNearbyParticle.isPlaying) playerNearbyParticle.Stop();
        }
    }

    void PlayerNearby(bool isNearby) {
        //Make stuff light up and be pretty if player is nearby
        playerNearby = isNearby;
    }

    public override void Activate() {
        if (!crystalsRevealed || !playerNearby) return;
        audioSource.Stop();
        audioSource.PlayOneShot(onActivate);

        //Show particle
        if (activationParticle != null)
        {
            foreach (MeshRenderer mr in activationCrystals)
            {
                Instantiate(activationParticle, mr.transform.position, Quaternion.identity);
            }
        }

        emitNearbyParticle = false;

        base.Activate();
    }
}
