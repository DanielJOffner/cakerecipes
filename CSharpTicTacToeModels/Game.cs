﻿using System.Collections.Generic;
using System;

namespace QUT.CSharpTicTacToe
{
    public class Game : ITicTacToeGame<Player>
    {
        public int size;
        public Player player;
        private List<List<Player>> board;

        public Game(int size, Player player)
        {
            this.size = size;
            this.player = player;
            board = new List<List<Player>>();

            //populate empty board [[None,None...size],[None,None..size]...size]
            for (int row = 0; row < size; row++)
            {
                List<Player> rowLine = new List<Player>();
                for (int col = 0; col < size; col++)
                {
                    rowLine.Add(Player.None);
                }
                board.Add(rowLine);
            }
        }

        //**ITicTacToeGame interface method requirements**//
        public int Size => size;
        public Player Turn => player;
        public string getPiece(int row, int col)
        {
            return getPieceFromPlayer(board[row][col]);
        }
        //** End interface method requirements**//

        //Returns a string to represent a Player on the board
        // "X" for Cross, "O" for Nought, "" for None
        private string getPieceFromPlayer(Player player)
        {
            switch (player)
            {
                case Player.Nought:
                    return "O";
                case Player.Cross:
                    return "X";
                case Player.None:
                    return "";
                default:
                    throw new Exception("Expected either Player.Nought, Player.Cross or Player.None");
            }
        }

        //switch player after each move is applied
        private Player switchPlayer(Player player)
        {
            switch (player)
            {
                case Player.Nought:
                    return Player.Cross;
                case Player.Cross:
                    return Player.Nought;
                default:
                    throw new Exception("Expected either Player.Nought or Player.Cross");

            }
        }

        //Apply a given move to the board at position [row][col]
        //then switch to the other player's turn
        public void applyMove(Move move)
        {
            board[move.row][move.col] = player;
            player = switchPlayer(player);
        }


        //Generate a list containing all possible combinations of board coordinates
        //Represented as list<ValueTuple<int, int>> 
        private List<(int row, int col)> getAllBoardCoordinates() 
        {
            List<ValueTuple<int, int>> allCoordinates = new List<ValueTuple<int, int>>();
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    (int row, int col) coordinate = (row, col);
                    allCoordinates.Add(coordinate);
                }
            }
            return allCoordinates;
        }


        //Generate a list containing all of the lines on the board: Horizontal, Vertical and Diagonal
        //Each line is represented as a list of coor List<ValueTuple<int,int>>
        private List<List<(int row, int col)>> getLines()
        { 
            var allCoordinates = getAllBoardCoordinates();

            //data structure for lines
            var lines = new List<List<(int row, int col)>>();
            for (int i = 0; i < size; i++)
            {
                //get horizontal lines for row 'i'
                lines.Add(allCoordinates.FindAll((coordinate) => coordinate.row == i));
                //get vertical lines for col 'i'
                lines.Add(allCoordinates.FindAll((coordinate) => coordinate.col == i));
                //get diagoalLeftRight (row = col)
                lines.Add(allCoordinates.FindAll((coordinate) => coordinate.row == coordinate.col));
                //get diagonalRightLeft (row + col = board size - 1)
                lines.Add(allCoordinates.FindAll((coordinate) => coordinate.row + coordinate.col == size-1));
            }
            return lines;
        }

        // Checks a single line (specified as a sequence of (row,column) coordinates) to determine if one of the players
        // has won by filling all of those squares, or a Draw if the line contains at least one Nought and one Cross
        private TicTacToeOutcome<Player> checkLine(List<(int row, int col)> line)
        {
            int crossCount = line.FindAll((c) => board[c.row][c.col] == Player.Cross).Count;
            int noughtCount = line.FindAll((c) => board[c.row][c.col] == Player.Nought).Count;

            //TicTacToeOutcome expects Tuple<int,int> not Value Tuple (int row, int col)
            var convertedLine = line.ConvertAll((input) => input.ToTuple());

            if (crossCount == size){ return TicTacToeOutcome<Player>.NewWin(Player.Cross, convertedLine); } 
            if (noughtCount == size){ return TicTacToeOutcome<Player>.NewWin(Player.Nought, convertedLine); }
            if (crossCount >= 1 && noughtCount >= 1){ return TicTacToeOutcome<Player>.Draw; }
            return TicTacToeOutcome<Player>.Undecided;
        }

        public TicTacToeOutcome<Player> getGameOutcome()
        {
            var lines = getLines();
            var outcomes = new List<TicTacToeOutcome<Player>>();

            //collect all outcomes for all lines 
            foreach(var line in lines)
            {
                outcomes.Add(checkLine(line));
            }

            //if a player has one, return the win outcome 
            foreach (var outcome in outcomes)
            {
                if(outcome.IsWin){ return outcome; }
            }

            //if all lines are a daw, return the draw outcome
            if (outcomes.TrueForAll((outcome) => outcome.IsDraw)){
                return TicTacToeOutcome<Player>.Draw;
            } 

            //if there are no wins and the game is not a daw - return the undecided outcome
            return TicTacToeOutcome<Player>.Undecided;
        }

    }    
}