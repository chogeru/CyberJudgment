using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    
    AudioSource audioSource;
    [SerializeField] AudioClip stepWalk;
    [SerializeField] AudioClip stepRun;
    [SerializeField] AudioClip reload;
    [SerializeField] AudioClip roll;
    [SerializeField] AudioClip death;
    SoldierController controller;

    void Start()
    {
        controller = transform.parent.gameObject.GetComponent<SoldierController>();
        audioSource = GetComponent<AudioSource>();
    }

    public void StepRun(){
        audioSource.PlayOneShot(stepRun);
    }

    public void StepWalk(){
        audioSource.PlayOneShot(stepWalk);
    }

    public void Roll(){
        audioSource.PlayOneShot(roll);
    }

    public void StopRolling(){
        controller.rolling = false;
    }

    public void StartReloading(){
        audioSource.PlayOneShot(reload);
    }

    public void FinishReloading(){
        controller.reloading = false;
    }

    public void Death1(){
        audioSource.PlayOneShot(stepWalk);
    }

    public void Death2(){
        audioSource.PlayOneShot(death);
    }
}
