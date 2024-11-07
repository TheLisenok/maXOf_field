using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class TurnScript : MonoBehaviour
{
    #region Constans
    [Header("Output text")] // Тексты выводящиеся на сцену
    public GameObject CrossTextWin;
    public GameObject ZeroTextWin;
    public TextMeshProUGUI moveIntText;

    [Header("Animations")]
    public Animator winAnim; // Анимация интерфейса при победе

    [Header("Data for AI")]
    public List<Vector2> setCross = new List<Vector2>(); // Список с уже поставленными крестами
    public List<Vector2> setZero = new List<Vector2>(); // Список с уже поставленными кругами
    [SerializeField] private AI aiScript; // Скрипт ИИ
    //private bool isAImove; // Совершает ли ИИ ход в данный момент

    [Header("Figures")]
    [SerializeField] private GameObject[] prefabs; // Префабы с фигурами
    [SerializeField] private Transform ObjectToSet; // Родительский объект, в который входят поставленный фигуры
    [SerializeField] private GameObject prefWinLine; // Линия, зачёркивающая победный ряд

    [Header("Figure under cursor")]
    [SerializeField][Range(0, 1)] private float alphaColorNow = 0.2f; // Прозрачность фигуры под курсором
    [SerializeField][Range(0, 20f)] private float speed = 0.1f; // Скорость передвижения этой фигуры

    [Header("Field settings")]
    [SerializeField] public int cellMultiplicity = 2; // Масштаб клетки
    // Объяснение, почему cellMultiplicity != 1: Юнити плохо работает как с числами с плавающей точкой, так и с большими. Поэтому размер 2 - это компромис, между плавностью работы камеры, движения фигуры и "бесконечностью поля"

    [Header("Game rules")]
    public int moveInt = 0; // Номер хода
    [SerializeField] private int countToWin = 5; // Количество фигур в ряде для победы
    private bool isAIGame; // Нужен ли вторым игроком ИИ
    private bool AIfirstMove; // Делает ли ИИ первый ход (при игре с ИИ естественно)

    [Header("Particle for set figures")]
    [SerializeField] private GameObject prefabPartc; // Сам префаб с партиклами
    [SerializeField] private Material[] materialPartc; // Материал
    [SerializeField] private Texture2D[] texturesPartc; // Текстура материала
    [SerializeField] private float lifetimePart; // Сколько живёт партикл после постановки
    [SerializeField] private float speedPart; // Скорость партиклов

    [Header("Sounds")]
    [SerializeField] private AudioSource setSound; // Фигуру поставили
    [SerializeField] private AudioSource winSound; // Победа

    [Header("Debug")]
    [SerializeField] private bool writeTextureForParticle = false; // Нужно ли записывать текстуру для материала. По какой-то причине возможность менять текстуру через Editor стана невозможной, лазанье в настройках и в интернете не помогло


    private bool isPlayerTurn; // Ходит ли игрок
    private bool isWin = false; // Проверка победы
    private GameObject figureNow; // Фигура, которая сейчас отображается под курсором
    private Vector2 startPosWL; // Стартовая            позиции       победы
    private Vector2 endPosWL;   //           и конечная         линии

    private List<Vector2> offsets = new List<Vector2> // Список оффсетов для проверки победы
    {
        new Vector2(-1, 1),
        new Vector2(0, 1),
        new Vector2(1, 1),
        new Vector2(1, 0),
        new Vector2(1, -1),
        new Vector2(0, -1),
        new Vector2(-1, -1),
        new Vector2(-1, 0)
    };
    #endregion

    #region Voids
    public bool CheckWin(List<Vector2> setFigures, Vector2 figurePos)
    {
        // Перебираем все оффсеты
        for (int i = 0; i < offsets.Count; i++)
        {
            var tmpPos = figurePos;
            var offset = offsets[i];
            int count = 1;

            // Переходим к крайней фигуре
            while (setFigures.Contains(tmpPos + offset)) 
            {
                tmpPos += offset;
            }

            startPosWL = tmpPos; // Задаём начальную позицию победной линии сразу

            // Считаем кол-во фигур в ряде начиная с крайнего
            while (setFigures.Contains(tmpPos - offset))
            {
                count++;
                tmpPos -= offset;
            }

            // Если составлена линия из 5 фигур
            if (count >= countToWin)
            {
                endPosWL = tmpPos; // Задаём позицию конца победной линии

                return true;
            } 
        }
        return false;
    }

    // Список действий при победе
    private void Win(int moveInt, Vector2 startPosWL, Vector2 endPosWL)
    {
        isWin = true;
        Destroy(figureNow); // Удаляем фигуру под курсором
        figureNow = Instantiate(new GameObject()); // и заменяем её пустышкой


        // Действия с победной линией
        GameObject winLine = Instantiate(prefWinLine); // Спавним её на поле
        winLine.transform.position = (endPosWL + startPosWL) / 2; // Переносим на среднюю позицию между началом и концом
        
        // Задаём длинну линии
        float lineLong = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(endPosWL.x - startPosWL.x), 2) + Mathf.Pow(Mathf.Abs(endPosWL.y - startPosWL.y), 2));
        // Задаём размер по Х оси (длинна линии)
        winLine.transform.localScale = new Vector2(lineLong + 2f, winLine.transform.localScale.y);
        // "Наклоняем" линию под нужным углом
        winLine.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan((endPosWL.y - startPosWL.y) / (endPosWL.x - startPosWL.x)) * 180 / Mathf.PI);
        // Задаём ей цвет той фигуры, которая победила
        winLine.GetComponent<SpriteRenderer>().color = prefabs[moveInt % prefabs.Length].GetComponent<SpriteRenderer>().color;
  

        // Проигрываем звук победы
        winSound.Play();

        switch (moveInt % 2)
        {
            case 0: // Cross win
                CrossTextWin.SetActive(true); // Включаем текст победы крестиков
                winAnim.SetTrigger("IsWin"); // Запускаем анимацию победного окна
                break;
            case 1: // Zero win
                ZeroTextWin.SetActive(true); // Включаем текст победы ноликов
                winAnim.SetTrigger("IsWin");
                break;
        }
    }

    private void Move(Vector2 figurePos, List<Vector2> listFigures) // Совершение ходов
    {
        listFigures.Add(figurePos); // Добавляем координату в список

        // Создаем фигуру на том месте, возле которого был курсор
        GameObject fig = Instantiate(prefabs[moveInt % prefabs.Length], figurePos, Quaternion.Euler(0, 0, 0));
        fig.transform.SetParent(ObjectToSet, false);

        // Проигрываем анимацию постановки фигуры
        GameObject partc = Instantiate(prefabPartc, figurePos, Quaternion.Euler(0, 0, 0)); // Спавним систему частиц
        partc.GetComponent<ParticleSystem>().GetComponent<Renderer>().material = materialPartc[moveInt % 2]; // Задаём материал соответственно фигуре

        if (writeTextureForParticle) materialPartc[moveInt % 2].mainTexture = texturesPartc[moveInt % 2]; // Юнити отаказывается менять текстуру у материала в редакторе, в не зависимости от компьютера. Поиски в инете не помогли. Поэтому пришлось костылить и менять текстуру через код (уже писал в начале)
        
        partc.GetComponent<ParticleSystem>().Play(); // Запускаем работу системы частиц
        Destroy(partc, lifetimePart); // Удаляем объект с частицами через время жизни

        setSound.Play(); // Проигрываем звук постановки фигуры

        // Если ход победный
        if (CheckWin(listFigures, figurePos))
        {
            // Запускаем процедуру действий при победе
            Win(moveInt, startPosWL, endPosWL);
        }

        else
        {
            moveInt++; // Переходим на следующий ход
            moveIntText.text = moveInt.ToString(); // Обновляем число хода в тексте

            // Проверяем, есть ли чекпоинты для следующего хода (костыль!!!)
            gameObject.GetComponent<CheckpointManager>().CheckActiveButton(moveInt);

            // Удаляем и создаем новую фигуру под курсором
            Destroy(figureNow);
            UpdateFigureNow();
        }
    }

    private void UpdateFigureNow() // Обновление фигуры под курсором, при смене хода игрока
    {
        figureNow = Instantiate(prefabs[moveInt % prefabs.Length], figureNow.transform.position, Quaternion.identity) as GameObject; // Создаем фигуру

        // Меняем цвет на полупрозрачный
        Color nowColor = figureNow.GetComponent<SpriteRenderer>().color;
        figureNow.GetComponent<SpriteRenderer>().color = new Color(nowColor.r, nowColor.g, nowColor.b, alphaColorNow);
    }

    private void AiMove() // Ход ИИ
    {
        // Запрашиваем у класса AI лучший ход и совершаем обычных ход с ним
        if (moveInt % 2 == 1)
        {
            Move(aiScript.AIMove(setCross, setZero), setZero);
        }
        else
        {
            Move(aiScript.AIMove(setZero, setCross), setCross);
        }
    }

    private bool IsMouseOnUI() // Проверка мыши на интерфейсе
    {
        // Запрашиваем эту информацию у Системы Событий
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return true;
        }
        else return false;
    }

    private bool intToBool(int value) // Делаем из числа bool, нужно для системы сохранений
    {
        if (value == 1) return true;
        else return false;
    }
    #endregion

    #region Monobehaviour Callback
    private void Awake() // При загрузке
    {
        // Узнаём из системы сохранений какой тип игры будет
        isAIGame = intToBool(PlayerPrefs.GetInt("isAIGame")); 
        if (isAIGame)
        {
            AIfirstMove = intToBool(PlayerPrefs.GetInt("IsFirstMoveAI"));
        }
        else
        {
            AIfirstMove = false;
        }
        isPlayerTurn = !AIfirstMove;

        // При загрузке проекта сразу умножаем оффсеты на cellMultiplicity, чтобы дальне не было недопониманий
        for (int i = 0; i < offsets.Count; i++)
        {
            offsets[i] = new Vector2(offsets[i].x * cellMultiplicity, offsets[i].y * cellMultiplicity);
        }

        // Инициализируем фигуру под курсором и раскрашиваем её
        figureNow = Instantiate(prefabs[moveInt % prefabs.Length]);
        Color nowColor = figureNow.GetComponent<SpriteRenderer>().color;
        figureNow.GetComponent<SpriteRenderer>().color = new Color(nowColor.r, nowColor.g, nowColor.b, alphaColorNow);

        moveIntText.text = moveInt.ToString();
    }

    private void Update()
    {
        // Переводим позицию мыши с экрана на игровое поле
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Ходит игрок
        if (isPlayerTurn)
        {
            if (Input.GetMouseButtonDown(0)) // Нажата ЛКМ
            {
                // Округляем позицию
                Vector2 figurePos = new Vector2(Mathf.Round(mousePos.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(mousePos.y / cellMultiplicity) * cellMultiplicity);

                // Условия для установки фигуры (фигура должна быть не в списках, курсор не на интерфейсе, победа не наступила)
                if (!setCross.Contains(figurePos) && !setZero.Contains(figurePos) && !IsMouseOnUI() && !isWin) 
                {
                    // Ходит Крестик
                    if (moveInt % 2 == 0)
                    {
                        Move(figurePos, setCross);

                        // Если игра с ИИ, запрещаем игроку делать следующий ход
                        if (isAIGame) isPlayerTurn = !isPlayerTurn;
                    }
                    // Ходит Нолик
                    else if (moveInt % 2 == 1)
                    {
                        Move(figurePos, setZero);

                        // Если игра с ИИ, запрещаем игроку делать следующий ход
                        if (isAIGame) isPlayerTurn = !isPlayerTurn;
                    }
                }
            }
        }
        // Ходит ИИ
        else if (!isWin && isAIGame)
        {
            AiMove();

            isPlayerTurn = !isPlayerTurn;
        }

        // Обновляем позицию фигуры под курсором https://www.cyberforum.ru/csharp-beginners/thread1449949.html (Ответ от kolorotur)
        var pos = new Vector2(Mathf.Round(mousePos.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(mousePos.y / cellMultiplicity) * cellMultiplicity);
        figureNow.transform.position = Vector2.Lerp(figureNow.transform.position, pos, speed * Time.deltaTime);
    }
    #endregion
}
