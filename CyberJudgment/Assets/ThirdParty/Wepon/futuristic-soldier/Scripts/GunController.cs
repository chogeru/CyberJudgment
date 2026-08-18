using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{

    [SerializeField] ParticleSystem muzzle;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip sfx;
    [SerializeField] GameObject character;
    [SerializeField] float fireRate = 0.05f;
    float timerToNextShoot = 0;
    Animator anim;
    bool shooting = false;
    SoldierController controller;

    // Start is called before the first frame update
    void Start()
    {
        anim = character.GetComponentInChildren<Animator>();
        controller = character.GetComponent<SoldierController>();
        muzzle.transform.localScale *= controller.Scale();
    }

    void Update(){
        if(Input.GetMouseButton(0) && !controller.rolling && !controller.reloading && controller.alive){
            Fire();
        }else{
            if(shooting){
                shooting = false;
                anim.SetFloat("shooting", 0);
                muzzle.Stop();
            }

        }
    }

    public void Fire(){
        if(timerToNextShoot <= 0){
            timerToNextShoot += fireRate;
            shooting = true;
            anim.SetFloat("shooting", 1);
            audioSource.PlayOneShot(sfx);
            if(!muzzle.isPlaying){
                muzzle.Play();
            }
        }else{
            timerToNextShoot -= Time.deltaTime;
        }
    }
}
