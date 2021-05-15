using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class AmbientMusicManager : NetworkBehaviour
{
    public List<string> ambientMusic;
    public float checkDuration;
    public float checkChance;

    // Start is called before the first frame update
    void Start()
    {
        if(isServer)
            StartCoroutine(ambientMusicLoop());
    }
//TODO fix main menu sound
    IEnumerator ambientMusicLoop()
    {
        System.Random random = new System.Random();
        while (true)
        {
            yield return new WaitForSeconds(checkDuration);

            if(random.NextDouble() < checkChance)
            {
                PlaySong();
            }
        }
    }

    [Server]
    public void PlaySong()
    {
        System.Random random = new System.Random();
        string songName = ambientMusic[random.Next(0, ambientMusic.Count)];

        Sound.Play(new Location(), "music/ambient/" + songName + "/" + songName, SoundType.Music, 1f, 1f, int.MaxValue, false);
    }
}
