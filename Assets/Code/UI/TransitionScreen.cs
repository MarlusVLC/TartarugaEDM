﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code;
using Code.DeathMessages;
using DG.Tweening;
using Ez;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class TransitionScreen : MonoBehaviour
    {
        [SerializeField] private GameObject gameEnder;
        [SerializeField] private float fadeInTime = 1.0f;
        [SerializeField] private float delayTime = 0.5f;
        [SerializeField] private float fadeOutTime = 1.0f;

        [Header("Message")]
        [SerializeField] private DeathMessageManager deathMessages;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float textFadeInTime;
        [Space]
        [SerializeField] private AudioClip deathTrack;
        [SerializeField] private AudioSource musicPlayer;

        private Image _blackoutScreen;

        public GameObject GameEnder
        {
            get => gameEnder;
            set => gameEnder = value;
        }

        private void Awake()
        {
            _blackoutScreen = GetComponent<Image>();
            deathMessages.TryGetMessagesFromResourceFolder();
        }

        void Start()
        {
            if (gameEnder != null)
            {
                gameEnder.GetComponent<GameEnder>().OnCriticalPointReached += FadeInCoroutine;
            }
            
            if (musicPlayer == null)
            {
                var musicPlayerGameObject = GameplayManager.Instance.MusicPlayerGameObject;
                if (musicPlayerGameObject != null)
                {
                    musicPlayer = musicPlayerGameObject.GetComponent<AudioSource>();
                    if (musicPlayer == null)
                    {
                        Debug.LogWarning("No valid audioSource has been found!");
                    }
                }

            }
                
            _blackoutScreen.color = new Color(_blackoutScreen.color.r, _blackoutScreen.color.g, _blackoutScreen.color.b, 0);
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 0);
            StartCoroutine(BackgroundFadeOut(true));
            
        }

        private void OnDisable()
        {
            if (gameEnder != null)
            {
                gameEnder.GetComponent<GameEnder>().OnCriticalPointReached -= FadeInCoroutine;
            }
        }
        
        private IEnumerator BackgroundFadeIn(CauseOfDeath causeOfDeath)
        {
            TryPlayDeathTrack();
            
            Tween fadeTween = _blackoutScreen.DOFade(1, fadeInTime);
            yield return new WaitForSeconds(fadeInTime+delayTime);
            if (causeOfDeath == CauseOfDeath.Default && !GameplayManager.Instance.IsTheLastScene())
            {
                GameplayManager.Instance.GoToNextScene();
            }
            else
            {
                StartCoroutine(MessageFadeCycle(causeOfDeath));
            }
        }
        private void FadeInCoroutine(CauseOfDeath causeOfDeath) => StartCoroutine(BackgroundFadeIn(causeOfDeath));

        private IEnumerator BackgroundFadeOut(bool startOpaque = false)
        {
            if (startOpaque)
            {
                Color newColour = _blackoutScreen.color;
                newColour.a = 1;
                _blackoutScreen.color = newColour;
            }
            _blackoutScreen.DOFade(0, fadeOutTime+delayTime);
            yield return null;
        }
        
        private async void BackgroundFadeIn_Async()
        {
            Tween fadeTween = _blackoutScreen.DOFade(1, fadeInTime);
            await fadeTween.AsyncWaitForCompletion();
        }

        private IEnumerator MessageFadeCycle(CauseOfDeath causeOfDeath)
        {
            DeathMessageSO deathMessage = deathMessages.GetMessage(causeOfDeath);
            var fadeCompletionYield = new WaitForSeconds(textFadeInTime);
            for (int i = 0; i < deathMessage.messageParts.Length; i++)
            {
                messageText.text = deathMessage.messageParts[i];
                Tween fadeTween = messageText.DOFade(1, textFadeInTime);
                yield return new WaitForSeconds(textFadeInTime + deathMessage.messageDurations[i]);
                fadeTween = messageText.DOFade(0, textFadeInTime);
                yield return fadeCompletionYield;
            }

            if (causeOfDeath == CauseOfDeath.Default)
            {
                GameplayManager.Instance.GoToNextScene();
            }
            else
            {
                GameplayManager.Instance.RestartGame();
            }
        }

        private void TryPlayDeathTrack()
        {
            if (musicPlayer == null) return;
            
            musicPlayer.loop = false;
            musicPlayer.clip = deathTrack;
            musicPlayer.Play();
        }
    }
}