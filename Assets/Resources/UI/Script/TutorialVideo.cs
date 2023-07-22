using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace Assets.Resources.UI.Script
{
    public class TutorialVideo : MonoBehaviour
    {
        private VideoPlayer player;

        private void Start()
        {
            player = GetComponent<VideoPlayer>();
        }

        public void ContinueVid()
        {
            player.Play();
        }

        public void StopVid()
        {
            player.Stop();
        }
    }
}
