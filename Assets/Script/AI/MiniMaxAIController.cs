using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MiniMaxAIController
{
    // To-Do List
    // 랜덤 실수 (랜덤하게 덜 좋은 수 리턴)
    // 탐색 시간 개선
    // 방어적인 플레이라 AI 자신이 5연승할 자리에 안 둠 -> 해결
    
    private const int SEARCH_DEPTH = 3; // 탐색 깊이 제한 (3 = 빠른 응답, 4 = 좀 더 강한 AI 그러나 느린)
    private const int WIN_COUNT = 5;

    private static int _playerLevel; // 급수 설정

    // 급수 설정 -> 실수 넣을 때 계산
    public static void SetLevel(int level)
    {
        _playerLevel = level;
    }
    
    public static (int row, int col)? GetBestMove(Enums.PlayerType[,] board)
    {
        float bestScore = -1000;
        (int row, int col)? bestMove = null;
        List<(int row, int col)> validMoves = GetValidMoves(board);
        
        if (validMoves.Count == 0) // 놓을 수 있는 칸 없음 == 칸 꽉 참
        {
            Debug.Log("칸이 없습니다...");
            return null;
        }
        
        // To-Do : bestMove는 null로 유지하고 맨 마지막 리턴 문에서 삼항 연산자로 날리기(Second용)
        bestMove = validMoves[0]; // 기본 값, null 반환 방지. 
        
        // 5연승 가능한 자리를 먼저 찾아서 우선적으로 설정
        List<(int row, int col)> fiveInARowMoves = GetFiveInARowCandidateMoves(board);
        if (fiveInARowMoves.Count > 0)
        {
            bestMove = fiveInARowMoves[0]; 
            Debug.Log($"5 wins move {bestMove.Value.row}, {bestMove.Value.col}");
            return bestMove;
        }
        
        foreach (var (row, col) in validMoves)
        {
            board[row, col] = Enums.PlayerType.PlayerB;
            float score = DoMinimax(board, SEARCH_DEPTH, false, -1000, 1000, row, col);
            board[row, col] = Enums.PlayerType.None;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = (row, col); // 초반에는 bestMove가 잘 안바뀌어서 (돌 한 2~3개 둬야 여러 개 나옴) 2라운드까지는 그대로 출력 필요
                // To-Do : 실수용으로 secondBestMove 추가
            }
        }

        return bestMove;
    }

    private static float DoMinimax(Enums.PlayerType[,] board, int depth, bool isMaximizing, float alpha, float beta,
        int recentRow, int recentCol)
    {
        if (CheckGameWin(Enums.PlayerType.PlayerA, board, recentRow, recentCol)) return -100 + depth;
        if (CheckGameWin(Enums.PlayerType.PlayerB, board, recentRow, recentCol)) return 100 - depth;
        if (depth == 0) return EvaluateBoard(board);

        float bestScore = isMaximizing ? float.MinValue : float.MaxValue;
        List<(int row, int col)> validMoves = GetValidMoves(board); // 현재 놓을 수 있는 자리 리스트

        foreach (var (row, col) in validMoves)
        {
            board[row, col] = isMaximizing ? Enums.PlayerType.PlayerB : Enums.PlayerType.PlayerA;
            float score = DoMinimax(board, depth - 1, !isMaximizing, alpha, beta, row, col);
            board[row, col] = Enums.PlayerType.None;

            if (isMaximizing)
            {
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, bestScore);
            }
            else
            {
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, bestScore);
            }
            
            if (beta <= alpha) break;
        }
        return bestScore;
    }

    /// <summary>
    /// 이동 가능 + 주변에 돌 있는 위치 탐색
    /// </summary>
    /// <returns></returns>
    private static List<(int row, int col)> GetValidMoves(Enums.PlayerType[,] board)
    {
        List<(int, int)> validMoves = new List<(int, int)>();
        int size = board.GetLength(0);

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] == Enums.PlayerType.None && HasNearbyStones(board, row, col))
                {
                    validMoves.Add((row, col));
                }
            }
        }
        return validMoves;
    }

    /// <summary>
    /// 주변 8칸에 놓인 돌이 있는 지 확인
    /// </summary>
    /// <param name="row">현재 탐색하는 위치의 row값</param>
    /// <param name="col">현재 탐색하는 위치의 col값</param>
    /// <returns>true: 돌 있음, fasle: 돌 없음</returns>
    private static bool HasNearbyStones(Enums.PlayerType[,] board, int row, int col)
    {
        // 9칸 기준으로 현재 위치를 중앙으로 상정한 후 나머지 8방향
        int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int size = board.GetLength(0);

        for(int i = 0; i < dr.Length; i++)
        {
            int nr = row + dr[i], nc = col + dc[i];
            if (nr >= 0 && nr < size && nc >= 0 && nc < size && board[nr, nc] != Enums.PlayerType.None)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 최근에 둔 돌 위치 기반으로 게임 승리를 판별하는 함수.
    /// </summary>
    /// <param name="player"> 어떤 플레이어 기준으로 승리 판별 </param>
    /// <param name="board"> 게임 보드 </param>
    /// <param name="row"> 최근에 둔 돌의 위치 중 row 값 </param>
    /// <param name="col">최근에 둔 돌의 위치 중 col 값 </param>
    /// <returns> true: 승리, false: 승리 아님 </returns>
    public static bool CheckGameWin(Enums.PlayerType player, Enums.PlayerType[,] board, int row, int col)
    {
        int size = board.GetLength(0);
        
        int[][] directions = new int[][]
        {
            new int[] {1, 0}, // 수직
            new int[] {0, 1}, // 수평
            new int[] {1, 1}, // 대각선 ↘ ↖
            new int[] {1, -1} // 대각선 ↙ ↗
        };
        
        // 각 방향별로 판단
        foreach (var dir in directions)
        {
            // 자기 자신 포함해서 카운트 시작
            int stoneCount = 1;

            // 정방향 탐색
            int r = row + dir[0], c = col + dir[1];
            while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player) // 0~15내에서 플레이어 타입 판단
            {
                // 동일 플레이어 타입인 경우
                stoneCount++;
                r += dir[0]; // row값 옮기기 
                c += dir[1]; // col값 옮기기
            }
            
            // 역방향 탐색 전에 정방향에서 Win했는지 확인 (이미 Win한 상태에서 역방향 검사 방지)
            if (stoneCount >= WIN_COUNT) return true; 

            // 역방향 탐색
            r = row - dir[0]; 
            c = col - dir[1];
            while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player)
            {
                stoneCount++;
                r -= dir[0];
                c -= dir[1];
            }

            if (stoneCount >= WIN_COUNT) return true; 
        }
        
        return false;
    }
    
    private static List<(int row, int col)> GetFiveInARowCandidateMoves(Enums.PlayerType[,] board)
    {
        List<(int row, int col)> fiveInARowMoves = new List<(int, int)>();
        int size = board.GetLength(0);

        // 방향 설정: 가로, 세로, 대각선
        int[][] directions = new int[][]
        {
            new int[] {1, 0}, // 가로
            new int[] {0, 1}, // 세로
            new int[] {1, 1}, // 대각선 (\)
            new int[] {1, -1} // 대각선 (/)
        };

        // 각 칸에 대해 5연승이 될 수 있는 위치 찾기
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] != Enums.PlayerType.None)
                    continue; // 이미 돌이 놓인 곳

                foreach (var dir in directions)
                {
                    int count = 0;
                    int openEnds = 0;
                    // 왼쪽 방향부터 확인
                    int r = row + dir[0], c = col + dir[1];
                    while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.PlayerB)
                    {
                        count++;
                        r += dir[0];
                        c += dir[1];
                    }
                    if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
                        openEnds++;

                    // 오른쪽 방향 확인
                    r = row - dir[0];
                    c = col - dir[1];
                    while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.PlayerB)
                    {
                        count++;
                        r -= dir[0];
                        c -= dir[1];
                    }
                    if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
                        openEnds++;

                    // 5연승이 가능하면 그 자리를 리스트에 추가
                    if (count == 4 && openEnds > 0)
                    {
                        fiveInARowMoves.Add((row, col));
                    }
                }
            }
        }

        return fiveInARowMoves;
    }
    
    /// <summary>
    /// 현재 보드의 상태 평가
    /// </summary>
    /// <returns>평가 후 점수</returns>
    private static float EvaluateBoard(Enums.PlayerType[,] board)
    {
        float score = 0;
        int size = board.GetLength(0);
        int[][] directions = new int[][] 
        {
            new int[] {1, 0},  // 수직
            new int[] {0, 1},  // 수평
            new int[] {1, 1},  // 대각선 ↘
            new int[] {1, -1}  // 대각선 ↙
        };

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] == Enums.PlayerType.None) continue;

                Enums.PlayerType player = board[row, col];
                int playerScore = (player == Enums.PlayerType.PlayerB) ? 1 : -1; // AI는 양수, 상대는 음수

                foreach (var dir in directions)
                {
                    int count = 1;
                    int openEnds = 0;
                    int r = row + dir[0], c = col + dir[1];

                    // 같은 돌 개수 세기
                    while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player)
                    {
                        count++;
                        r += dir[0];
                        c += dir[1];
                    }

                    // 열린 방향 확인
                    if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
                        openEnds++;

                    // 반대 방향 검사
                    r = row - dir[0]; 
                    c = col - dir[1];

                    while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player)
                    {
                        count++;
                        r -= dir[0];
                        c -= dir[1];
                    }

                    if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
                        openEnds++;

                    // 점수 계산
                    if (count >= 5) 
                        score += playerScore * 1000000; // 실제로 호출되는 일이 없음 왜지??
                    else if (count == 4)
                        score += playerScore * (openEnds == 2 ? 10000 : 1000);
                    else if (count == 3)
                        score += playerScore * (openEnds == 2 ? 1000 : 100);
                    else if (count == 2)
                        score += playerScore * (openEnds == 2 ? 100 : 10);
                }
            }
        }

        return score;
    }
}
