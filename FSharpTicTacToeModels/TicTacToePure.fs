namespace QUT

    module FSharpPureTicTacToeModel =
    
        // type to represent the two players: Noughts and Crosses
        type Player = Nought | Cross 

        // Returns a string to represent player piece on the board
        // "X" for Cross, "O" for Nought and "" for none
        let getPiece (player: Player) : string =
            match player with
            | Nought -> "O"
            | Cross -> "X"
            
        // type to represent a single move specified using (row, column) coordinates of the selected square
        type Move = 
            { Row: int; Column: int }
            interface ITicTacToeMove with
                member this.Row with get() = this.Row
                member this.Col with get() = this.Column

        // type to represent the current state of the game, including the size of the game (NxN), who's turn it is and the pieces on the board
        type GameState = 
            { Turn: Player; Size: int; Board: list<int*int*string>}
            interface ITicTacToeGame<Player> with
                member this.Turn with get()    = this.Turn
                member this.Size with get()    = this.Size
                member this.getPiece(row, col) = raise (System.NotImplementedException("getPiece"))

        // Returns a string representing which player occupies a given row col
        // "O" for Nought, "X" for Cross, "" for none
        let getPlayerAt (gameState:GameState) row col : string =
            gameState.Board
            |> List.filter (fun (a,b,c) -> a = row && b = col)
            |> fun list -> list.[0]
            |> fun (a,b,c) -> c
      
        let CreateMove row col = {Row = row; Column = col}

        let ApplyMove (oldState:GameState) (move: Move) = 
            let currentPlayerTurn = oldState.Turn
            let newPlayerTurn =
                match oldState.Turn with
                | Nought -> Cross
                | Cross -> Nought
            let newBoardState =
                oldState.Board
                |> List.map (fun (row,col,piece) -> 
                    if row = move.Row && col = move.Column 
                    then (row,col, getPiece currentPlayerTurn) 
                    else (row,col,piece))
            {
            Turn = newPlayerTurn; 
            Size = oldState.Size; 
            Board = newBoardState
            }

        // Returns a sequence containing all of the lines on the board: Horizontal, Vertical and Diagonal
        // The number of lines returned should always be (size*2+2)
        // the number of squares in each line (represented by (row,column) coordinates) should always be equal to size
        // For example, if the input size = 2, then the output would be: 
        //     seq [seq[(0,0);(0,1)];seq[(1,0);(1,1)];seq[(0,0);(1,0)];seq[(0,1);(1,1)];seq[(0,0);(1,1)];seq[(0,1);(1,0)]]
        // The order of the lines and the order of the squares within each line does not matter
        let Lines (size:int) : seq<seq<int*int>> = 
            let allPossibleCoordinates = seq { for row in 0 .. size-1 do for col in 0 .. size-1 do yield(row, col)}
            let getLine filter : seq<int*int> =
                allPossibleCoordinates
                |> Seq.filter filter

            let horizontalLines = seq { for i in 0 .. size-1 do yield getLine (fun (a,b) -> a = i)}
            let verticalLines = seq { for i in 0 .. size-1 do yield getLine (fun (a,b) -> b = i)}
            let diagonalLeftToRight = seq { for i in 0 .. size-1 do yield getLine (fun (a,b) -> a = b)}
            let diagonalRightToLeft = seq { for i in 0 .. size-1 do yield getLine (fun (a,b) -> a + b = size-1)}
            Seq.concat [ horizontalLines; verticalLines; diagonalLeftToRight; diagonalRightToLeft]

        // Checks a single line (specified as a sequence of (row,column) coordinates) to determine if one of the players
        // has won by filling all of those squares, or a Draw if the line contains at least one Nought and one Cross
        let CheckLine (game:GameState) (line:seq<int*int>) : TicTacToeOutcome<Player> = 
            line
            |> Seq.map (fun (row,col) -> getPlayerAt game row col)
            |> Seq.reduce (+)
            |> fun reducedLine ->
                match reducedLine with
                | "OOO" -> Win (winner = Nought, line = line)
                | "XXX" -> Win (winner = Cross, line = line)
                | "XOX" -> Draw
                | "OXO" -> Draw
                | _ -> Undecided

        let GameOutcome game : TicTacToeOutcome<Player>= raise (System.NotImplementedException("MiniMaxWithPruning"))
            //Lines game.Size
            //|> Seq.map (fun line -> CheckLine game line)
            //|> fun seq ->
            //    if Seq.exists (fun line -> line = Draw) seq
            //    then Undecided
            //Undecided

        let GameStart (firstPlayer:Player) size =
            { 
            Turn = firstPlayer; 
            Size = size; 
            Board = Seq.toList <| seq {for row in 0 ..size-1 do for col in 0 .. size-1 do yield(row, col, "")}
            }                                                                                  

        let MiniMax game = raise (System.NotImplementedException("MiniMax"))

        let MiniMaxWithPruning game = raise (System.NotImplementedException("MiniMaxWithPruning"))

        // plus other helper functions ...

        [<AbstractClass>]
        type Model() =
            abstract member FindBestMove : GameState -> Move
            interface ITicTacToeModel<GameState, Move, Player> with
                member this.Cross with get()             = Cross 
                member this.Nought with get()            = Nought 
                member this.GameStart(firstPlayer, size) = GameStart firstPlayer size
                member this.CreateMove(row, col)         = CreateMove row col
                member this.GameOutcome(game)            = GameOutcome game
                member this.ApplyMove(game, move)        = ApplyMove game move 
                member this.FindBestMove(game)           = this.FindBestMove game

        type BasicMiniMax() =
            inherit Model()
            override this.ToString()         = "Pure F# with basic MiniMax";
            override this.FindBestMove(game) = raise (System.NotImplementedException("FindBestMove"))


        type WithAlphaBetaPruning() =
            inherit Model()
            override this.ToString()         = "Pure F# with Alpha Beta Pruning";
            override this.FindBestMove(game) = raise (System.NotImplementedException("FindBestMove"))