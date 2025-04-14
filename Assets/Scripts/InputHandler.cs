using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    [Header("Figure under cursor")]
    [SerializeField][Range(0, 1)] private float alphaColorNow = 0.2f; // Прозрачность фигуры под курсором
    [SerializeField][Range(0, 20f)] private float speed = 0.1f; // Скорость передвижения этой фигуры

    private GameObject figureUnderCursor; // Фигура, которая сейчас отображается под курсором

    private void Awake()
    {
        // Singletone
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Инициализируем фигуру под курсором и раскрашиваем её
        figureUnderCursor = Instantiate(TurnScript.Instance.GetPrefabCurrentFigure());
        Color nowColor = figureUnderCursor.GetComponent<SpriteRenderer>().color;
        figureUnderCursor.GetComponent<SpriteRenderer>().color = new Color(nowColor.r, nowColor.g, nowColor.b, alphaColorNow);
    }

    private void Update()
    {
        // Не обрабатываем ходы, когда игрок не ходит или игра уже закончилась
        if (!TurnScript.Instance.IsPlayerTurn || TurnScript.Instance.IsWin) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 roundedPos = new Vector2(
                Mathf.Round(mousePos.x / TurnScript.Instance.CELL_MULTIPLICITY) * TurnScript.Instance.CELL_MULTIPLICITY,
                Mathf.Round(mousePos.y / TurnScript.Instance.CELL_MULTIPLICITY) * TurnScript.Instance.CELL_MULTIPLICITY
            );

            if (!IsMouseOnUI())
            {
                Debug.Log("Make move");
                TurnScript.Instance.MakeMove(roundedPos);
                UpdateFigureUnderCursor(TurnScript.Instance.GetPrefabCurrentFigure(1));
            }
        }

        // Обновление фигуры под курсором
        Vector2 cursorTarget = new Vector2(
            Mathf.Round(Camera.main.ScreenToWorldPoint(Input.mousePosition).x / TurnScript.Instance.CELL_MULTIPLICITY) * TurnScript.Instance.CELL_MULTIPLICITY,
            Mathf.Round(Camera.main.ScreenToWorldPoint(Input.mousePosition).y / TurnScript.Instance.CELL_MULTIPLICITY) * TurnScript.Instance.CELL_MULTIPLICITY
        );

        figureUnderCursor.transform.position = Vector2.Lerp(
            figureUnderCursor.transform.position,
            cursorTarget,
            speed * Time.deltaTime
        );

        // Обработка ИИ 
        TurnScript.Instance.HandleAITurn();
    }

    private bool IsMouseOnUI() // Проверка мыши на интерфейсе
    {
        // Запрашиваем эту информацию у Системы Событий
        return EventSystem.current.currentSelectedGameObject != null;
    }

    /// <summary>
    /// Обновление фигуры под курсором, при смене хода игрока
    /// </summary>
    /// <param name="newFigureUnderCursor">Новая фигура на замену старой</param>
    private void UpdateFigureUnderCursor(GameObject newFigureUnderCursor)
    {
        // Создаем фигуру
        newFigureUnderCursor = Instantiate(newFigureUnderCursor, figureUnderCursor.transform.position, Quaternion.identity) as GameObject;

        if (newFigureUnderCursor != null )
        {
            // Меняем цвет на полупрозрачный
            Color nowColor = newFigureUnderCursor.GetComponent<SpriteRenderer>().color;
            newFigureUnderCursor.GetComponent<SpriteRenderer>().color = new Color(nowColor.r, nowColor.g, nowColor.b, alphaColorNow);
        }

        Destroy(figureUnderCursor);
        figureUnderCursor = newFigureUnderCursor;
    }

    public void OffFigureUnderCursor()
    {
        Destroy(figureUnderCursor);
        figureUnderCursor = new GameObject();
    }
}
