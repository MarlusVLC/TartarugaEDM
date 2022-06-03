﻿using System;
using System.Collections.Generic;
using Code.Utilities;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Code
{
    public class GameplayManager : MonoSingleton<GameplayManager>
    {
        [SerializeField] private GameObject playerGameObject;
        [SerializeField] private TransitionScreen victoryTransitionScreen;    

        private List<GameEnder> gameEnders;
        private bool CanPause = true;

        private float defaultTime = 1f;
        private bool isPaused;

        public Action OnGamePause;
        public Action OnGameResume;

        public TransitionScreen VictoryTransitionScreen => victoryTransitionScreen;
        public GameObject PlayerGameObject => playerGameObject;

        private void Awake()
        {
            gameEnders = MonoUtilities.FindGameObjects<GameEnder>();
        }
        
        private void OnEnable()
        {
            gameEnders.ForEach(gE => gE.OnCriticalPointReached += DisablePause);
            
            Time.timeScale = defaultTime;

        }
        
        private void OnDisable()
        {
            gameEnders.ForEach(gE => gE.OnCriticalPointReached += DisablePause);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && CanPause)
            {
                TogglePause();
            }
        }

        private bool IsPaused
        {
            get => isPaused;
            set
            {
                isPaused = value;
                if (isPaused)  OnGamePause?.Invoke();
                else OnGameResume?.Invoke();
            }
        } 

        public void TogglePause()
        {
            if (!IsPaused)
            {
                defaultTime = Time.timeScale;
                Time.timeScale = 0f;
                IsPaused = true;
            }
            else
            {
                Time.timeScale = defaultTime;
                IsPaused = false;
            }
        }

        public void QuitGame()
        {
            // AppHelper.Quit();
            SceneManager.LoadScene(0);
        }

        public void DisablePause()
        {
            CanPause = false;
        }
    }
}