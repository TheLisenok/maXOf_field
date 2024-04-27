using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    #region Constans
    [SerializeField] private GameObject exitButton;
    [SerializeField] private Canvas pauseCanvas = null;
    [SerializeField] private GameObject toggleIsfirstAI;
    [SerializeField] private Animator playTransition;
    [SerializeField] private Animator exitTransition;
    [SerializeField] private float waitTime = 1f;
    #endregion


    #region public void
    public void PlayPlayer()
    {
        PlayerPrefs.SetInt("isAIGame", 0);
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }
    public void PlayAI()
    {
        PlayerPrefs.SetInt("isAIGame", 1);
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void IsFirstMoveAI()
    {
        if (toggleIsfirstAI != null)
        {
            if (toggleIsfirstAI.GetComponent<Toggle>().isOn == true)
            {
                PlayerPrefs.SetInt("IsFirstMoveAI", 1);
            }
            else
            {
                PlayerPrefs.SetInt("IsFirstMoveAI", 0);
            }
        }

        Debug.Log("IsFirstMoveAI: " + PlayerPrefs.GetInt("IsFirstMoveAI"));
    }

    public void Exit()
    {
        StartCoroutine(ExitWithAnimation());
    }

    public void Pause()
    {
        pauseCanvas.enabled = !pauseCanvas.enabled;
    }

    public void MainMenu()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex - 1));
    }
    public void Restart()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex));
    }
    #endregion


    #region MonoBehaviour callback
    private void Awake()
    {
        if (pauseCanvas != null)
        {
            pauseCanvas.enabled = false;
        }

        if (exitButton != null && Application.platform == RuntimePlatform.WebGLPlayer)
        {
            exitButton.SetActive(false);
        }

        if (!PlayerPrefs.HasKey("IsFirstMoveAI"))
        {
            PlayerPrefs.SetInt("IsFirstMoveAI", 0);
        }

        if (toggleIsfirstAI != null)
        {
            if (PlayerPrefs.GetInt("IsFirstMoveAI") == 0)
            {
                toggleIsfirstAI.GetComponent<Toggle>().isOn = false;
            }
            else
            {
                toggleIsfirstAI.GetComponent<Toggle>().isOn = true;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }
    #endregion


    #region IEnumerator
    IEnumerator LoadLevel(int levelIndex)
    {
        playTransition.SetTrigger("Start"); // Запускаем анимацию через триггер

        yield return new WaitForSeconds(waitTime); // Ждём когда анимация закончится

        SceneManager.LoadScene(levelIndex); // Загружаем уровень
    }
    IEnumerator ExitWithAnimation()
    {
        exitTransition.SetTrigger("Start");

        yield return new WaitForSeconds(waitTime);

        Application.Quit();

        
    }
    #endregion
}
