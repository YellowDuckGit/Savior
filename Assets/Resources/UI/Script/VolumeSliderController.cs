using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    public Slider volumeSlider;
    public List<AudioSource> gameAudioSource;

    private void Start()
    {
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

        for(int i = 0; i < transform.childCount-1; i++)
        {
            AudioSource audio = transform.GetChild(i).GetComponent<AudioSource>();
            if(audio != null)
            {
                gameAudioSource.Add(audio);
            }
        }
    }

    private void OnVolumeSliderChanged(float volume)
    {
        gameAudioSource.ForEach(a=>a.volume = volume);
    }
}
