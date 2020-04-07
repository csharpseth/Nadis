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
    
    private int currentlyActiveSources = 0;
    private bool CanPlay
    {
        get
        {
            return (currentlyActiveSources < maxActiveSources);
        }
    }

    public int NetID { get; private set; }

    public void InitFromServer(int playerID)
    {
        NetID = playerID;

        Events.BipedAnimator.OnRightFootStepping += RightFootStepping;
        Events.BipedAnimator.OnLeftFootStepping += LeftFootStepping;

        Events.BipedAnimator.OnRightFootBeginStep += RightFootBeginStep;
        Events.BipedAnimator.OnLeftFootBeginStep += LeftFootBeginStep;

        Events.BipedAnimator.OnRightFootFinishStep += RightFootFinishStep;
        Events.BipedAnimator.OnLeftFootFinishStep += LeftFootFinishStep;

        Events.Player.UnSubscribe += UnSubcribe;
    }
    
    private void UnSubcribe(int playerID)
    {
        if (NetID != playerID) return;

        Events.BipedAnimator.OnRightFootStepping -= RightFootStepping;
        Events.BipedAnimator.OnLeftFootStepping -= LeftFootStepping;

        Events.BipedAnimator.OnRightFootBeginStep -= RightFootBeginStep;
        Events.BipedAnimator.OnLeftFootBeginStep -= LeftFootBeginStep;

        Events.BipedAnimator.OnRightFootFinishStep -= RightFootFinishStep;
        Events.BipedAnimator.OnLeftFootFinishStep -= LeftFootFinishStep;

        Events.Player.UnSubscribe -= UnSubcribe;
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

    public void RightFootBeginStep(int playerID)
    {
        if (CanPlay == false || playerID != NetID)
            return;

        rightHipSrc.pitch = Pitch();
        rightHipSrc.PlayOneShot(defaultJointSound);
        AddActive();
        Invoke("RemoveActive", defaultJointSound.length);
    }
    public void LeftFootBeginStep(int playerID)
    {
        if (CanPlay == false || playerID != NetID)
            return;

        leftHipSrc.pitch = Pitch();
        leftHipSrc.PlayOneShot(defaultJointSound);
        AddActive();
        Invoke("RemoveActive", defaultJointSound.length);
    }

    public void RightFootFinishStep(int playerID)
    {
        if (CanPlay == false || playerID != NetID)
            return;
        AudioClip clip = GetStepClip();

        rightFootSrc.pitch = Pitch();
        rightFootSrc.PlayOneShot(clip);
        AddActive();
        Invoke("RemoveActive", clip.length);
    }
    public void LeftFootFinishStep(int playerID)
    {
        if (CanPlay == false || playerID != NetID)
            return;
        AudioClip clip = GetStepClip();

        leftFootSrc.pitch = Pitch();
        leftFootSrc.PlayOneShot(clip);
        AddActive();
        Invoke("RemoveActive", clip.length);
    }

    public void RightFootStepping(int playerID)
    {
        if (CanPlay == false || playerID != NetID)
            return;

    }
    public void LeftFootStepping(int playerID)
    {
        if (CanPlay == false || playerID != NetID)
            return;

    }

    public float Pitch()
    {
        float v = Random.Range(-1f, 1f);
        return 1f + (v * pitchDifference);
    }
}
