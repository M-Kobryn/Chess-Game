using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using Assets;

public class DragAndDropBehavior : MonoBehaviour
{
    private Vector3 mousePos = Vector3.zero;
    private Vector3 returnMousePostion = Vector3.zero;

    [HideInInspector]
    public GameLogic gameLogic;
    [HideInInspector]
    public GameObject gameBoard;
    private List<Vector3Int> possibleMoves = new List<Vector3Int>();

    public bool tmpLegalMove = true;

    Vector2Int startPosition;
    public void Update()
    {

    }

    public  void Awake()
    {

    }

    private void OnMouseDown()
    {
        if (!enabled) return;
        returnMousePostion = transform.position;

        Vector3 tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        tmp = new Vector3(tmp.x, tmp.y, -9);
        RaycastHit ray;
        if (Physics.Raycast(tmp, Vector3.forward, out ray, Mathf.Infinity))
        {
            tmp = ray.transform.position;
            transform.position = new Vector3(tmp.x, tmp.y, transform.position.z);
        }
        startPosition = Assets.Utility.FindIndexIn2DArray(gameBoard.GetComponent<GameBoard>().squareGameObjectArray, ray.transform.gameObject);

        possibleMoves = gameLogic.PossibleMoves(startPosition);
        foreach (Vector3Int move in possibleMoves) 
        {
            Color color = move.z == 0 ? Color.yellow : Color.red;
            color.a = 128;
            Color currentColor = gameBoard.GetComponent<GameBoard>().squareGameObjectArray[move.x, move.y].GetComponent<SpriteRenderer>().color;
            gameBoard.GetComponent<GameBoard>().squareGameObjectArray[move.x, move.y].GetComponent<SpriteRenderer>().color = Color.Lerp(color,currentColor, 0.5f); 
        }
    }


    private void OnMouseDrag()
    {
        if (!enabled) return;
        Vector3 tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3 (tmp.x, tmp.y, -1);
    }

    private void OnMouseUp()
    {
        if (!enabled) return;
        gameBoard.GetComponent<GameBoard>().UpdateBoard();
        Vector3 tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        tmp = new Vector3(tmp.x, tmp.y, -9);
        RaycastHit ray;
        if (Physics.Raycast(tmp, Vector3.forward, out ray, Mathf.Infinity))
        {
            Vector2Int endPostion = Utility.FindIndexIn2DArray(gameBoard.GetComponent<GameBoard>().squareGameObjectArray, ray.transform.gameObject);
            if (gameLogic.MovePice(startPosition, endPostion))
            {
                tmp = ray.transform.position;
                transform.position = new Vector3(tmp.x, tmp.y, transform.position.z);
                return;
            }
        }
        transform.position = returnMousePostion;
    }


}
