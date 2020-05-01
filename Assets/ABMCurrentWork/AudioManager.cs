using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    /*   Variables   */
    [SerializeField] IntEvent effectInt; //Get information on which effect to play
    [SerializeField] IntEvent songInt;   //Get information on which song to play

    [SerializeField] GameObject[] effectPrefabsList;  //List of effect prefabs given by user
    [SerializeField] GameObject[] songPrefabsList;    //List of song prefabs given by user



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

    //Variables to hold audio objects
    private List<AudioObject> effectPoolList; //Save all instances of effects 
    private Queue<AudioObject> songQueue;   //Will hold current song playing

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
        songQueue = new Queue<AudioObject>();

        savedSong = -1; //Default saved song is mute

        //Listen for command to play audio
        songInt.AddListener(playSong);
        effectInt.AddListener(playEffect);
    
        
    
    }

    //Used by listener of effectEvent
    //Plays events specified, those that loop and one time loopers
    //If is a looping effect that is playing, will stop rather than play event
    private void playEffect(int num)
    {
        //Verify effect exists in the effectPrefabList provided by user
        if (0 <= num && num < effectPrefabsList.Length)
        {
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
    }
        
    //Used by listener of songInt Event
    //Plays song given by input, turning off song that is currently playing and destroying 
    //If -1 is inputed, Stops playing all songs
    //If -2 is inputed, saves the song tag currently playing (can only save one song at a time)
    //If -3 is inputted, plays the saved song! (if none have been saved will mute songs)
    private void playSong(int num)
    {
        //Verify song exists in the songPrefabList provided by user,
        //and if input is -1
        if (-3 <= num && num < songPrefabsList.Length)
        {
            AudioObject song; //Holder for songs played or removed

            //Save a song in savedSong value
            if (num == -2)
            {
                song = songQueue.Dequeue(); //Remove song
                savedSong = song.tag;       //Save tag
                songQueue.Enqueue(song);    //Place song back
                return;                     //End code
            }
            //Play the saved Song!
            if (num == -3)
            {
                //Prevention for infinite loop (Impossible but percationart)
                if (savedSong == -3) { return; }
                playSong(savedSong);
                return;                     //End code
            }


            //Removing song, only if song is playing
            if (songQueue.Count > 0)
            {
                //Remove song previously playing, stop and destroy
                song = songQueue.Dequeue();
                //If the song is already playing and is called again, leave alone
                if (song.tag == num)
                {
                    songQueue.Enqueue(song);
                    return;
                }
                song.audioPrefab.GetComponent<AudioSource>().Stop();
                Destroy(song.audioPrefab);

                //Clears effectsPoolList (assummed some scene change occurs with song change)
                clearEffectPrefabs();
            }

            //If playing song, create new GameObject for song and add to the queue
            if (num >= 0)
            {
                song = new AudioObject(num, (GameObject)Instantiate(songPrefabsList[num], gameObject.transform));
                song.audioPrefab.GetComponent<AudioSource>().Play();
                songQueue.Enqueue(song);

                //If only one song is in queue, do not remove the song
                if (songQueue.Count == 1) { return; }
            }
        }
        else
        {
            Debug.Log("Song" + num + " does not exist in songPrefabList!");
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

}
