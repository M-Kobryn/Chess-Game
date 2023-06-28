using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Transactions;
using System.Xml.Schema;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;
using TMPro;

public class GameLogic : MonoBehaviour
{
    public int[,] board = new int[8, 8];

    public GameObject gameBoard;
    private List<string> gameHistory = new List<string>();

    public int currentPlayer;
    string castleAbility;
    string enpasanePassedSuare;
    private int halfMoves;
    private int turn;

    public bool AutoPlayEnabled;
    private bool gameEnded = false;

    public GameObject controller;
    public Action<int,string> MoveMade;
    public Action OnStateJump;
    bool autoPlayRunning = false;

    private static Dictionary<int, char> boardToFen = new Dictionary<int, char> 
    { 
        {1 , 'p'},
        {2 , 'r'},
        {3 , 'n'},
        {4 , 'b'},
        {5 , 'q'},
        {6 , 'k'}
    };

    private static Dictionary<Vector2, string> castleRook = new Dictionary<Vector2, string>
    {
        {new Vector2(0,0), "q" },
        {new Vector2(0,7), "Q" },
        {new Vector2(7,0), "k" },
        {new Vector2(7,7), "K" }
    };

    private static Dictionary<int, string> columnToLetter = new Dictionary<int, string>
    {
        { 0 , "a"  },
        { 1 , "b"  },
        { 2 , "c"  },
        { 3 , "d"  },
        { 4 , "e"  },
        { 5 , "f"  },
        { 6 , "g"  },
        { 7 , "h"  }
    };



    private async void Update()
    {
        if (AutoPlayEnabled && !autoPlayRunning)
        {
            autoPlayRunning = true;
            await controller.GetComponent<ChessEngineController>().NewMove(gameHistory.Last());
        }
        if (!AutoPlayEnabled)
        {
            autoPlayRunning = false;
        }
    }

    void Start()
    {
        gameHistory.Add("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        boardStateFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        controller.GetComponent<ChessEngineController>().OnNewBestMove += DoMove;

    }
    public async void DoMove(string e)
    {
        if (!gameEnded && AutoPlayEnabled) 
        {
            string move = e;
            Vector2Int from = stringPositionToBoard(move.Substring(0, 2));
            Vector2Int to = stringPositionToBoard(move.Substring(2, 2));
            MovePice(from, to);
            await controller.GetComponent<ChessEngineController>().NewMove(gameHistory.Last());
        }
    }

    private void EndPlayerTurn( int player ) 
    {
        //gameBoard.GetComponent<GameBoard>().activePlayer(currentPlayer);
        if (player == 1) currentPlayer = 0;
        else currentPlayer = 1;

    }

    private string FENfromBoardState() 
    {
        StringBuilder output = new StringBuilder();
        for (int y = 0; y < board.GetLength(0); y++) 
        {
            int tmp = 0;
            for (int x = 0; x < board.GetLength(1); x++) 
            {
                if (board[x, y] == Pices.blank)
                {
                    tmp += 1;
                }
                else
                {
                    if (tmp > 0)
                        {
                        output.Append(tmp.ToString());
                        tmp = 0;
                        }
                    char c = boardToFen[board[x, y] & Pices.piceMask];

                    if (Pices.IsWhite(board[x, y]))
                    {
                        c = char.ToUpper(c);
                    }
                    output.Append(c);
                }
            }
            if (tmp > 0)
            {
                output.Append(tmp.ToString());
                tmp = 0;
            }
            output.Append('/');
        }
        output.Length--;
        output.Append(' ');

        output.Append(currentPlayer == 0 ? "w" : "b" );

        output.Append(' ');

        if (castleAbility == "")
        {
            output.Append("-");
        }
        else 
        {
            output.Append(castleAbility);
        }
        output.Append(' ');

        output.Append(enpasanePassedSuare);
        output.Append(' ');

        output.Append(halfMoves.ToString());
        output.Append(' ');

        output.Append(turn.ToString());

        return output.ToString();
    }

    void boardStateFromFEN(string stringFEN)
    {
        int x = 0;
        int y = 0;
        string[] parts = stringFEN.Split(' ');
        foreach (char c in parts[0])
        {
            if ((c == '/'))
            {
                x = 0;
                y++;
                continue;
            }
            if (char.IsDigit(c))
            {
                for (int i = 0; i < char.GetNumericValue(c); i++)
                {
                    board[x, y] = Pices.blank;
                    x++;
                }
                continue;
            }
            board[x, y] = 0;
            if (char.IsUpper(c))
            {
                board[x, y] += Pices.white;
            }
            else
            {
                board[x, y] += Pices.black;
            }
            board[x,y] += boardToFen.FirstOrDefault(z => z.Value == char.ToLower(c)).Key;
 
            x++;
        }
        if (parts[1] == "w") currentPlayer = 0;
        else currentPlayer = 1;

        castleAbility = parts[2];

        enpasanePassedSuare = parts[3];

        halfMoves = Int32.Parse(parts[4]);

        turn = Int32.Parse(parts[5]);

    }
    public List<Vector3Int> PossibleMoves(Vector2Int position) 
    {
        List<Vector3Int> possibleMoves = UnrestricedPossibleMoves(position);
        if ((board[position.x, position.y] & Pices.piceMask) == Pices.king)
        {
            possibleMoves = possibleMoves.Concat(CastlePossibleMoves(position)).ToList();
        }
        possibleMoves = MoveRestrictions(position,possibleMoves);
        return possibleMoves;

    }

    public List<Vector3Int> MoveRestrictions(Vector2Int position, List<Vector3Int> possibleMoves) 
    {
        int pice = board[position.x, position.y];
        int piceType = board[position.x, position.y] & Pices.piceMask;
        int piceColor = board[position.x, position.y] & Pices.colorMask;
        int x = position.x;
        int y = position.y;

        //
        List<Vector3Int> movesToRemove = new List<Vector3Int>();
        if (piceType == Pices.king) 
        {
            foreach (Vector3Int move in possibleMoves)
            {
                if (CheckIfAnyPiceAttackSquare(Pices.ShiftColor(piceColor), new Vector2Int(move.x, move.y)))
                {
                    movesToRemove.Add(move);
                }
            }
        }

        //Remove from Possible the move that dont allowed because of pinned pice and check
        Vector2Int sameColorKingPosition = board.FindIndexIn2DArray(Pices.king | (pice & Pices.colorMask));

        foreach (Vector3Int move in possibleMoves)
        {
            int[,] copyBoard = board.Clone() as int[,];
            PhonyMovePice(new Vector2Int(x, y), new Vector2Int(move.x,move.y));
            if (piceType == Pices.king)
            {
                Vector2Int newKingPos = new Vector2Int(move.x,move.y);
                if (IsUnderAttack(Pices.ShiftColor(pice) & Pices.colorMask, new Vector2(newKingPos.x, newKingPos.y)).Count > 0)
                {
                    movesToRemove.Add(move);
                }
            }
            else 
            {
                if (IsUnderAttack(Pices.ShiftColor(pice) & Pices.colorMask, new Vector2(sameColorKingPosition.x, sameColorKingPosition.y)).Count > 0)
                {
                    movesToRemove.Add(move);
                }
            }
            board = copyBoard.Clone() as int[,];
        }
        return possibleMoves.Except(movesToRemove).ToList();

    }
    public List<Vector3Int> UnrestricedPossibleMoves(Vector2Int position)
    {
        List<Vector3Int> possibleMoves = new List<Vector3Int>();
        int pice = board[position.x, position.y];
        int piceType = board[position.x, position.y] & Pices.piceMask;
        int piceColor = board[position.x, position.y] & Pices.colorMask;
        int x = position.x;
        int y = position.y;
        if (piceType ==  Pices.blank) 
        {
            return possibleMoves;
        }
        if (piceType == Pices.pawn)
        {
            possibleMoves = PawnPossibleMoves(x, y, piceColor);
        }
        else if (piceType == Pices.knight)
        {
            possibleMoves = HorsePossibleMoves(x, y, pice);
        }
        else if (piceType == Pices.king) 
        {
            possibleMoves = KingPossibleMoves(x, y, pice);
        }
        else
        {
            possibleMoves = LongPossibleMoves(x, y, pice);
        }

        return possibleMoves;
    }

    private List<Vector3Int> PawnPossibleMoves(int x, int y, int color) 
    {
        List < Vector3Int > possibleMoves = new List<Vector3Int>();
        int d = 0;
        if (y == 0 || y == board.GetLength(1) -1) 
        {
            return possibleMoves;
        }
        if ((color & Pices.white) == Pices.white)
        {
            d = -1;
        }
        else
        {
            d = 1;
        }
        if (y == board.GetLength(0) - 2 && Pices.IsWhite(color) && board[x, y - 1] == Pices.blank && board[x, y - 2] == Pices.blank)
        {
            possibleMoves.Add(new Vector3Int(x, y - 2, 0));
        }
        if (y == 1 && Pices.IsBlack(color) && board[x, y + 1] == Pices.blank && board[x, y +2] == Pices.blank)
        {
            possibleMoves.Add(new Vector3Int(x, y +2, 0));
        }

        if (board[x, y + d] == Pices.blank)
        {
            possibleMoves.Add(new Vector3Int(x, y + d, 0));
        }
        if (x + 1 < board.GetLength(0) && board[x + 1, y + d] != Pices.blank && Pices.IsOppositeColor(board[x + 1, y + d],color))
        {
            possibleMoves.Add(new Vector3Int(x + 1, y + d, 1));
        }
        if (x - 1 >= 0 && board[x - 1, y + d] != Pices.blank && Pices.IsOppositeColor(board[x - 1 , y + d],color))
        {
            possibleMoves.Add(new Vector3Int(x - 1, y + d, 1));
        }
        if (enpasanePassedSuare != "-") 
        {
            if (x + 1 < board.GetLength(0) && board[x + 1, y + d] == Pices.blank && stringPositionToBoard(enpasanePassedSuare) == new Vector2Int(x + 1, y + d))
            {
                possibleMoves.Add(new Vector3Int(x + 1, y + d, 4));
            }
            if (x - 1 >= 0 && board[x - 1, y + d] == Pices.blank && stringPositionToBoard(enpasanePassedSuare) == new Vector2Int(x - 1, y + d))
            {
                possibleMoves.Add(new Vector3Int(x - 1, y + d, 4));
            }
        }

        return possibleMoves;
    }
    private List<Vector3Int> HorsePossibleMoves(int x, int y, int pice) 
    {
        List<Vector3Int> possibleMoves = new List<Vector3Int>();
        foreach ( Vector2Int v in Pices.DirectionList[pice & Pices.piceMask]) 
        {
            if (x + v.x >= board.GetLength(0) || x + v.x < 0) continue;
            if (y + v.y >= board.GetLength(1) || y + v.y < 0) continue;

            if (board[x + v.x, y + v.y] == Pices.blank)
            {
                possibleMoves.Add(new Vector3Int(x + v.x, y + v.y, 0));
            }
            else if ((board[x + v.x, y + v.y] & Pices.colorMask) != (pice & Pices.colorMask)) 
            {
                possibleMoves.Add(new Vector3Int(x + v.x, y + v.y, 1));
            }
        }
        return possibleMoves;
    }
    private List<Vector3Int> KingPossibleMoves(int x, int y, int pice)
    {
        List<Vector3Int> possibleMoves = new List<Vector3Int>();
        foreach (Vector2Int v in Pices.DirectionList[pice & Pices.piceMask])
        {
            if (x + v.x >= board.GetLength(0) || x + v.x < 0) continue;
            if (y + v.y >= board.GetLength(1) || y + v.y < 0) continue;
            if (board[x + v.x, y + v.y] == Pices.blank)
            {
                possibleMoves.Add(new Vector3Int(x + v.x, y + v.y, 0));
            }
            else if ((board[x + v.x, y + v.y] & Pices.colorMask) != (pice & Pices.colorMask))
            {
                possibleMoves.Add(new Vector3Int(x + v.x, y + v.y, 1));
            }
        }

        return possibleMoves;
    }
    private List<Vector3Int> LongPossibleMoves(int x, int y, int pice)
    {
        List<Vector3Int> possibleMoves = new List<Vector3Int>();
        foreach (Vector2Int v in Pices.DirectionList[pice & Pices.piceMask])
        {
            int scale = 1;
            for (int a = 0; a< 20;a++)
            {
                if (x + scale* v.x >= board.GetLength(0) || x + scale * v.x < 0) break;
                if (y + scale* v.y >= board.GetLength(1) || y + scale * v.y < 0) break;

                if (board[x + scale * v.x, y + scale * v.y] == Pices.blank)
                {
                    possibleMoves.Add(new Vector3Int(x + scale * v.x, y + scale * v.y, 0));
                    scale++;
                }
                else if ((board[x + scale * v.x, y + scale * v.y] & Pices.colorMask) != (pice & Pices.colorMask))
                {
                    possibleMoves.Add(new Vector3Int(x + scale * v.x, y + scale * v.y, 1));
                    break;
                }
            }

        }
        return possibleMoves;
    }

    private List<Vector3Int> CastlePossibleMoves(Vector2Int position)
    {
        int pice = board[position.x, position.y];
        int piceType = board[position.x, position.y] & Pices.piceMask;
        int piceColor = board[position.x, position.y] & Pices.colorMask;
        int x = position.x;
        int y = position.y;
        List<Vector3Int> possibleMoves = new List<Vector3Int>();
        if (Pices.IsWhite(piceColor) && castleAbility.Contains("Q"))
        {
            if (board[x - 1, y] == Pices.blank && board[x - 2, y] == Pices.blank && board[x - 3, y] == Pices.blank) 
            {
                if (!(CheckIfAnyPiceAttackSquare(Pices.black, new Vector2(x - 1, y)) || CheckIfAnyPiceAttackSquare(Pices.black, new Vector2(x - 2, y))))
                {
                    possibleMoves.Add(new Vector3Int(x - 2, y, 2));
                }
            }
        }
        if (Pices.IsWhite(piceColor) && castleAbility.Contains("K"))
        {
            if (board[x +1, y] == Pices.blank && board[x +2, y] == Pices.blank)
            {
                if (!(CheckIfAnyPiceAttackSquare(Pices.black, new Vector2(x + 1, y)) || CheckIfAnyPiceAttackSquare(Pices.black, new Vector2(x + 2, y))))
                {
                    possibleMoves.Add(new Vector3Int(x + 2, y, 3));
                }
            }
        }

        if (Pices.IsBlack(piceColor) && castleAbility.Contains("q"))
        {
            if (board[x - 1, y] == Pices.blank && board[x - 2, y] == Pices.blank && board[x - 3, y] == Pices.blank)
            {
                if (!(CheckIfAnyPiceAttackSquare(Pices.white, new Vector2(x - 1, y)) || CheckIfAnyPiceAttackSquare(Pices.white, new Vector2(x - 2, y))))
                {
                    possibleMoves.Add(new Vector3Int(x - 2, y, 2));
                }
            }
        }
        if (Pices.IsBlack(piceColor) && castleAbility.Contains("k"))
        {
            if (board[x + 1, y] == Pices.blank && board[x + 2, y] == Pices.blank)
            {
                if (!(CheckIfAnyPiceAttackSquare(Pices.white, new Vector2(x + 1, y)) || CheckIfAnyPiceAttackSquare(Pices.white, new Vector2(x + 2, y))))
                {
                    possibleMoves.Add(new Vector3Int(x + 2, y, 3));
                }
            }
        }
        return possibleMoves;
    }

    //Nie sprawdza czy król coœ atakuje 
    private bool CheckIfAnyPiceAttackSquare(int attackerColor, Vector2 positionToCheck) 
    {
        for (int x = 0; x < board.GetLength(0); x++) 
        {
            for (int y = 0; y < board.GetLength(1); y++) 
            {
                if ( (board[x, y] == Pices.blank) || Pices.IsOppositeColor(board[x,y],attackerColor )) 
                {
                    continue;
                }
                List<Vector3Int>  moves = UnrestricedPossibleMoves( new Vector2Int(x,y));
                foreach (Vector3Int move in moves) 
                {
                    if (move.z ==0 && ( (board[x,y] & Pices.piceMask ) == Pices.pawn) )continue;
                    if (positionToCheck == new Vector2(move.x, move.y)) 
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    private List<Vector2Int> IsUnderAttack(int attackerColor, Vector2 positionToCheck)
    {
        List<Vector2Int> attackers = new List<Vector2Int>();
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if ((board[x, y] == Pices.blank) || Pices.IsOppositeColor(board[x, y], attackerColor) )
                {
                    continue;
                }
                List<Vector3Int> moves = UnrestricedPossibleMoves(new Vector2Int(x, y));
                foreach (Vector3Int move in moves)
                {
                    if (positionToCheck == new Vector2(move.x, move.y))
                    {
                        attackers.Add(new Vector2Int(x, y));
                    }
                }
            }
        }
        return attackers;
    }

    public static string boardPositionToString(Vector2Int position) 
    {
        return columnToLetter[position.x] + (8 - position.y).ToString();
    }

    public static Vector2Int stringPositionToBoard(string position)
    {
        return new Vector2Int(columnToLetter.FirstOrDefault(z => z.Value == position[0].ToString()).Key , 8 - Int32.Parse( position[1].ToString()));
    }

    public bool MovePice(Vector2Int from, Vector2Int destination) 
    {
        List<Vector3Int> moves =  PossibleMoves(from);
        foreach (Vector3Int move in moves) 
        {
            if (move.x == destination.x && move.y == destination.y) 
            {
                halfMoves += 1;
                enpasanePassedSuare = "-";
                board[move.x, move.y] = board[from.x, from.y];
                board[from.x, from.y] = Pices.blank;
                if (move.z == 1) halfMoves = 0;
                if ((board[destination.x, destination.y] & Pices.piceMask) == Pices.pawn) 
                {
                    halfMoves = 0;
                    if (Mathf.Abs(destination.y - from.y) == 2) 
                    {
                        enpasanePassedSuare = boardPositionToString(new Vector2Int(destination.x ,(destination.y + from.y)/2));
                    }
                    if (move.z == 4) 
                    {
                        board[destination.x, destination.y + (( Pices.IsWhite(board[destination.x, destination.y]))? 1 : -1 )] = 0;
                    }
                    if (!((destination.y != 0) && destination.y != board.GetLength(0) - 1))
                    {
                        gameBoard.GetComponent<GameBoard>().BlockAllPices();
                        StartCoroutine(Promotion(destination));
                    }
                }
                if ((board[destination.x, destination.y] & Pices.piceMask) == Pices.king) 
                {
                    castleAbility = castleAbility.Replace(Pices.IsWhite(board[destination.x, destination.y]) ? "Q" : "q", string.Empty);
                    castleAbility = castleAbility.Replace(Pices.IsWhite(board[destination.x, destination.y]) ? "K" : "k", string.Empty);
                    if (move.z == 2)
                    {
                        board[3, destination.y] = board[0, destination.y];
                        board[0, destination.y] = Pices.blank;
                    }
                    else if ( move.z == 3)
                    {
                        board[5, destination.y] = board[7, destination.y];
                        board[7, destination.y] = Pices.blank;
                    }
                }
                if (castleRook.ContainsKey(destination))
                {
                    castleAbility = castleAbility.Replace(castleRook[destination], string.Empty);
                }
                else if (castleRook.ContainsKey(from)) 
                {
                    castleAbility = castleAbility.Replace(castleRook[from], string.Empty);
                }
                MoveMade?.Invoke(currentPlayer, boardPositionToString(from) + boardPositionToString(destination) );
                AfterMove(board[destination.x, destination.y] & Pices.colorMask);
                return true;
            }
        }
        return false;
    }
    public void AfterMove(int colorThatMoved) 
    {
        if (!IsCheckmate(Pices.ShiftColor(colorThatMoved)))
        {
            if (IsPat(Pices.ShiftColor(colorThatMoved)))
            {
                gameEnded = true;
            }
        }
        else 
        {
            gameEnded = true;
        }
        if (colorThatMoved == Pices.black) 
        {
            turn += 1;
        }
        //gameBoard.GetComponent<GameBoard>().UpdatePices();
        EndPlayerTurn(currentPlayer);
        gameHistory.Add(FENfromBoardState());
        RepetirionRule();


        if (halfMoves > 50) 
        {
            Debug.Log("Remis!");
            gameEnded = true;
        }
    }
    private bool RepetirionRule()
    {
        string[] states = gameHistory.ToArray().Reverse().ToArray();
        for(int x = 0; x< states.Count(); x++ ) 
        {
            states[x] = states[x].Split(" ")[0];
        }
        if (states.Length > 8)
        {
            for (int x = 0; x < 4; x++)
            {
                if (states[x] != states[4 + x])
                {
                    return false;

                }
            }
        }
        else 
        {
            return false;
        }
        Debug.Log("Draw : Repetition Rule");
        gameEnded = true;
        return true;
    }
    private bool IsCheckmate(int color) 
    {
        Vector2Int kingPos = board.FindIndexIn2DArray(Pices.king | color);
        if (IsUnderAttack(Pices.ShiftColor(board[kingPos.x, kingPos.y] & Pices.colorMask), kingPos).Count > 0)
        {
            bool anyOne = false;
            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    if (board[x, y] == Pices.blank) continue;
                    if (!((board[x, y] & Pices.colorMask) == (board[kingPos.x, kingPos.y] & Pices.colorMask))) continue;
                    if (PossibleMoves(new Vector2Int(x, y)).Count > 0)
                    {
                        anyOne = true;
                    }
                }
            }
            if (!anyOne) Debug.Log("Check Mate!");
            if (!anyOne) return true;
        }
        return false;
    }

    private bool IsPat(int color)
    {
        Vector2Int kingPos = board.FindIndexIn2DArray(Pices.king | color);
        bool anyOne = false;
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == Pices.blank) continue;
                if (!((board[x, y] & Pices.colorMask) == (board[kingPos.x, kingPos.y] & Pices.colorMask))) continue;
                if (PossibleMoves(new Vector2Int(x, y)).Count > 0)
                {
                    anyOne = true;
                }
            }
        }
        if (!anyOne) Debug.Log("Remis!");
        if (!anyOne) return true;
        return false;
    }


    public bool PhonyMovePice(Vector2Int from, Vector2Int destination)
    {
        List<Vector3Int> moves = UnrestricedPossibleMoves(from);
        foreach (Vector3Int move in moves)
        {
            if (move.x == destination.x && move.y == destination.y)
            {
                board[move.x, move.y] = board[from.x, from.y];
                board[from.x, from.y] = Pices.blank;
                if (!((board[destination.x, destination.y] & Pices.piceMask) == Pices.pawn || (board[destination.x, destination.y] & Pices.piceMask) == Pices.king))
                {
                    return true;
                }

                if ((board[destination.x, destination.y] & Pices.piceMask) == Pices.king && move.z == 2)
                {
                    board[3, destination.y] = board[0, destination.y];
                    board[0, destination.y] = Pices.blank;

                }
                if ((board[destination.x, destination.y] & Pices.piceMask) == Pices.king && move.z == 3)
                {
                    board[5, destination.y] = board[7, destination.y];
                    board[7, destination.y] = Pices.blank;
                }
                return true;
            }
        }
        return false;
    }

    public void BackInTime(int index) 
    {
        if (gameHistory.Count < index) 
        {
            return;
        }
        List<string> newGameHistory = new List<string>();
        int x =0;
        foreach (string fen in gameHistory) 
        {
            if (x <= index) 
            {
                newGameHistory.Add(fen);
            }
            x++;
        }
        gameHistory = newGameHistory;
        boardStateFromFEN(gameHistory.Last());
        OnStateJump?.Invoke();
    }


    IEnumerator Promotion( Vector2Int position) 
    {
        int promotionFigure = 0;
        while (!((Input.GetKeyDown(KeyCode.Q)) || (Input.GetKeyDown(KeyCode.W)) || (Input.GetKeyDown(KeyCode.E)) || (Input.GetKeyDown(KeyCode.R)))) 
        {
            yield return null;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            promotionFigure = Pices.queen;
        }
        else if (Input.GetKeyDown(KeyCode.W)) 
        {
            promotionFigure = Pices.rook;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            promotionFigure = Pices.knight;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            promotionFigure = Pices.bishop;
        }
        board[position.x,position.y] = promotionFigure | (board[position.x, position.y] & Pices.colorMask );
        gameBoard.GetComponent<GameBoard>().UpdatePices();
        DragAndDropBehavior[] children = GetComponentsInChildren<DragAndDropBehavior>();
        foreach (DragAndDropBehavior child in children)
        {
            child.enabled = true;
        }
        IsCheckmate(Pices.ShiftColor((board[position.x, position.y] & Pices.colorMask)));
    }
}
