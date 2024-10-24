﻿using System;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject keyTutorial;
    [SerializeField] private TutorialStep[] tutorialSteps;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private GameObject tutorialTextObject;
    
    private int tutorialStepIndex;
    
    private void Start()
    {
        tutorialTextObject.SetActive(false);
        foreach (TutorialStep step in tutorialSteps)
        {
            step.Hide();
            step.gameObject.SetActive(false);
        }
        tutorialStepIndex = 0;
        keyTutorial.SetActive(true);
        InputManager.Instance.OnInteract += InputManager_OnInteract;
        CustomerManager.Instance.DisableSpawning();
    }

    private void OnDestroy()
    {
        InputManager.Instance.OnInteract -= InputManager_OnInteract;
    }

    private void InputManager_OnInteract(object sender, EventArgs e)
    {
        keyTutorial.SetActive(false);
        InputManager.Instance.OnInteract -= InputManager_OnInteract;
        
        tutorialTextObject.SetActive(true);
        tutorialSteps[tutorialStepIndex].Show();
    }
    
    public void CompleteTutorialStep()
    {
        tutorialStepIndex++;
        if (tutorialStepIndex < tutorialSteps.Length)
        {
            tutorialSteps[tutorialStepIndex].Show();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void setTutorialText(string text)
    {
        tutorialText.text = text;
    }
}