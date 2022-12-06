using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public string[] songs;
    private GameObject currentSongSource;

    private void Start()
    {
    }
    
    // Update is called once per frame
    void Update()
    {
        if(currentSongSource == null)
            PlayNextSong();
    }

    public void PlayNextSong()
    {
        System.Random r = new System.Random();
        string song = songs[r.Next(0, songs.Length)];
        
        currentSongSource = Sound.PlayLocal(new Location(0, 0), song, 0, SoundType.Music, 1, 500, false);
    }
}
