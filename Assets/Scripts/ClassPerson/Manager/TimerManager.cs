using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fictology.UnityEditor;
using Fictology.UnityGenerator;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ClassPerson.Manager
{
    public partial class TimerManager: MonoBehaviour
    {
        public Image counterBackground;
        public TMP_Text countdownText;
        [Observable] [DisplayOnly] public int current = 15;
        [DisplayOnly] public bool shouldTick = true;
        public event Action<TimerManager> CountdownStart = (countdown => {});
        public event Action CountdownEnd = delegate { };

        public async void StartCountDown(int seconds)
        {
            CountdownStart.Invoke(this);
            current = seconds;
            countdownText.text = $"{current}";
            counterBackground.gameObject.SetActive(true);

            for (var time = seconds - 1; time > -1; time--)
            {
                if (!shouldTick) return;
                countdownText.text = $"{current}"; 
                await UniTask.WaitForSeconds(1);
                CurrentChanged(current, time);
                current = time;
            }
            CountdownEnd?.Invoke();
            StopCountdown();
            gameObject.SetActive(false);
        }

        public void StopCountdown()
        {
            shouldTick = false;
            counterBackground.gameObject.SetActive(false);
        }

    }
}