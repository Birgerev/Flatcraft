using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class AmbientMusicManager : NetworkBehaviour
{
    public List<string> ambientMusic;
    public float checkDuration;
    public float checkChance;

    // Start is called before the first frame update
    private void Start()
    {
        if (isServer)
            StartCoroutine(ambientMusicLoop());
    }

    private IEnumerator ambientMusicLoop()
    {
        Random random = new Random();
        while (true)
        {
            yield return new WaitForSeconds(checkDuration);

            if (random.NextDouble() < checkChance)
                PlaySong();
        }
    }

    [Server]
    public void PlaySong()
    {
        Random random = new Random();
        string songName = ambientMusic[random.Next(0, ambientMusic.Count)];

        Sound.Play(new Location(), "music/ambient/" + songName + "/" + songName, SoundType.Music, 1f, 1f, int.MaxValue
            , false);
    }
}