using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AI : MonoBehaviour
{
    #region Constans
    [SerializeField] private bool IsDebug = false;
    [SerializeField] private GameObject checkBox;
    
    private int cellMultiplicity;
    private List<Vector2> offsets = new List<Vector2> // Список оффсетов для проверки победы (Без умножения на cellMultiplicity)
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
    bool CheckWin(List<Vector2> setFigures, Vector2 figurePos)
    {
        return gameObject.GetComponent<TurnScript>().CheckWin(setFigures, figurePos);
    }

    bool CheckSame(List<Vector2> vectors, Vector2 nowVector)
    {
        return gameObject.GetComponent<TurnScript>().CheckSame(vectors, nowVector);
    }

    int CountLine(List<Vector2> figures, Vector2 nowPos) // Переделанная CheckWin из TurnScript
    {
        int maxCount = 0;

        for (int o = 0; o < offsets.Count; o++) // Проходимся по всем оффсетам
        {
            int count = 0;
            //var offset = new Vector2(offsets[o].x * cellMultiplicity, offsets[o].y * cellMultiplicity);

            while (CheckSame(figures, nowPos - offsets[o])) // Переходим в край обратный оффсету
            {
                nowPos -= offsets[o];
            }

            while (CheckSame(figures, nowPos + offsets[o])) // Считаем кол-во фигур в ряде начиная с крайнего
            {
                count++;
                nowPos += offsets[o];
            }

            if (count > maxCount) maxCount = count;
        }

        //Debug.Log(maxCount);
        return maxCount;
    }

    List<Vector2> FindEdge(List<Vector2> figuresWhoCheck, Vector2 nowPos, List<Vector2> figuresOtherPlayer) // figuresOtherPlayer Нужен для того, чтобы случайно не поставить фигуру в уже поставленное место
    {
        List<Vector2> edges = new List<Vector2>();
        int maxCount = 0; // Если вдруг будет выбор из линии из трёх и линии из четырех, то будем выбирать максимальную по длинне, и уже ей препятствовать

        for (int o = 0; o < offsets.Count; o++)
        {
            int count = 0;
            //var offset = new Vector2(offsets[o].x * cellMultiplicity, offsets[o].y * cellMultiplicity);
            Vector2 startPos = new Vector2();

            while (CheckSame(figuresWhoCheck, nowPos + offsets[o])) // Переходим в край обратный оффсету
            {
                nowPos += offsets[o];
            }
            startPos = nowPos;

            while (CheckSame(figuresWhoCheck, nowPos - offsets[o])) // Считаем кол-во фигур в ряде начиная с крайнего
            {
                count++;
                nowPos -= offsets[o];
            }

            if (count > maxCount)
            {
                edges.Clear(); // Обнуляем список, так как появился претендент получше
                maxCount = count;

                // Находим один из краев, куда можно поставить фигуру, и если ее уже нет в списках, то заносим
                var startEdge = startPos + offsets[o];
                if (!CheckSame(figuresWhoCheck, startEdge) && !CheckSame(figuresOtherPlayer, startEdge))
                {
                    edges.Add(startEdge);
                }

                var endEdge = nowPos - offsets[o];
                if (!CheckSame(figuresWhoCheck, endEdge) && !CheckSame(figuresOtherPlayer, endEdge))
                {
                    edges.Add(endEdge);
                }
            }


        }
        return edges;
    }
    #endregion

    #region Monobehaviour CallBack
    private void Awake()
    {
        cellMultiplicity = gameObject.GetComponent<TurnScript>().cellMultiplicity;

        // При загрузке проекта сразу умножаем оффсеты на cellMultiplicity, чтобы дальне не было недопониманий
        for (int i = 0; i < offsets.Count; i++)
        {
            offsets[i] = new Vector2(offsets[i].x * cellMultiplicity, offsets[i].y * cellMultiplicity);
            //Debug.Log(offsets[i]);
        }
    }
    #endregion

    public Vector2 AIMove(List<Vector2> playerSet, List<Vector2> AIset)
    {
        int maxScore = int.MinValue; // У лучшей клетки для хода будет макс кол-во очков
        Vector2 bestMove = new Vector2(); // Возвращаем лучший ход

        #region First move
        // Если у ИИ первый ход:
        if (AIset.Count == 0)
        {
            // Ставим первую фигуру в радиусе 10 клеток от начала координат
            var firstMove = new Vector2(Mathf.Round(Random.Range(-10, 10) / cellMultiplicity) * cellMultiplicity, Mathf.Round(Random.Range(-10, 10) / cellMultiplicity) * cellMultiplicity);

            while (CheckSame(playerSet, firstMove)) // Защита от постановки фигуры ии на фигуру игрока
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
            for (int o = 0; o < offsets.Count; o++) // Пробираемся по оффсетам у каждой клетки
            {
                int score = 0; // Очки в данный момент
                var tmpPos = AIset[i] + offsets[o]; // Отступаем от клетки на один
                List<Vector2> tmpAIset = new List<Vector2>(AIset); // ИМЕННО НОВЫЙ список, НЕ связанный с оригинальным
                

                if (!CheckSame(AIset, tmpPos) && !CheckSame(playerSet, tmpPos))
                {
                    tmpAIset.Add(tmpPos);

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
                        // Система примерно такая: Чем длиннее линия, тем нужно быстрее её продолжить
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
            var tmpMove = playerSet[i];

            for (int o = 0; o < offsets.Count; o++) 
            {
                var tmpOff = tmpMove + offsets[o];
                
                if (!CheckSame(AIset, tmpOff) && !CheckSame(playerSet, tmpOff))
                {
                    var tmpPlayerSet = new List<Vector2>(playerSet);
                    tmpPlayerSet.Add(tmpOff);

                    //Debug.Log("playerSet[i]: " + playerSet[i] + " | tmpOff: " + tmpOff + " => " + CheckWin(tmpPlayerSet, tmpOff));
                    if (CheckWin(tmpPlayerSet, tmpOff)) // Если игрок может следующим ходом победить
                    {
                        return tmpOff;
                    }
                }
            }
            
           if (CountLine(playerSet, tmpMove) >= 2) // Если линия из 3 фигур (игрок за 2 хода ее уже соберёт полностью, поэтому нам надо это предотвратить) ИМЕННО ТУТ ОПРАВДЫВАЕТСЯ ОДНО ИЗ ТРЕБОВАНИЙ КУРСОВОЙ :)
            {
                List<Vector2> edges = FindEdge(playerSet, tmpMove, AIset); // Находим список краев, куда можно поставить мешающую фигуру

                if (edges.Count > 0)
                {
                    //Debug.Log(edges.Count);
                    return edges[Random.Range(0, edges.Count)]; // Возвращаем рандомную из них  
                }
            }

            #endregion
           
        }
        return bestMove;
    }
}
