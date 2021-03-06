namespace QUT

    module FSharpPureTicTacToeModel =

        // type to represent the two players: Noughts and Crosses
        type Player = Nought | Cross 

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
                member this.getPiece(row, col) = 
                    this.Board
                    |> List.find(fun (rowInt,colInt,playerString) -> rowInt = row && colInt = col)
                    |> fun(rowInt,colInt,playerString) -> playerString
                    

        // returns a string to represent a player piece on the board
        // "X" for Cross, "O" for Nought and "" for none
        let getPiece (player: Player) : string =
            match player with
            | Nought -> "O"
            | Cross -> "X"

        // returns a string representing which player occupies a given row col
        let getPlayerStringAt (game:GameState) row col : string =
            game.Board
            |> List.find(fun (rowInt,colInt,playerString) -> rowInt = row && colInt = col)
            |> fun(rowInt,colInt,playerString) -> playerString

        // takes a sequence of outcomes seq<TicTacToeOutcome>
        // returns true if a payer has won or false if no player has won 
        let isGameWon outcomes =
            outcomes
            |> Seq.exists (fun outcome -> 
                 match outcome with
                 | Win (a, b) -> a = Nought || a = Cross
                 | _ -> false)

        // takes a sequence of outcomes seq<TicTacToeOutcome>
        // returns the player that has won the game
        let getWinner outcomes =
            outcomes 
            |> Seq.find (fun outcome -> 
                match outcome with
                | Win (a, b) -> a = Nought || a = Cross
                | _ -> false)

        // create a record type representing a move on the board
        let CreateMove row col = {Row = row; Column = col}
        
        // swaps the players turn
        let swapTurn turn =
            match turn with
            | Nought -> Cross
            | Cross -> Nought

        // returns a new board representing the board state after a move is applied 
        // applies a "X" or "O" at position move.row, move.column
        let getNewBoardAfterMove board move turn =
            board
            |> List.map (fun (row,col,piece) -> 
                if row = move.Row && col = move.Column 
                then (row,col, getPiece turn) 
                else (row,col,piece)) 
                
        // returns a new GameState which represents the state of the game after a move is applied
        // applies the move on the board and swaps the player turn 
        let ApplyMove (oldState:GameState) (move: Move) : GameState = 
            let newPlayerTurn = swapTurn oldState.Turn
            let newBoardState = getNewBoardAfterMove oldState.Board move oldState.Turn
            {
            Turn = newPlayerTurn; 
            Size = oldState.Size; 
            Board = newBoardState
            }

        // creates a new line on the board by applying a filter to a given set of coordinates
        let getLine coordinates filter : seq<int*int> =
            coordinates
            |> Seq.filter filter

        // Returns a sequence containing all of the lines on the board: Horizontal, Vertical and Diagonal
        // each line is a sequence of row,col coordinates 
        // example output : seq [seq[(0,0);(0,1)];seq[(1,0);(1,1) 
        let Lines (size:int) : seq<seq<int*int>> = 
            let allPossibleCoordinates = seq { for row in 0 .. size-1 do for col in 0 .. size-1 do yield(row, col)}

            let horizontalLines = seq { for i in 0 .. size-1 do yield getLine allPossibleCoordinates (fun (row,col) -> row = i)}
            let verticalLines = seq { for i in 0 .. size-1 do yield getLine allPossibleCoordinates (fun (row,col) -> col = i)}
            let diagonalLeftToRight = seq { for i in 0 .. size-1 do yield getLine allPossibleCoordinates (fun (row,col) -> row = col)}
            let diagonalRightToLeft = seq { for i in 0 .. size-1 do yield getLine allPossibleCoordinates (fun (row,col) -> row + col = size-1)}

            Seq.concat [ horizontalLines; verticalLines; diagonalLeftToRight; diagonalRightToLeft]


        // returns a sequence of players along a given line 
        // Example output: seq["";"X";"O"] 
        let findPlayersAlongLine game line =
            line
            |> Seq.map(fun (row, col) -> getPlayerStringAt game row col)

        // Checks a single line (specified as a sequence of (row,column) coordinates) to determine if one of the players
        // has won by filling all of those squares, or a Draw if the line contains at least one Nought and one Cross
        let CheckLine (game:GameState) (line:seq<int*int>) : TicTacToeOutcome<Player> = 
            let players = findPlayersAlongLine game line
            let noughtCount = 
                players
                |> Seq.filter(fun (player) -> player = "O")
                |> fun noughts -> Seq.length noughts
            let crossCount =
                players
                |> Seq.filter(fun (player) -> player = "X")
                |> fun crosses -> Seq.length crosses

            if (noughtCount = game.Size) then Win (winner = Nought, line = line)
            else if (crossCount = game.Size) then Win (winner = Cross, line = line)
            else if (noughtCount >= 1 && crossCount >= 1) then Draw
            else Undecided


        // evaluate the current outcome of the game Win, Daw or Undecided
        let GameOutcome (game:GameState) : TicTacToeOutcome<Player> = 
            let outcomes =
                Lines game.Size
                |> Seq.map (fun line -> CheckLine game line)

            if outcomes |> Seq.forall (fun outcome -> outcome = Draw) then Draw 
            else if outcomes |> isGameWon then outcomes |> getWinner
            else Undecided


        // generate an empty board where each row,col is ""
        let GetEmptyBoard size =
            Seq.toList <| seq {for row in 0 ..size-1 do 
                                for col in 0 .. size-1 do 
                                    yield(row, col, "")}

        // returns a new game where each square is empty
        let GameStart (firstPlayer:Player) size =
            { 
            Turn = firstPlayer; 
            Size = size; 
            Board = GetEmptyBoard size
            }                                              

        //*****************************************************************************************//
        //*************************  Helper functions for MiniMax only ****************************//
        //*****************************************************************************************//

        // heuristic to evalute the outcome of a game  
        // +1 for a win, -1 for a loss, 0 for a draw 
        let heuristic game perspectve =
            match GameOutcome game with
            | Draw -> 0
            | Win (player, line) ->
                if player = perspectve then +1
                else -1
            | Undecided -> raise (System.Exception("A heuristic should only be evaluted when the game is over (not undecided)"))

        // returns the current players turn from a given game
        let getTurn game = game.Turn

        // determine if a game is over
        let gameOver game =
            match GameOutcome game with
            | Undecided -> false
            | _ -> true

        // enumarate all possible moves from a given board state 
        let moveGenerator game : seq<Move> =
            game.Board
            |> Seq.filter (fun (row,col,player) -> player = "") //find only empty spaces on the board
            |> Seq.map (fun (row, col, player) -> CreateMove row col) 

        //*****************************************************************************************//
        //*********************  END helper functions for MiniMax only ****************************//
        //*****************************************************************************************//

        let MiniMax game = 
            let MiniMaxFunction = GameTheory.MiniMaxGenerator heuristic getTurn gameOver moveGenerator ApplyMove
            let bestMove = MiniMaxFunction game game.Turn
            match bestMove with
            | (move, score) ->
                match move with
                | Some move -> move
                | _ -> raise (System.Exception("MiniMax should not be called if there are no moves left"))
                


        let MiniMaxWithPruning game = 
            let MiniMaxFunction = GameTheory.MiniMaxWithAlphaBetaPruningGenerator heuristic getTurn gameOver moveGenerator ApplyMove
            let bestMove = MiniMaxFunction -1 1 game game.Turn
            match bestMove with
            | (move, score) ->
              match move with
              | Some move -> move  
              | _ -> raise (System.Exception("MiniMax should not be called if there are no moves left"))    

        

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
            override this.FindBestMove(game) = MiniMax game
            

        type WithAlphaBetaPruning() =
            inherit Model()
            override this.ToString()         = "Pure F# with Alpha Beta Pruning";
            override this.FindBestMove(game) = MiniMaxWithPruning game