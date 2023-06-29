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

    private float offset;

    [HideInInspector]
    public GameObject[,] squareGameObjectArray;
    private List<GameObject> piceList = new List<GameObject>();

    private float boardLenght;
    private Vector2 startPos;
    private float cellSize;
    private int player = 0;
    private int currentMove;
    //private Color hightlightColor = new Color( 0.827451f, 0.3294118f,0f,1);
    private Color hightlightColor = new Color(0.2f, 0.59f, 0.85f, 1);
    //private Color hightlightColor = new Color(0.94f, 0.76f, 0.05f, 1f);

    private Vector2Int from;
    private Vector2Int to;




    // Start is called before the first frame update
    void Start()
    {
        float height = Camera.main.orthographicSize * 2;
        offset = height / 100;
        boardLenght = (height - 2*offset) / 8;
        startPos = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)) + new Vector3(boardLenght / 2, boardLenght / 2, 0) + new Vector3(offset,offset,0);
        cellSize = boardLenght / 2;


        board  = Utility.FlipHorizontaly2DArray( gameLogic.GetComponent<GameLogic>().board);
        if (player == 1)
        {
            board = Utility.FlipHorizontaly2DArray(Utility.FlipVerticaly2DArray(board));
        }
        BoardObject.transform.parent = gameObject.transform;
        squareGameObjectArray = new GameObject[8, 8];
        CreateBoard();
        gameLogic.GetComponent<GameLogic>().MoveMade += hightlightLastMove;
        gameLogic.GetComponent<GameLogic>().OnStateJump += SetUp;
        FileHandler.OnGameLoad += SimpleUpdate;
    }

    public void hightlightLastMove(int player, string move)
    {
        //currentMove = Pices.ShiftColor(player);
        currentMove = player;

        from = GameLogic.stringPositionToBoard(move.Substring(0,2));
        to = GameLogic.stringPositionToBoard(move.Substring(2,2));
        UpdateBoard();
        UpdatePices();
    }
    private void SetUp() 
    {
        currentMove =  gameLogic.GetComponent<GameLogic>().currentPlayer == 1 ? 0 : 1; 
        from = Vector2Int.zero;
        to = Vector2Int.zero;
        UpdateBoard();
        UpdatePices();
    }

    public void SimpleUpdate() 
    {
        from = Vector2Int.zero;
        to = Vector2Int.zero;
        UpdateBoard();
        UpdatePices();
    }


    // Update is called once per frame
    void Update()
    {

    }
    void DrawSquare(Vector3 position, Sprite sprite, Color color, Vector2Int xy)  
    {
        Vector2 coff = new Vector2( boardLenght *100 / sprite.textureRect.width, boardLenght *100 / sprite.textureRect.height);
        GameObject child = new GameObject();
        child.transform.localScale = new Vector3(child.transform.localScale.x *coff.x, child.transform.localScale.y * coff.y, 1);
        child.transform.parent = BoardObject.transform;
        child.layer = LayerMask.NameToLayer("Board");
        child.name = sprite.name +  xy.x.ToString() + xy.y.ToString();
        squareGameObjectArray[xy.x, xy.y] = child;
        child.AddComponent<SpriteRenderer>();
        child.GetComponent<SpriteRenderer>().sprite = sprite;
        child.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Board");
        child.GetComponent<SpriteRenderer>().color = color;
        child.AddComponent<BoxCollider>();
        child.transform.position = position;
    }
    void DrawPice (Vector3 position, Sprite sprite, Color color, Vector2 xy)
    {
        GameObject child = Instantiate(PiceTemplate,position, Quaternion.identity, pices.transform);
        Vector2 coff = new Vector2(boardLenght * 100 / sprite.textureRect.width, boardLenght * 100 / sprite.textureRect.height)* 0.8f;
        child.transform.localScale = new Vector3(child.transform.localScale.x * coff.x, child.transform.localScale.y * coff.y, 1);
        child.GetComponent<SpriteRenderer>().sprite = sprite;
        child.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Pieces");
        if (color == Color.white) child.tag = "White";
        else
        {
            child.tag = "Black";
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
                DrawSquare(new Vector3(boardLenght * x + startPos.x, boardLenght * y + startPos.y, 0), BlankSquare, ( (x+y) % 2) == 0 ? whiteSquareColor : blackSquareColor, new Vector2Int( x,y ) );

                if (board[x, y] != 0) 
                {
                    string spriteIndex = Pices.Name(board[x, y]);
                    Color color = (board[x, y] & Pices.colorMask) == Pices.white ? Color.white: Color.black;
                    Sprite[] sprites = new Sprite[6];
                    DrawPice(new Vector3(boardLenght * x + startPos.x, boardLenght * y + startPos.y, -1), PicesSpriteArray.GetSprite(spriteIndex), color, new Vector2(x, y));
                }
            }
        }
        squareGameObjectArray = Utility.FlipHorizontaly2DArray(squareGameObjectArray);
        if (player == 1)
        {
            squareGameObjectArray = Utility.FlipHorizontaly2DArray(Utility.FlipVerticaly2DArray(squareGameObjectArray));
        }
        activePlayer(1);
    }

    public void UpdatePices() 
    {
        int counter = piceList.Count;
        foreach (GameObject obj in piceList) 
        {
            Destroy(obj);
        }
        board = Utility.FlipHorizontaly2DArray(gameLogic.GetComponent<GameLogic>().board);

        if (player == 1)
        {
            board = Utility.FlipHorizontaly2DArray(Utility.FlipVerticaly2DArray(board));
        }
        piceList = new List<GameObject>();
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] != 0)
                {
                    string spriteIndex = Pices.Name(board[x,y]);
                    Color color = (board[x, y] & Pices.colorMask) == Pices.white ? Color.white : Color.black;
                    Sprite[] sprites = new Sprite[6];
                    DrawPice(new Vector3(boardLenght * x + startPos.x, boardLenght * y + startPos.y, -1), PicesSpriteArray.GetSprite(spriteIndex), color, new Vector2(x, y));
                }
            }
        }
        if (counter > piceList.Count) 
        {
            GetComponent<AudioSource>().Play();
        }
        activePlayer(currentMove);
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
        if (from != to) 
        {
            squareGameObjectArray[from.x, from.y].GetComponent<SpriteRenderer>().color = Color.Lerp(hightlightColor, squareGameObjectArray[from.x, from.y].GetComponent<SpriteRenderer>().color, 0.5f);
            squareGameObjectArray[to.x, to.y].GetComponent<SpriteRenderer>().color = Color.Lerp(hightlightColor, squareGameObjectArray[from.x, from.y].GetComponent<SpriteRenderer>().color, 0.5f);
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
