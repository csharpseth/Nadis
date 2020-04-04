using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    public AudioClip[] defaultStepSounds;
    public AudioClip defaultJointSound;
    public float pitchDifference = 0.2f;
    public int maxActiveSources = 2;

    public AudioSource rightFootSrc;
    public AudioSource leftFootSrc;

    public AudioSource rightHipSrc;
    public AudioSource leftHipSrc;

    private BipedProceduralAnimator animator;
    private int currentlyActiveSources = 0;
    private bool CanPlay
    {
        get
        {
            return (currentlyActiveSources < maxActiveSources);
        }
    }

    private void Awake()
    {
        Events.BipedAnimator.OnRightFootStepping += RightFootStepping;
        Events.BipedAnimator.OnLeftFootStepping += LeftFootStepping;

        Events.BipedAnimator.OnRightFootBeginStep += RightFootBeginStep;
        Events.BipedAnimator.OnLeftFootBeginStep += LeftFootBeginStep;

        Events.BipedAnimator.OnRightFootFinishStep += RightFootFinishStep;
        Events.BipedAnimator.OnLeftFootFinishStep += LeftFootFinishStep;
    }
    
    private void AddActive()
    {
        currentlyActiveSources++;
    }
    private void RemoveActive()
    {
        currentlyActiveSources--;
    }

    private AudioClip GetStepClip()
    {
        int index = Random.Range(0, defaultStepSounds.Length);
        return defaultStepSounds[index];
    }

    public void RightFootBeginStep()
    {
        if (CanPlay == false)
            return;

        rightHipSrc.pitch = Pitch();
        rightHipSrc.PlayOneShot(defaultJointSound);
        AddActive();
        Invoke("RemoveActive", defaultJointSound.length);
    }
    public void LeftFootBeginStep()
    {
        if (CanPlay == false)
            return;

        leftHipSrc.pitch = Pitch();
        leftHipSrc.PlayOneShot(defaultJointSound);
        AddActive();
        Invoke("RemoveActive", defaultJointSound.length);
    }

    public void RightFootFinishStep()
    {
        if (CanPlay == false)
            return;
        AudioClip clip = GetStepClip();

        rightFootSrc.pitch = Pitch();
        rightFootSrc.PlayOneShot(clip);
        AddActive();
        Invoke("RemoveActive", clip.length);
    }
    public void LeftFootFinishStep()
    {
        if (CanPlay == false)
            return;
        AudioClip clip = GetStepClip();

        leftFootSrc.pitch = Pitch();
        leftFootSrc.PlayOneShot(clip);
        AddActive();
        Invoke("RemoveActive", clip.length);
    }

    public void RightFootStepping(float angle)
    {
        if (CanPlay == false)
            return;

    }
    public void LeftFootStepping(float angle)
    {
        if (CanPlay == false)
            return;

    }

    public float Pitch()
    {
        float v = Random.Range(-1f, 1f);
        return 1f + (v * pitchDifference);
    }
}
