using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientMusicManager : MonoBehaviour
{
    public List<string> ambientMusic;
    public float checkDuration;
    public float checkChance;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ambientMusicLoop());
    }

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

    public void PlaySong()
    {
        System.Random random = new System.Random();
        string songName = ambientMusic[random.Next(0, ambientMusic.Count)];

        Sound.Play(Player.localInstance.Location, "music/ambient/" + songName + "/" + songName, SoundType.Music, 1f, 1f, 100000);
    }
}
