using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class TurnScript : MonoBehaviour
{
    #region Constans
    [Header("Output text")]
    public GameObject CrossTextWin;
    public GameObject ZeroTextWin;
    public TextMeshProUGUI moveIntText;

    [Header("Animations")]
    public Animator winAnim;

    [Header("Data for AI")]
    public List<Vector2> setCross = new List<Vector2>(); // Список с уже поставленными крестами
    public List<Vector2> setZero = new List<Vector2>(); // Список с уже поставленными кругами
    [SerializeField] private AI aiScript;
    private bool isAImove;

    [Header("Figures")]
    [SerializeField] private GameObject[] prefabs; // Префабы с фигурами
    [SerializeField] private Transform ObjectToSet;
    [SerializeField] private GameObject prefWinLine;

    [Header("Figure under cursor")]
    [SerializeField][Range(0, 1)] private float alphaColorNow = 0.2f; // Альфа цвет фигуры под курсором
    [SerializeField][Range(0, 20f)] private float speed = 0.1f;

    [Header("Field settings")]
    [SerializeField] public int cellMultiplicity = 2;
    // Объяснение, почему cellMultiplicity != 1: Юнити плохо работает как с числами с плавающей точкой, так и с большими. Поэтому размер 2 - это компромис, между плавностью работы камеры, движения фигуры и "бесконечностью поля"

    [Header("Game rules")]
    public int moveInt = 0; // Номер хода
    [SerializeField] private int countToWin = 5;
    private bool isAIGame;
    private bool AIfirstMove;

    [Header("Particle for set figures")]
    [SerializeField] private GameObject prefabPartc;
    [SerializeField] private Material[] materialPartc;
    [SerializeField] private Texture2D[] texturesPartc;
    [SerializeField] private float lifetimePart;
    [SerializeField] private float speedPart;

    [Header("Sounds")]
    [SerializeField] private AudioSource setSound;
    [SerializeField] private AudioSource winSound;

    [Header("Debug")]
    [SerializeField] private bool writeTextureForParticle = false;


    private bool isPlayerTurn;
    private bool isWin = false; // Проверка победы
    private GameObject figureNow; // Фигура, которая сейчас отображается под курсором
    private Vector2 startPosWL;
    private Vector2 endPosWL;

    private List<Vector2> offsets = new List<Vector2> // Список оффсетов для проверки победы (по часовой стрелке)
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
    public bool CheckSame(List<Vector2> vectors, Vector2 nowVector)
    {
        //Debug.Log(nowVector + ": " + (nowVector == vectors.Find(x => x == nowVector)));
        
        if (nowVector == Vector2.zero)
        {
            // Старая реазизация
            if (nowVector == Vector2.zero)
            {
                for (int i = 0; i < vectors.Count; i++)
                {
                    if (vectors[i] == nowVector)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        

        // НЕ РАБОТАЕТ НА НУЛЕВОЙ КООРДИНАТЕ
        return nowVector == vectors.Find(x => x == nowVector);
        
        
        
    }

    public bool CheckWin(List<Vector2> setFigures, Vector2 figurePos)
    {
        // Перебираем все оффсеты
        for (int i = 0; i < offsets.Count; i++)
        {
            var tmpPos = figurePos;
            var offset = offsets[i];
            int count = 1;


            // Переходим к крайней фигуре
            while (CheckSame(setFigures, tmpPos + offset))
            {
                tmpPos += offset;
            }

            startPosWL = tmpPos;

            // Считаем кол-во фигур в ряде начиная с крайнего
            while (CheckSame(setFigures, tmpPos - offset))
            {
                count++;
                tmpPos -= offset;
            }

            

            if (count >= countToWin)
            {
                endPosWL = tmpPos;

                return true;
                
                //Win(moveInt, startPosWL, endPosWL);
            } 
        }
        return false;
    }

    private void Win(int moveInt, Vector2 startPosWL, Vector2 endPosWL)
    {
        isWin = true;
        Destroy(figureNow);
        figureNow = Instantiate(new GameObject());

        GameObject winLine = Instantiate(prefWinLine);
        winLine.transform.position = (endPosWL + startPosWL) / 2;

        float lineLong = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(endPosWL.x - startPosWL.x), 2) + Mathf.Pow(Mathf.Abs(endPosWL.y - startPosWL.y), 2));
        winLine.transform.localScale = new Vector2(lineLong + 2f, winLine.transform.localScale.y);
        winLine.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan((endPosWL.y - startPosWL.y) / (endPosWL.x - startPosWL.x)) * 180 / Mathf.PI);

        winLine.GetComponent<SpriteRenderer>().color = prefabs[moveInt % prefabs.Length].GetComponent<SpriteRenderer>().color;

        winSound.Play();

        switch (moveInt % 2)
        {
            case 0: // Cross win
                CrossTextWin.SetActive(true);
                winAnim.SetTrigger("IsWin");
                break;
            case 1: // Zero win
                ZeroTextWin.SetActive(true);
                winAnim.SetTrigger("IsWin");
                break;
        }
    }

    private void Move(Vector2 figurePos, List<Vector2> playerFigures)
    {
        playerFigures.Add(figurePos);

        // Создаем фигуру на том месте, возле которого был курсор
        GameObject fig = Instantiate(prefabs[moveInt % prefabs.Length], figurePos, Quaternion.Euler(0, 0, 0));
        fig.transform.SetParent(ObjectToSet, false);

        // Проигрываем анимацию при постановке фигуры
        GameObject partc = Instantiate(prefabPartc, figurePos, Quaternion.Euler(0, 0, 0)); // Спавним систему частиц
        partc.GetComponent<ParticleSystem>().GetComponent<Renderer>().material = materialPartc[moveInt % 2]; 

        if (writeTextureForParticle) materialPartc[moveInt % 2].mainTexture = texturesPartc[moveInt % 2]; // Юнити отаказывается менять текстуру у материала в редакторе, в не зависимости от компьютера. Поиски в инете не помогли. Поэтому пришлось костылить и менять текстуру через код
        
        partc.GetComponent<ParticleSystem>().Play();
        Destroy(partc, lifetimePart);

        setSound.Play();


        if (CheckWin(playerFigures, figurePos))
        {
            Win(moveInt, startPosWL, endPosWL);
        }

        else
        {
            // Проверяем, есть ли чекпоинты для следующего хода (костыль!!!)
            gameObject.GetComponent<CheckpointManager>().CheckActiveButton(moveInt);

            moveInt++; // Переходим на следующий ход
            moveIntText.text = moveInt.ToString();

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

    private void AiMove()
    {
        isAImove = true;
        
        if (moveInt % 2 == 1)
        {
            Move(aiScript.AIMove(setCross, setZero), setZero);
        }
        else
        {
            Move(aiScript.AIMove(setZero, setCross), setCross);
        }

        isAImove = false;
    }

    private bool IsMouseOnUI()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return true;
        }
        else return false;
    }

    private bool intToBool(int value)
    {
        if (value == 1) return true;
        else return false;
    }
    #endregion

    #region Monobehaviour Callback
    private void Awake()
    {
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
            //Debug.Log(offsets[i]);
        }


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
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 figurePos = new Vector2(Mathf.Round(mousePos.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(mousePos.y / cellMultiplicity) * cellMultiplicity);

                // Условия для установки фигуры
                if (!CheckSame(setCross, figurePos) && !CheckSame(setZero, figurePos) && !IsMouseOnUI() && !isWin)
                {
                    if (moveInt % 2 == 0)
                    {
                        Move(figurePos, setCross);

                        if (isAIGame) isPlayerTurn = !isPlayerTurn;
                    }
                    else if (moveInt % 2 == 1)
                    {
                        Move(figurePos, setZero);

                        if (isAIGame) isPlayerTurn = !isPlayerTurn;
                    }
                }
            }
        }
        else if (!isWin && isAIGame)
        {
            AiMove();

            isPlayerTurn = !isPlayerTurn;
        }

        // Обновляем позицию фигуры под курсором https://www.cyberforum.ru/csharp-beginners/thread1449949.html
        var pos = new Vector2(Mathf.Round(mousePos.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(mousePos.y / cellMultiplicity) * cellMultiplicity);
        figureNow.transform.position = Vector2.Lerp(figureNow.transform.position, pos, speed * Time.deltaTime);
    }
    #endregion
}
