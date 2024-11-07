using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AI : MonoBehaviour
{
    #region Constans
    [Header("Debug")]
    [SerializeField] private bool IsDebug = false; // Отображает данные для дебага в консоли
    [SerializeField] private GameObject checkBox; // Ставится в клетку, если её проверяет ИИ
    
    private int cellMultiplicity; // Масштаб клетки
    private List<Vector2> offsets = new List<Vector2> // Список офсетов для проверки (Без умножения на cellMultiplicity)
    {
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(1, 1),
        new Vector2(1, 0),
        new Vector2(1, -1),
        new Vector2(-1, -1),
        new Vector2(-1, 1),
        new Vector2(-1, 0)
    };
    #endregion

    #region Functions
    bool CheckWin(List<Vector2> setFigures, Vector2 figurePos) // Проверка победы наследуется из TurnScript
    {
        return gameObject.GetComponent<TurnScript>().CheckWin(setFigures, figurePos);
    }

    int CountLine(List<Vector2> figures, Vector2 nowPos) // Считает макс длину линии из позиции (Переделанная CheckWin из TurnScript)
    {
        int maxCount = 0; // Макс длинна

        for (int o = 0; o < offsets.Count; o++) // Проходимся по всем офсетам
        {
            int count = 0;

            while (figures.Contains(nowPos - offsets[o])) // Переходим в край обратный офсету
            {
                nowPos -= offsets[o];
            }

            while (figures.Contains(nowPos + offsets[o])) // Считаем кол-во фигур в ряде начиная с крайнего
            {
                count++; // Считаем длину
                nowPos += offsets[o];
            }

            // Находим максимальную
            if (count > maxCount) maxCount = count;
        }

        // Возвращаем максимальную длину
        return maxCount;
    }

    
    List<Vector2> FindEdge(List<Vector2> figuresWhoCheck, Vector2 nowPos, List<Vector2> figuresOtherPlayer) // Находит край на линии фигур (figuresOtherPlayer Нужен для того, чтобы случайно не поставить фигуру в уже поставленное место)
    {
        List<Vector2> edges = new List<Vector2>();
        int maxCount = 0; // Если вдруг будет выбор из линии из трёх и линии из четырех, то будем выбирать максимальную по длине, и уже ей препятствовать

        for (int o = 0; o < offsets.Count; o++)
        {
            int count = 0;
            Vector2 startPos;

            while (figuresWhoCheck.Contains(nowPos + offsets[o])) // Переходим в край обратный офсету
            {
                nowPos += offsets[o];
            }
            startPos = nowPos;

            while (figuresWhoCheck.Contains(nowPos - offsets[o])) // Считаем кол-во фигур в ряде начиная с крайнего
            {
                count++;
                nowPos -= offsets[o];
            }

            if (count > maxCount) // Нынешняя длинна больше прошлого максимальной
            {
                edges.Clear(); // Обнуляем список ходов, так как появился претендент получше
                maxCount = count; 

                // Находим один из краев, куда можно поставить фигуру, и если ее уже нет в списках, то заносим
                var startEdge = startPos + offsets[o]; // От стартовой + отступ
                if (!figuresWhoCheck.Contains(startEdge) && !figuresOtherPlayer.Contains(startEdge)) // Если её нет в списках
                {
                    edges.Add(startEdge); // то заносим
                }

                var endEdge = nowPos - offsets[o]; // От конечной - отступ
                if (!figuresWhoCheck.Contains(endEdge) && !figuresOtherPlayer.Contains(endEdge)) // Если её нет в списках
                {
                    edges.Add(endEdge); // то заносим
                }
            }
        }
        return edges;
    }
    #endregion

    #region Monobehaviour CallBack
    private void Awake() // Загрузка сцены
    {
        cellMultiplicity = gameObject.GetComponent<TurnScript>().cellMultiplicity; // Узнаём масштаб клетки

        // При загрузке проекта сразу умножаем офсеты на cellMultiplicity, чтобы дальне не было недопониманий
        for (int i = 0; i < offsets.Count; i++)
        {
            offsets[i] = new Vector2(offsets[i].x * cellMultiplicity, offsets[i].y * cellMultiplicity);
        }
    }
    #endregion

    public Vector2 AIMove(List<Vector2> playerSet, List<Vector2> AIset) // ОСНОВНАЯ функция с алгоритмом нахождения лучшего хода
    {
        int maxScore = int.MinValue; // У лучшей клетки для хода будет макс кол-во очков
        Vector2 bestMove = new Vector2(); // Возвращаем лучший ход

        #region First move
        // Если у ИИ первый ход:
        if (AIset.Count == 0)
        {
            // Ставим первую фигуру в радиусе 10 клеток от начала координат
            var firstMove = new Vector2(Mathf.Round(Random.Range(-10, 10) / cellMultiplicity) * cellMultiplicity, Mathf.Round(Random.Range(-10, 10) / cellMultiplicity) * cellMultiplicity);

            while (playerSet.Contains(firstMove)) // Защита от постановки фигуры ии на фигуру игрока
            {
                firstMove = new Vector2(Mathf.Round(Random.Range(-10, 10) / cellMultiplicity) * cellMultiplicity, Mathf.Round(Random.Range(-10, 10) / cellMultiplicity) * cellMultiplicity);
            }

            return firstMove;
        }
        #endregion

        // ИИ строит свою структуру; Низкий приоритет, потому что сначала нужно мешать игроку, а потом уже ходить для себя
        #region AI move for Yourself (low priority)
        for (int i = 0; i < AIset.Count; i++) // Пробираемся по всем ходам ии
        {
            for (int o = 0; o < offsets.Count; o++) // Пробираемся по офсетам у каждой клетки
            {
                int score = 0; // Очки в данный момент
                var tmpPos = AIset[i] + offsets[o]; // Отступаем от клетки на один
                List<Vector2> tmpAIset = new List<Vector2>(AIset); // ИМЕННО НОВЫЙ список, НЕ связанный с оригинальным, чтобы при каком-то изменении случайно не задеть основной список           

                // Если отступа нет в списках
                if (!AIset.Contains(tmpPos) && !playerSet.Contains(tmpPos))
                {
                    tmpAIset.Add(tmpPos); // Временно добавляем 

                    //Дебаг
                    if (IsDebug)
                    {
                        Instantiate(checkBox, tmpPos, Quaternion.identity);
                    }


                    if (CheckWin(AIset, tmpPos)) // ГАРАНТИРОВАННАЯ ПОБЕДА НА ХОДЕ, поэтому дальше проверять нет смысла
                    {
                        return tmpPos;
                    }
                    else // Победы нет
                    {
                        // Система аля Минимакс такая: ИИ строит пытается строить линию длиннее, как только сможет, пока игрок не начал побеждать
                        score = CountLine(AIset, tmpPos);

                        if (score > maxScore)
                        {
                            maxScore = score;
                            bestMove = tmpPos;
                        }
                    }
                }
            }
        }
        #endregion

        // ИИ мешает игроку; Высокий приоритет, чтобы игрок не выиграл
        #region AI messes player (hight priority)

        // Перебираем клетки Игрока
        for (int i = 0; i < playerSet.Count; i++)
        {
            var tmpMove = playerSet[i]; // Выбираем позицию

            for (int o = 0; o < offsets.Count; o++) 
            {
                var tmpOff = tmpMove + offsets[o]; // Отступаем от позиции
                
                // Если координаты нет в списках
                if (!AIset.Contains(tmpOff) && !playerSet.Contains(tmpOff))
                {
                    // Заносим фигуру во временный список
                    var tmpPlayerSet = new List<Vector2>(playerSet);
                    tmpPlayerSet.Add(tmpOff);

                    if (CheckWin(tmpPlayerSet, tmpOff)) // Если игрок может следующим ходом победить
                    {
                        return tmpOff; // Препятствуем ему, совершая ход в эту клетку 
                    }
                }
            }
            
           if (CountLine(playerSet, tmpMove) >= 2) // Если линия из 3 фигур (игрок за 2 хода ее уже соберёт полностью, поэтому нам надо это предотвратить) ИМЕННО ТУТ ОПРАВДЫВАЕТСЯ ОДНО ИЗ ТРЕБОВАНИЙ КУРСОВОЙ :)
            {
                List<Vector2> edges = FindEdge(playerSet, tmpMove, AIset); // Находим список краев, куда можно поставить мешающую фигуру

                if (edges.Count > 0)
                {
                    return edges[Random.Range(0, edges.Count)]; // Возвращаем рандомную из них  
                }
            }

            #endregion
           
        }
        // Если return выше не сработали, то возвращаем лучший ход
        return bestMove;
    }
}
