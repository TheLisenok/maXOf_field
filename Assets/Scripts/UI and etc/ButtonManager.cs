using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    #region Constans
    [SerializeField] private GameObject exitButton; // Кнопка выхода (для её отключения при использовании WebGL)
    [SerializeField] private Canvas pauseCanvas; // Меню паузы
    [SerializeField] private GameObject toggleIsFirstAI; // Галочка того, что первый ход совершает ИИ
    [SerializeField] private Animator defaultTransition; // Обычная анимация перехода между сценами 
    [SerializeField] private Animator exitTransition; // У выхода анимация своя
    [SerializeField] private float waitTime = 1f; // Время анимации
    #endregion


    #region public void
    public void PlayPlayer() // Кнопка начала игры. Игрок против Игрока
    {
        PlayerPrefs.SetInt("isAIGame", 0); // Сохраняем переменную isAIGame в реестре
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1)); // Переходим на следующую сцену
    }
    public void PlayAI() // Кнопка начала игры. Игрок против ИИ
    {
        PlayerPrefs.SetInt("isAIGame", 1);
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void IsFirstMoveAI() // Галочка того, что первым ходит ии
    {
        if (toggleIsFirstAI != null) // Если галочка задана
        {
            if (toggleIsFirstAI.GetComponent<Toggle>().isOn == true) // Первым ходит ИИ
            {
                PlayerPrefs.SetInt("IsFirstMoveAI", 1); // Сохраняем в реестр
            }
            else // Первым ходит Игрок
            {
                PlayerPrefs.SetInt("IsFirstMoveAI", 0);
            }
        }
    }

    public void Exit() // Кнопка выхода
    {
        // Запускаем соопроцес выхода
        StartCoroutine(ExitWithAnimation());
    }

    public void MainMenu() // Кнопка возврата в главное меню
    {
        // Загружаем прошлую сцену 
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex - 1));
    }
    public void Restart() // Кнопка рестарта
    {
        // Загружаем загруженную сцену заново
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex));
    }
    public void Pause() // Кнопка пауза/продолжить
    {
        // Переключаем канвас
        pauseCanvas.enabled = !pauseCanvas.enabled;
    }
    #endregion


    #region MonoBehaviour callback
    private void Awake() // При загрузке
    {
        if (pauseCanvas != null) // Если есть канвас паузы
        {
            pauseCanvas.enabled = false; // Отключаем его
        }

        if (exitButton != null && Application.platform == RuntimePlatform.WebGLPlayer) // Если есть кнопка выхода И игра запущена в БРАУЗЕРЕ
        {
            exitButton.SetActive(false); // Отключаем кнопку
        }

        if (!PlayerPrefs.HasKey("IsFirstMoveAI")) // Если в реестре нет значения, что первым ли ходит ИИ
        {
            PlayerPrefs.SetInt("IsFirstMoveAI", 0); // Устанавливаем на false, потому что на сцене по умолчанию она тоже отключена
        }

        if (toggleIsFirstAI != null) // Если галочка есть
        {
            // То задаём ей такое значение, какое есть в реестре
            if (PlayerPrefs.GetInt("IsFirstMoveAI") == 0) 
            {
                toggleIsFirstAI.GetComponent<Toggle>().isOn = false;
            }
            else
            {
                toggleIsFirstAI.GetComponent<Toggle>().isOn = true;
            }
        }
    }

    private void Update() // Нажатия кнопок на клавиатуре
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Эскейп
        {
            Pause();
        }
        if (Input.GetKeyDown(KeyCode.R)) // Английская R
        {
            Restart();
        }
    }
    #endregion

    // Процессы, запущенные параллельно основного скрипта
    #region IEnumerator
    IEnumerator LoadLevel(int levelIndex) // Загрузка сцены по её индексу
    {
        defaultTransition.SetTrigger("Start"); // Запускаем анимацию через триггер

        yield return new WaitForSeconds(waitTime); // Ждём когда анимация закончится

        SceneManager.LoadScene(levelIndex); // Загружаем уровень
    }
    IEnumerator ExitWithAnimation() // Выход из игры
    {
        exitTransition.SetTrigger("Start"); // Ставим триггер анимации

        yield return new WaitForSeconds(waitTime); // Ждём 

        Application.Quit(); // Закрываем игру

        
    }
    #endregion
}
