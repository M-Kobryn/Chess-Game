using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class GameBoard : MonoBehaviour
{
    public int[,] board;
    public Color blackSquareColor;
    public Color whiteSquareColor;
    public GameObject BoardObject;
    public Sprite BlankSquare;
    public SpriteAtlas PicesSpriteArray;
    public Vector2 squareSize;
    public GameObject SquareTemplate;
    public GameObject PiceTemplate;
    public GameObject pices;
    public GameObject gameLogic;

    [HideInInspector]
    public GameObject[,] squareGameObjectArray;
    private List<GameObject> piceList = new List<GameObject>();

    public Vector2 cellSize;
    // Start is called before the first frame update
    void Start()
    {
        board  = gameLogic.GetComponent<GameLogic>().board;
        BoardObject.transform.parent = gameObject.transform;
        squareGameObjectArray = new GameObject[8, 8];
        CreateBoard();

    }

    // Update is called once per frame
    void Update()
    {
    }

    void DrawSquare(Vector3 position, Sprite sprite, Color color, Vector2Int xy)  
    {

        GameObject child = new GameObject();
        child.transform.parent = BoardObject.transform;
        child.layer = LayerMask.NameToLayer("Board");
        child.name = sprite.name +  xy.x.ToString() + xy.y.ToString();
        squareGameObjectArray[xy.x, xy.y] = child;
        child.AddComponent<SpriteRenderer>();
        child.GetComponent<SpriteRenderer>().sprite = sprite;
        child.GetComponent<SpriteRenderer>().color = color;
        child.AddComponent<BoxCollider>();
        child.transform.position = position;
    }
    void DrawPice (Vector3 position, Sprite sprite, Color color, Vector2 xy)
    {
        GameObject child = Instantiate(PiceTemplate,position, Quaternion.identity, pices.transform);
        child.GetComponent<SpriteRenderer>().sprite = sprite;
        child.GetComponent<SpriteRenderer>().color = color;
        if (color == Color.white) child.tag = "White";
        else 
        {
            child.tag = "Black";
            child.GetComponent<DragAndDropBehavior>().enabled = false;
        }
        child.GetComponent<DragAndDropBehavior>().gameBoard = gameObject;
        child.GetComponent<DragAndDropBehavior>().gameLogic = gameLogic.GetComponent<GameLogic>();
        piceList.Add(child);
        child.name = sprite.name + xy.x.ToString() + xy.y.ToString();

    }
    void CreateBoard() 
    {
        for (int x = 0; x < board.GetLength(0); x++) 
        {
            for (int y = 0; y < board.GetLength(1); y++) 
            {
                DrawSquare(new Vector3(squareSize.x * x, squareSize.y * y, 0), BlankSquare, ( (x+y) % 2) == 0 ? blackSquareColor : whiteSquareColor , new Vector2Int( x,y ) );

                if (board[x, y] != 0) 
                {
                    int spriteIndex = board[x,y] &  Pices.piceMask;
                    Color color = (board[x, y] & Pices.colorMask) == Pices.white ? Color.white: Color.black;
                    Sprite[] sprites = new Sprite[6];
                    DrawPice(new Vector3(squareSize.x * x, squareSize.y * y, -1), PicesSpriteArray.GetSprite("ChessPices_" + spriteIndex.ToString()), color, new Vector2(x, y));
                }
            }
        }
    }

    public void UpdatePices() 
    {
        foreach (GameObject obj in piceList) 
        {
            Destroy(obj);
        }
        board = gameLogic.GetComponent<GameLogic>().board;
        piceList = new List<GameObject>();
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] != 0)
                {
                    int spriteIndex = board[x, y] & Pices.piceMask;
                    Color color = (board[x, y] & Pices.colorMask) == Pices.white ? Color.white : Color.black;
                    Sprite[] sprites = new Sprite[6];
                    DrawPice(new Vector3(squareSize.x * x, squareSize.y * y, -1), PicesSpriteArray.GetSprite("ChessPices_" + spriteIndex.ToString()), color, new Vector2(x, y));
                }
            }
        }
    }
    public void UpdateBoard() 
    {
        for (int x = 0; x < board.GetLength(0); x++) 
        {
            for (int y = 0; y < board.GetLength(1); y++) 
            {
                squareGameObjectArray[x,y].GetComponent<SpriteRenderer>().color = ( (x + y) % 2) == 0 ? blackSquareColor : whiteSquareColor;
            }
        }
    }

    public void activePlayer(int player) 
    {
        string activateTag = " ";
        string deactivateTag = " ";
        if (player == 1)
        {
            activateTag = "White";
            deactivateTag = "Black";
        }
        else
        {
            activateTag = "Black";
            deactivateTag = "White";
        }
        foreach (Transform child in pices.transform)
        {
            if (child.tag == activateTag)
            {
                child.GetComponent<DragAndDropBehavior>().enabled = true;
            }
            else if (child.tag == deactivateTag)
            {
                child.GetComponent<DragAndDropBehavior>().enabled = false;
            }
        }
    }
    public void BlockAllPices() 
    {
        
        DragAndDropBehavior[] children = pices.GetComponentsInChildren<DragAndDropBehavior>();
        foreach (DragAndDropBehavior child in children)
        {
            child.enabled = false;
        }
    }
}
