using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    /*   Variables   */
    //Holds audio and their tag
    struct AudioObject
    {
        public int tag;
        public GameObject audioPrefab;

        //Constructor for AudioObject
        public AudioObject(int Tag, GameObject AudioPrefab)
        {
            tag = Tag;
            audioPrefab = AudioPrefab;
        }
    };

    [SerializeField] IntEvent effectInt; //Get information on which effect to play
    [SerializeField] IntEvent songInt;   //Get information on which song to play
    [SerializeField] IntEvent effectDoneInt; // Returns the int and corresponding number when finished 

    //Effect Var.
    [SerializeField] GameObject[] effectPrefabsList;  //List of effect prefabs given by user
    private List<AudioObject> effectPoolList; //Save all instances of effects 

    //Song Var.
    [SerializeField] AudioClip[] songClipList;    //List of Song Clips
    
    AudioSource[] songList; //Holder of audioSource's with respective clips
    private int currentSong; //Save index of current song playing
    private int savedSong; //Will hold the tag for the saved song called by 


    //used to ensure onlyone instance of AudioManager exists
    public static AudioManager instance;


    /*        Implementation        */
    private void Awake()
    {
        //Ensure Only one instance of AudioManager Exists
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
			return;
        }
        DontDestroyOnLoad(gameObject);

        //Create variables 
        effectPoolList = new List<AudioObject>();
        

        //Create the songlist Array with songClipList
        songList = new AudioSource[songClipList.Length];
        for (int i = 0; i < songClipList.Length; i++)
        {
            songList[i] = gameObject.AddComponent<AudioSource>();
            songList[i].clip = songClipList[i];
            songList[i].loop = true;
        }
        //Set currentSong and savedSong to mute
        currentSong = -1;
        savedSong = -1;


        //Listeners to play audio
        songInt.AddListener(playSelectSong);
        effectInt.AddListener(playEffect);
    }

    //Used by listener of effectEvent
    //Plays events specified, those that loop and one time loopers
    //If is a looping effect that is playing, will stop rather than play event
    //Valid Inputs: Tracks: 0 - effectPrefabsList.length; Stop all effects: -1
    private void playEffect(int num)
    {
        //Verify that correct input given, otherwise doe nothing and return error
        if (num < -1 && effectPrefabsList.Length <= num)
        {
            Debug.Log("Incorrect input for playEffect! \n" +
                      "Valid inputs are: \n" +
                      "Effects: 0 -" + (effectPrefabsList.Length - 1) + "\n" +
                      "Stop all Effects: -1;");
            return;
        }
 
        //turns off all the sounds
        if (num == -1)
         {
            foreach (AudioObject effect in effectPoolList)
                effect.audioPrefab.GetComponent<AudioSource>().Stop(); //Stop all audioSources
            return;
         }

        //Check if object is in list *Note: Could probably be more efficient, may edit
        foreach (AudioObject effect in effectPoolList)
        {
            AudioSource sound = effect.audioPrefab.GetComponent<AudioSource>(); //Instance of audiosource

            //if effectPrefab being called is a LoopingEffect, and is Playing stop playing
            if (effect.tag == num && (sound.isPlaying) && (sound.loop))
            {
                sound.Stop();
                return;
            }

            //if effectPrefab being called is in our list and not playing, play it
            if (effect.tag == num && !(sound.isPlaying))
            {
                sound.Play();
                return;
            }
        }

        //Effect not found in list, so new one will be created and added
		if (this && gameObject) {
			AudioObject audioObject = new AudioObject(num, (GameObject)Instantiate(effectPrefabsList[num], gameObject.transform));
			effectPoolList.Add(audioObject);
			audioObject.audioPrefab.GetComponent<AudioSource>().Play();
		}
	}


    //Clears all effects prefab Objects 
    private void clearEffectPrefabs()
    {
        //Saves effects that happen to still be playing when effects cleared!
        List<AudioObject> effectListStillPlaying = new List<AudioObject>();

        foreach (AudioObject effect in effectPoolList)
        {
            if (effect.audioPrefab.GetComponent<AudioSource>().isPlaying)
            {
                //if effect happens to be playing in some transition, do not destroy yet
                effectListStillPlaying.Add(effect);
            }
            else
            {
                //Destroy if effect is no longer doing anything
                Destroy(effect.audioPrefab);
            }
        }
        effectPoolList.Clear(); //Clear whole list

        //Add all effects to pool list that happen to still be playing
        foreach (AudioObject effect in effectListStillPlaying)
        {
            effectPoolList.Add(effect);
        }
    }

    //Used by listener of songInt Event
    //Play Song based on Input, and turns off song currently playing 
    //Inputs:
    //0 - Length(songlist): Plays song in list
    // -1: Stop all songs playing
    // -2: Saves currently playing song tag (only one save at a time)
    // -3: Plays saved song (if none, mutes all songs)
    private void playSelectSong(int num)
    {
        //If given input is incorrect return error!
        if(num < -3 && songList.Length <= num)
        {
            Debug.Log("Incorrect input for playSong! \n" +
                      "Valid inputs are: Tracks: 0 -" + (songList.Length - 1) + "\n" + 
                      "Stop Music: -1; Save Song: -2; Play Saved Song: -3");
            return;
        }

        //Play savedSong
        if (num == -3)
        {
            //Prevention for infinite loop (Impossible but percation)
            if (savedSong == -3) { Debug.Log("Broke playSong b/c infinite loop"); return; }
            
            playSelectSong(savedSong);
            return;                     //End code
        }

        //Save current song to savedSong value
        if (num == -2)
        {      
            savedSong = currentSong;       //Save song
            return;                     //End code
        }

        //If song being called is already playing, do nothing
        if (currentSong == num) { return; }

        //Muting all the music
        if (num == -1)
        {
            StartCoroutine(stopSong(songList[currentSong])); // Stop the current song
            currentSong = -1;
        }
        
        //New track is going to be played
        if (num >= 0)
        {
            //Play if no song is currently playing
            if (currentSong == -1)
                StartCoroutine(playSong(songList[num]));
            //Transition to the new song
            else
                StartCoroutine(transitionSong(songList[num], songList[currentSong]));

            currentSong = num; //Set it to the new current song
        }
    }

    //Plays song when backgorund is mute with added effects
    IEnumerator playSong(AudioSource source)
    {
        source.volume = .25f;
        source.Play();
        while(source.volume < 1)
        {
            source.volume += .1f;
            yield return new WaitForSeconds(.1f);
        }
        source.volume = 1;
    }

    //Stop playing music with any added effects
    IEnumerator stopSong(AudioSource source)
    {
        while (source.volume > 0)
        {
            source.volume -= .05f;
            yield return new WaitForSeconds(.1f);
        }
        source.Stop();
        source.volume = 1;
    }

    //Transitions between songs playing with any added effects
    //sourcePlay is song being played, sourceStop is song being stoped
    IEnumerator transitionSong(AudioSource sourcePlay, AudioSource sourceStop)
    {
        sourcePlay.volume = 0;
        sourcePlay.Play();
        while (sourceStop.volume > 0 && sourcePlay.volume < 1)
        {
            sourcePlay.volume += .05f;
            sourceStop.volume -= .05f;
            yield return new WaitForSeconds(.1f);
        }
        sourceStop.volume = 0;
        sourceStop.Stop();
    }
}
