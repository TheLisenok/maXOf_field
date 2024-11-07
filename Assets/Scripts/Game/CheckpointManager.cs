using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointManager : MonoBehaviour
{
    // Два списка нужны для логического и визуального отображения чекпоинта
    // Иначе, если убрать список с позициями, то становится очень сложно и ресурсозатратно обрабатывать позицию через игровые объекты
    // Гарантируется, что чекпоинты будут добавляться сразу в 2 списка и их порядковые номера будут одинаково изменятся  
    // Списки списков нужны для того, чтобы хранить информацию о чекпоинтов для каждого игрока отдельно

    [SerializeField] private List<List<GameObject>> checkpointsGameObject = new List<List<GameObject>>(); // Список списков объектов на сцене
    [SerializeField] private List<List<Vector2>> checkpointPosList = new List<List<Vector2>>(); // Список списков позиций

    [SerializeField] private GameObject[] checkpointObject; // Префабы чекпоинта, для каждого игрока свои
    [SerializeField] private GameObject targetForCamera; // Объект, за которым следует камера (см. скрипт CameraMove)
    [SerializeField] private GameObject[] buttons; // Кнопки для перехода между чекпоинтами

    private int cellMultiplicity; // Масштаб клетки
    private List<int> countCheck = new List<int>{0, 0}; // Позиция последнего чекпоинта, которые переключал игрок (для каждого игрока свой)
    private TurnScript turnScript; // Основной скрипт совершения ходов 

    #region Voids for button
    public void GoForward() // Переход вперёд по списку чекпоинтов
    {
        int moveInt = turnScript.moveInt; // Берём номер хода из основного скрипта 

        if (checkpointsGameObject[moveInt % 2].Count > 0) // Если чекпоинты есть в списке
        {
            if (countCheck[moveInt % 2] + 1 > checkpointsGameObject[moveInt % 2].Count - 1) // Если позиция последнего чекпоинта превышает список
            {
                countCheck[moveInt % 2] = 0; // Обнуляем позицию
            }
            else // Если всё нормально
            {
                countCheck[moveInt % 2]++; // Переходим вперёд по чекпоинтам
            }

            // Переносим цель камеры на позицию чекпоинта
            targetForCamera.transform.position = checkpointsGameObject[moveInt % 2][countCheck[moveInt % 2]].transform.position;


        }
    }
    public void GoBack() // Назад по чекпоинтам
    {
        int moveInt = turnScript.moveInt; // Номер хода из основного скрипта

        if (checkpointsGameObject[moveInt % 2].Count > 0) // Если чекпоинты есть
        {
            if (countCheck[moveInt % 2] - 1 < 0) // Если позиция последнего чекпоинта меньше нулевого индекса списка
            {
                countCheck[moveInt % 2] = checkpointsGameObject[moveInt % 2].Count - 1; // Переносим позицию на другой конец списка
            }
            else // Всё нормально
            {
                countCheck[moveInt % 2]--; // Переходим назад
            }

            // Переносим камеру на позицию чекпоинта
            targetForCamera.transform.position = checkpointsGameObject[moveInt % 2][countCheck[moveInt % 2]].transform.position;
        }
    }
    public void CheckActiveButton(int moveInt) // Активируется в TurnScript (да костыль, да я знаю, что в больших проектах так делать нельзя, но иначе мне нужно будет каждый кадр проверять, а есть ли чекпоинты или нет)
    {
        if (checkpointPosList[moveInt % 2].Count == 0) // Если чекпоинтов нет
        {
            // Отключаем кнопки перехода
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].GetComponent<Button>().interactable = false;
            }
        }
        else // Чекпоинты есть
        {
            // Включаем кнопки перехода
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].GetComponent<Button>().interactable = true;
            }
        }
    }
    #endregion

    private void Awake() // При старте
    {
        turnScript = gameObject.GetComponent<TurnScript>(); // Инициализируем основной скрипт

        //Создание двух списков в списке, ВОЗМОЖНО КОСТЫЛЬ
        checkpointsGameObject.Add(new List<GameObject>()); // Крестики
        checkpointsGameObject.Add(new List<GameObject>()); // Нолики

        checkpointPosList.Add(new List<Vector2>()); // Крестики
        checkpointPosList.Add(new List<Vector2>()); // Нолики


        if (targetForCamera == null) // Если цель камеры не задана
        {
            targetForCamera = GameObject.FindGameObjectWithTag("targetForCamera"); // Ищем цель на сцене
        }
        
        // Ставим кнопки неактивными при старте, так как чекпоинтов нет
        for (int i = 0; i < buttons.Length; i++) 
        {
            buttons[i].GetComponent<Button>().interactable = false;
        }

        cellMultiplicity = turnScript.cellMultiplicity;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(2)) // ср кнопка мыши
        {
            int moveInt = turnScript.moveInt; // Берём номер хода

            // Округляем/Получаем интовую позицию мыши 
            // Правила округления берем из TurnScript
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 checkpointPos = new Vector2(Mathf.Round(mousePos.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(mousePos.y / cellMultiplicity) * cellMultiplicity);



            // Ставим чекпоинт
            if (!checkpointPosList[moveInt % 2].Contains(checkpointPos)) 
            {
                // Заносим ее в список
                checkpointPosList[moveInt % 2].Add(checkpointPos);

                // Спавним префаб с визуальным отображением чекпоинта и тоже заносим в список
                checkpointsGameObject[moveInt % 2].Add(Instantiate(checkpointObject[moveInt % 2], checkpointPos, Quaternion.identity));

                countCheck[moveInt % 2] = checkpointPosList[moveInt % 2].Count - 1; // При постановке фигуры переходим в конец
            }
            else // Убираем чекпоинт
            {
                int index = checkpointPosList[moveInt % 2].FindIndex(a => a == checkpointPos);

                // УДАЛЯЕМ
                Destroy(checkpointsGameObject[moveInt % 2][index]); // Игровой объект
                checkpointsGameObject[moveInt % 2].RemoveAt(index); // Индекс из списка
                checkpointPosList[moveInt % 2].Remove(checkpointPos); // Игровой объект из списка

                countCheck[moveInt % 2] = checkpointPosList[moveInt % 2].Count - 1; // При удалении чекпоинта тоже переходим в конец
            }

            // Проверяем нужно ли активировать кнопки
            CheckActiveButton(moveInt);

        }

        if (Input.GetKeyDown(KeyCode.Z)) // Переход назад по чекпоинтам
        {
            GoBack();
            
        }
        if (Input.GetKeyDown(KeyCode.X))// Переход вперёд по чекпоинтам
        {
            GoForward();
        }
    }
}
