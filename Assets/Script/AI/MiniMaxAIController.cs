using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MiniMaxAIController
{
    // To-Do List
    // 탐색 시간 개선
    // 코드 중복 제거
    // AI 난이도 개선
    
    private const int SEARCH_DEPTH = 3; // 탐색 깊이 제한 (3 = 빠른 응답, 4 = 좀 더 강한 AI 그러나 느린)
    private const int WIN_COUNT = 5;
    
    private static int[][] _directions = new int[][]
    {
        new int[] {1, 0}, // 수직
        new int[] {0, 1}, // 수평
        new int[] {1, 1}, // 대각선 ↘ ↖
        new int[] {1, -1} // 대각선 ↙ ↗
    };

    private static int _playerLevel = 1; // 급수 설정
    private static float _mistakeMove;
    private static Enums.PlayerType _AIPlayerType = Enums.PlayerType.PlayerB;

    // 급수 설정 -> 실수 넣을 때 계산
    public static void SetLevel(int level)
    {
        _playerLevel = level;
        
        _mistakeMove = GetMistakeProbability(_playerLevel);
    }
    
    // 실수 확률 계산 함수
    private static float GetMistakeProbability(int level)
    {
        // 레벨이 1일 때 실수 확률 0%, 레벨이 18일 때 실수 확률 50%
        return (level - 1) / 17f * 0.5f;
    }
    
    // return 값이 null 일 경우 == 보드에 칸 꽉 참
    public static (int row, int col)? GetBestMove(Enums.PlayerType[,] board)
    {
        float bestScore = float.MinValue;
        (int row, int col)? bestMove = null;
        (int row, int col)? secondBestMove = null;
        List<(int row, int col)> validMoves = GetValidMoves(board);
        
        if (validMoves.Count == 0)
        {
            Debug.Log("칸이 없습니다...");
            return null;
        }
        
        // 5연승 가능한 자리를 먼저 찾아서 우선적으로 설정
        List<(int row, int col)> fiveInARowMoves = GetFiveInARowCandidateMoves(board);
        if (fiveInARowMoves.Count > 0)
        {
            bestMove = fiveInARowMoves[0]; 
            return bestMove;
        }
        
        foreach (var (row, col) in validMoves)
        {
            board[row, col] = _AIPlayerType;
            float score = DoMinimax(board, SEARCH_DEPTH, false, -1000, 1000, row, col);
            board[row, col] = Enums.PlayerType.None;

            if (score > bestScore)
            {
                bestScore = score;

                if (bestMove != null)
                {
                    secondBestMove = bestMove;
                }
                
                bestMove = (row, col);
            }
        }

        // 랜덤 실수
        if (secondBestMove != null && UnityEngine.Random.value < _mistakeMove) // UnityEngine.Random.value == 0~1 사이 반환
        {
            Debug.Log("AI Mistake");
            return secondBestMove;
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

    // 이동 가능 + 주변에 돌 있는 위치 탐색
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

    // 최근에 둔 돌 위치 기반으로 게임 승리를 판별하는 함수
    public static bool CheckGameWin(Enums.PlayerType player, Enums.PlayerType[,] board, int row, int col)
    {
        int size = board.GetLength(0);
        
        // 각 방향별로 판단
        foreach (var dir in _directions)
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

        // 각 칸에 대해 5연승이 될 수 있는 위치 찾기
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] != Enums.PlayerType.None)
                    continue; // 이미 돌이 놓인 곳

                foreach (var dir in _directions)
                {
                    int count = 0;
                    int openEnds = 0;
                    
                    // 왼쪽 방향 확인
                    int r = row + dir[0], c = col + dir[1];
                    while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == _AIPlayerType)
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
                    while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == _AIPlayerType)
                    {
                        count++;
                        r -= dir[0];
                        c -= dir[1];
                    }
                    if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
                        openEnds++;
                    
                    if (count == 4 && openEnds > 0)
                    {
                        fiveInARowMoves.Add((row, col));
                    }
                }
            }
        }

        return fiveInARowMoves;
    }
    
    // 현재 보드의 상태 평가
    private static float EvaluateBoard(Enums.PlayerType[,] board)
    {
        float score = 0;
        int size = board.GetLength(0);

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] == Enums.PlayerType.None) continue;

                Enums.PlayerType player = board[row, col];
                int playerScore = (player == _AIPlayerType) ? 1 : -1; // AI는 양수, 상대는 음수

                foreach (var dir in _directions)
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
                        r -= dir[0];
                        c -= dir[1];
                    }

                    if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
                        openEnds++;

                    // 점수 계산
                    if (count == 4)
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
