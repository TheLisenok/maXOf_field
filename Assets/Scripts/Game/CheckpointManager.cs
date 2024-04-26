using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private List<List<GameObject>> checkpointsGameObject = new List<List<GameObject>>();
    [SerializeField] private List<List<Vector2>> checkpointPosList = new List<List<Vector2>>();

    [SerializeField] private GameObject[] checkpointObject;
    [SerializeField] private GameObject targerForCamera;
    [SerializeField] private GameObject[] buttons;

    private int cellMultiplicity;
    //private int moveInt;
    private List<int> countCheck = new List<int>{0, 0}; // Позиция последнег чекпоинта, которые переключал игрок (для каждого игрока свой)
    private TurnScript turnScript;

    public void GoForward()
    {
        int moveInt = turnScript.moveInt;

        if (checkpointsGameObject[moveInt % 2].Count > 0) // Если чекпоинты хотя бы уже ставили
        {
            if (countCheck[moveInt % 2] + 1 > checkpointsGameObject[moveInt % 2].Count - 1)
            {
                countCheck[moveInt % 2] = 0;
            }
            else
            {
                countCheck[moveInt % 2]++;
            }

            try
            {
                targerForCamera.transform.position = checkpointsGameObject[moveInt % 2][countCheck[moveInt % 2]].transform.position;
            }
            catch
            {
                Debug.LogWarning("countCheck: " + countCheck[moveInt % 2] + "; Count: " + checkpointsGameObject[moveInt % 2].Count + "; moveInt: " + moveInt);
            }

        }
    }

    public void GoBack()
    {
        int moveInt = turnScript.moveInt;

        if (checkpointsGameObject[moveInt % 2].Count > 0)
        {
            if (countCheck[moveInt % 2] - 1 < 0)
            {
                countCheck[moveInt % 2] = checkpointsGameObject[moveInt % 2].Count - 1;
            }
            else
            {
                countCheck[moveInt % 2]--;
            }

            targerForCamera.transform.position = checkpointsGameObject[moveInt % 2][countCheck[moveInt % 2]].transform.position;
        }
    }
    
    public void CheckActiveButton(int moveInt) // Активируется в TurnScript (да костыль, да я знаю, что в больших проектах так делать нельзя, но иначе мне нужно будет каждый кадр проверять, а есть ли чекпоинты или нет)
    {
        if (checkpointPosList[moveInt % 2].Count == 0) 
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].GetComponent<Button>().interactable = false;
            }
        }
    }
    
    private bool CheckSameCheckpoints(List<Vector2> _checkpointPosList, Vector2 nowCheckpoint)
    {
        return nowCheckpoint == _checkpointPosList.Find(x => x == nowCheckpoint);
    }

    private void Awake()
    {
        turnScript = gameObject.GetComponent<TurnScript>();

        //Создание двух списков в списке, ВОЗМОЖНО КОСТЫЛЬ
        checkpointsGameObject.Add(new List<GameObject>());
        checkpointsGameObject.Add(new List<GameObject>());

        checkpointPosList.Add(new List<Vector2>());
        checkpointPosList.Add(new List<Vector2>());


        if (targerForCamera == null)
        {
            targerForCamera = GameObject.FindGameObjectWithTag("targetForCamera");
        }

        for (int i = 0; i < buttons.Length; i++) // Ставим кнопки неактивными при старте, так как чекпоинтов нет
        {
            buttons[i].GetComponent<Button>().interactable = false;
        }

        cellMultiplicity = turnScript.cellMultiplicity;
        //moveInt = turnScript.moveInt;
}

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(2)) // ср кнопка мыши
        {
            int moveInt = turnScript.moveInt;

            // Округляем/Получаем интовую позицию мыши 
            // Правила округления берем из TurnScript
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 checkpointPos = new Vector2(Mathf.Round(mousePos.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(mousePos.y / cellMultiplicity) * cellMultiplicity);




            //DEBUG
            foreach (var i in checkpointPosList){
                //Debug.Log(i);
            }



            // Если вектора нет в списке
            if (!CheckSameCheckpoints(checkpointPosList[moveInt % 2], checkpointPos))
            {
                // Заносим ее в список
                checkpointPosList[moveInt % 2].Add(checkpointPos);

                // Спавним префаб с визуальным отображением чекпоинта и тоже заносим в список
                checkpointsGameObject[moveInt % 2].Add(Instantiate(checkpointObject[moveInt % 2], checkpointPos, Quaternion.identity));

                countCheck[moveInt % 2] = checkpointPosList[moveInt % 2].Count - 1; // При постановке фигуры переходим в конец
            }
            else
            {
                //Debug.Log(checkpointPosList.FindIndex(a => a == checkpointPos));
                int index = checkpointPosList[moveInt % 2].FindIndex(a => a == checkpointPos);

                Destroy(checkpointsGameObject[moveInt % 2][index]);
                checkpointsGameObject[moveInt % 2].RemoveAt(index); // По индексу

                checkpointPosList[moveInt % 2].Remove(checkpointPos); // По эллементу

                countCheck[moveInt % 2] = checkpointPosList[moveInt % 2].Count - 1; // При удалении чекпоинта тоже переходим в конец
            }

            if (checkpointPosList[moveInt % 2].Count > 0) // Если есть хоть какие-то чекпоинты
            {
                for (int i = 0; i < buttons.Length; i++) // Включаем кнопки
                {
                    buttons[i].GetComponent<Button>().interactable = true;
                }
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++) 
                {
                    buttons[i].GetComponent<Button>().interactable = false;
                }
            }

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
