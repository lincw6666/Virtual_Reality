using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineSoundController : MonoBehaviour
{
    public AudioClip clip;

    AudioSource sound;
    bool explosion = false;

    // Start is called before the first frame update
    void Start()
    {
        sound = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!explosion && GameController.GetGameSTAT() == GameController.GAME_STAT.GameOver) {
            sound.Stop();
            sound.clip = clip;
            sound.loop = false;
            sound.Play();
            explosion = true;
        }
    }
}
