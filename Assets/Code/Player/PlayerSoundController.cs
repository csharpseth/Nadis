using UnityEngine;

public class PlayerSoundController : MonoBehaviour, IEventAccessor, INetworkInitialized
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

    public void InitFromNetwork(int playerID)
    {
        NetID = playerID;
        Subscribe();
    }
    
    public void Subscribe()
    {
        Events.BipedAnimator.OnRightFootStepping += RightFootStepping;
        Events.BipedAnimator.OnLeftFootStepping += LeftFootStepping;

        Events.BipedAnimator.OnRightFootBeginStep += RightFootBeginStep;
        Events.BipedAnimator.OnLeftFootBeginStep += LeftFootBeginStep;

        Events.BipedAnimator.OnRightFootFinishStep += RightFootFinishStep;
        Events.BipedAnimator.OnLeftFootFinishStep += LeftFootFinishStep;

        Events.Player.UnSubscribe += UnSubscribe;
    }
    public void UnSubscribe(int netID)
    {
        if (NetID != netID) return;

        Events.BipedAnimator.OnRightFootStepping -= RightFootStepping;
        Events.BipedAnimator.OnLeftFootStepping -= LeftFootStepping;

        Events.BipedAnimator.OnRightFootBeginStep -= RightFootBeginStep;
        Events.BipedAnimator.OnLeftFootBeginStep -= LeftFootBeginStep;

        Events.BipedAnimator.OnRightFootFinishStep -= RightFootFinishStep;
        Events.BipedAnimator.OnLeftFootFinishStep -= LeftFootFinishStep;

        Events.Player.UnSubscribe -= UnSubscribe;
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

    public void RightFootBeginStep(int netID)
    {
        if (CanPlay == false || netID != NetID)
            return;

        rightHipSrc.pitch = Pitch();
        rightHipSrc.PlayOneShot(defaultJointSound);
        AddActive();
        Invoke("RemoveActive", defaultJointSound.length);
    }
    public void LeftFootBeginStep(int netID)
    {
        if (CanPlay == false || netID != NetID)
            return;

        leftHipSrc.pitch = Pitch();
        leftHipSrc.PlayOneShot(defaultJointSound);
        AddActive();
        Invoke("RemoveActive", defaultJointSound.length);
    }

    public void RightFootFinishStep(int netID)
    {
        if (CanPlay == false || netID != NetID)
            return;
        AudioClip clip = GetStepClip();

        rightFootSrc.pitch = Pitch();
        rightFootSrc.PlayOneShot(clip);
        AddActive();
        Invoke("RemoveActive", clip.length);
    }
    public void LeftFootFinishStep(int netID)
    {
        if (CanPlay == false || netID != NetID)
            return;
        AudioClip clip = GetStepClip();

        leftFootSrc.pitch = Pitch();
        leftFootSrc.PlayOneShot(clip);
        AddActive();
        Invoke("RemoveActive", clip.length);
    }

    public void RightFootStepping(int netID)
    {
        if (CanPlay == false || netID != NetID)
            return;

    }
    public void LeftFootStepping(int netID)
    {
        if (CanPlay == false || netID != NetID)
            return;

    }

    public float Pitch()
    {
        float v = Random.Range(-1f, 1f);
        return 1f + (v * pitchDifference);
    }
}
