using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public List<AudioSource> CommandSounds;
    public List<AudioSource> RecallSounds;
    public List<AudioSource> FightSounds;


    public void PlayCommandSound()
    {
        CommandSounds[UnityEngine.Random.Range(0, CommandSounds.Count)].Play();
    }
    public void PlayRecallSound()
    {
        RecallSounds[UnityEngine.Random.Range(0, RecallSounds.Count)].Play();
    }
    public void PlayFightSound()
    {
        FightSounds[UnityEngine.Random.Range(0, FightSounds.Count)].Play();
    }
}
