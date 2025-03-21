using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AIEvaluator
{
    // 패턴 가중치 상수
    public struct PatternScore
    {
        // AI 패턴 점수
        public const float FIVE_IN_A_ROW = 100000f;
        public const float OPEN_FOUR = 15000f;
        public const float HALF_OPEN_FOUR = 5000f;
        public const float CLOSED_FOUR = 500f;
        public const float OPEN_THREE = 3000f;
        public const float HALF_OPEN_THREE = 500f;
        public const float CLOSED_THREE = 50f;
        public const float OPEN_TWO = 100f;
        public const float HALF_OPEN_TWO = 30f;
        public const float CLOSED_TWO = 10f;
        public const float OPEN_ONE = 10f;
        public const float CLOSED_ONE = 1f;
        
        // 복합 패턴 점수
        public const float DOUBLE_THREE = 8000f;
        public const float DOUBLE_FOUR = 12000f;
        public const float FOUR_THREE = 10000f;
        
        // 위치 가중치 기본값
        public const float CENTER_WEIGHT = 1.2f;
        public const float EDGE_WEIGHT = 0.8f;
    }

    private static readonly int[][] Directions = AIConstants.Directions;
    
    // 보드 전체 상태 평가
    public static float EvaluateBoard(Enums.PlayerType[,] board, Enums.PlayerType aiPlayer)
    {
        float score = 0;
        int size = board.GetLength(0);
        
        // 복합 패턴 감지를 위한 위치 저장 리스트
        List<(int row, int col, int[] dir)> aiOpen3Positions = new List<(int, int, int[])>();
        List<(int row, int col, int[] dir)> playerOpen3Positions = new List<(int, int, int[])>();
        List<(int row, int col, int[] dir)> ai4Positions = new List<(int, int, int[])>();
        List<(int row, int col, int[] dir)> player4Positions = new List<(int, int, int[])>();

        // 1. 기본 패턴 평가
        score += EvaluateBoardPatterns(board, aiPlayer, size, aiOpen3Positions, playerOpen3Positions, 
                                      ai4Positions, player4Positions);
        
        // 2. 복합 패턴 평가
        score += EvaluateComplexPatterns(aiOpen3Positions, playerOpen3Positions, ai4Positions, player4Positions, aiPlayer);
        
        return score;
    }
    
    // 기본 패턴 (돌의 연속, 열린 끝 등) 평가
    private static float EvaluateBoardPatterns(
        Enums.PlayerType[,] board, 
        Enums.PlayerType aiPlayer,
        int size,
        List<(int row, int col, int[] dir)> aiOpen3Positions,
        List<(int row, int col, int[] dir)> playerOpen3Positions,
        List<(int row, int col, int[] dir)> ai4Positions,
        List<(int row, int col, int[] dir)> player4Positions)
    {
        float score = 0;
        Enums.PlayerType opponentPlayer = (aiPlayer == Enums.PlayerType.PlayerA) ? 
                                      Enums.PlayerType.PlayerB : Enums.PlayerType.PlayerA;
        
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] == Enums.PlayerType.None) continue;

                Enums.PlayerType currentPlayer = board[row, col];
                int playerScore = (currentPlayer == aiPlayer) ? 1 : -1; // AI는 양수, 플레이어는 음수
                
                // 위치 가중치 계산
                float positionWeight = CalculatePositionWeight(row, col, size);

                foreach (var dir in Directions)
                {
                    var (count, openEnds) = MiniMaxAIController.CountStones(board, row, col, dir, currentPlayer);

                    // 기본 패턴 평가 및 점수 계산
                    float patternScore = EvaluatePattern(count, openEnds);
                    
                    // 깨진 패턴 평가
                    var (isBroken, brokenCount, brokenOpenEnds) = 
                        DetectBrokenPattern(board, row, col, dir, currentPlayer);
                
                    if (isBroken) // 깨진 패턴이 있을 시 비교 후 더 높은 점수 할당
                    {
                        float brokenScore = EvaluateBrokenPattern(brokenCount, brokenOpenEnds);
                        patternScore = Math.Max(patternScore, brokenScore);
                    }
                    
                    // 패턴 수집 (복합 패턴 감지용)
                    CollectPatterns(row, col, dir, count, openEnds, currentPlayer, aiPlayer,
                                  aiOpen3Positions, playerOpen3Positions, ai4Positions, player4Positions);
    
                    // 위치 가중치 적용
                    patternScore *= positionWeight;
                    
                    // 최종 점수 (플레이어는 음수)
                    score += playerScore * patternScore;
                }
            }
        }
        
        return score;
    }
    
    // 개별 패턴 평가 (돌 개수와 열린 끝 기준)
    private static float EvaluatePattern(int count, int openEnds)
    {
        if (count >= 5)
        {
            return PatternScore.FIVE_IN_A_ROW;
        }
        else if (count == 4)
        {
            return (openEnds == 2) ? PatternScore.OPEN_FOUR : 
                   (openEnds == 1) ? PatternScore.HALF_OPEN_FOUR : 
                                     PatternScore.CLOSED_FOUR;
        }
        else if (count == 3)
        {
            return (openEnds == 2) ? PatternScore.OPEN_THREE : 
                   (openEnds == 1) ? PatternScore.HALF_OPEN_THREE : 
                                     PatternScore.CLOSED_THREE;
        }
        else if (count == 2)
        {
            return (openEnds == 2) ? PatternScore.OPEN_TWO : 
                   (openEnds == 1) ? PatternScore.HALF_OPEN_TWO : 
                                     PatternScore.CLOSED_TWO;
        }
        else if (count == 1)
        {
            return (openEnds == 2) ? PatternScore.OPEN_ONE : PatternScore.CLOSED_ONE;
        }
        
        return 0;
    }
    
    // 복합 패턴 평가 (3-3, 4-4, 4-3 등)
    private static float EvaluateComplexPatterns(
        List<(int row, int col, int[] dir)> aiOpen3Positions,
        List<(int row, int col, int[] dir)> playerOpen3Positions,
        List<(int row, int col, int[] dir)> ai4Positions,
        List<(int row, int col, int[] dir)> player4Positions,
        Enums.PlayerType aiPlayer)
    {
        float score = 0;
        
        // 삼삼(3-3) 감지
        int aiThreeThree = DetectDoubleThree(aiOpen3Positions);
        int playerThreeThree = DetectDoubleThree(playerOpen3Positions);
    
        // 사사(4-4) 감지
        int aiFourFour = DetectDoubleFour(ai4Positions);
        int playerFourFour = DetectDoubleFour(player4Positions);
    
        // 사삼(4-3) 감지
        int aiFourThree = DetectFourThree(ai4Positions, aiOpen3Positions);
        int playerFourThree = DetectFourThree(player4Positions, playerOpen3Positions);
    
        // 복합 패턴 점수 합산
        score += aiThreeThree * PatternScore.DOUBLE_THREE;
        score -= playerThreeThree * PatternScore.DOUBLE_THREE;
    
        score += aiFourFour * PatternScore.DOUBLE_FOUR;
        score -= playerFourFour * PatternScore.DOUBLE_FOUR;
    
        score += aiFourThree * PatternScore.FOUR_THREE;
        score -= playerFourThree * PatternScore.FOUR_THREE;
        
        return score;
    }
    
    // 위치 가중치 계산 함수
    private static float CalculatePositionWeight(int row, int col, int size)
    {
        float boardCenterPos = (size - 1) / 2.0f;
    
        // 현재 위치와 중앙과의 거리 계산 (0~1 사이 값)
        float distance = Math.Max(Math.Abs(row - boardCenterPos), Math.Abs(col - boardCenterPos)) / boardCenterPos;
    
        // 중앙(거리 0)은 1.2배, 가장자리(거리 1)는 0.8배
        return PatternScore.CENTER_WEIGHT - ((PatternScore.CENTER_WEIGHT - PatternScore.EDGE_WEIGHT) * distance);
    }
    
    // 방향이 평행한지 확인하는 함수
    private static bool AreParallelDirections(int[] dir1, int[] dir2) // Vector로 변경
    {
        return (dir1[0] == dir2[0] && dir1[1] == dir2[1]) || 
               (dir1[0] == -dir2[0] && dir1[1] == -dir2[1]);
    }
    
    // 패턴 수집 함수 (복합 패턴 감지용)
    private static void CollectPatterns(
        int row, int col, int[] dir, int count, int openEnds, 
        Enums.PlayerType currentPlayer, Enums.PlayerType aiPlayer,
        List<(int row, int col, int[] dir)> aiOpen3Positions,
        List<(int row, int col, int[] dir)> playerOpen3Positions,
        List<(int row, int col, int[] dir)> ai4Positions,
        List<(int row, int col, int[] dir)> player4Positions)
    {
        // 열린 3 패턴 수집
        if (count == 3 && openEnds == 2)
        {
            if (currentPlayer == aiPlayer)
                aiOpen3Positions.Add((row, col, dir));
            else
                playerOpen3Positions.Add((row, col, dir));
        }
        
        // 4 패턴 수집
        if (count == 4 && openEnds >= 1)
        {
            if (currentPlayer == aiPlayer)
                ai4Positions.Add((row, col, dir));
            else
                player4Positions.Add((row, col, dir));
        }
    }

    #region Complex Pattern (3-3, 4-4, 4-3)
    // 삼삼(3-3) 감지 함수
    private static int DetectDoubleThree(List<(int row, int col, int[] dir)> openThreePositions)
    {
        int doubleThreeCount = 0;
        var checkedPairs = new HashSet<(int, int)>();
        
        for (int i = 0; i < openThreePositions.Count; i++)
        {
            var (row1, col1, dir1) = openThreePositions[i];
            
            for (int j = i + 1; j < openThreePositions.Count; j++)
            {
                var (row2, col2, dir2) = openThreePositions[j];
                
                // 같은 돌에서 다른 방향으로 두 개의 열린 3이 형성된 경우
                if (row1 == row2 && col1 == col2 && !AreParallelDirections(dir1, dir2))
                {
                    if (!checkedPairs.Contains((row1, col1)))
                    {
                        doubleThreeCount++;
                        checkedPairs.Add((row1, col1));
                    }
                }
            }
        }
        
        return doubleThreeCount;
    }
    
    // 사사(4-4) 감지 함수
    private static int DetectDoubleFour(List<(int row, int col, int[] dir)> fourPositions)
    {
        int doubleFourCount = 0;
        var checkedPairs = new HashSet<(int, int)>();
        
        for (int i = 0; i < fourPositions.Count; i++)
        {
            var (row1, col1, dir1) = fourPositions[i];
            
            for (int j = i + 1; j < fourPositions.Count; j++)
            {
                var (row2, col2, dir2) = fourPositions[j];
                
                if (row1 == row2 && col1 == col2 && !AreParallelDirections(dir1, dir2))
                {
                    if (!checkedPairs.Contains((row1, col1)))
                    {
                        doubleFourCount++;
                        checkedPairs.Add((row1, col1));
                    }
                }
            }
        }
        
        return doubleFourCount;
    }
    
    // 사삼(4-3) 감지 함수
    private static int DetectFourThree(
        List<(int row, int col, int[] dir)> fourPositions, 
        List<(int row, int col, int[] dir)> openThreePositions)
    {
        int fourThreeCount = 0;
        var checkedPairs = new HashSet<(int, int)>();
        
        foreach (var (row1, col1, _) in fourPositions)
        {
            foreach (var (row2, col2, _) in openThreePositions)
            {
                // 같은 돌에서 4와 열린 3이 동시에 형성된 경우
                if (row1 == row2 && col1 == col2)
                {
                    if (!checkedPairs.Contains((row1, col1)))
                    {
                        fourThreeCount++;
                        checkedPairs.Add((row1, col1));
                    }
                }
            }
        }
        
        return fourThreeCount;
    }
    
    // 깨진 패턴 (3-빈칸-1) 감지
    private static (bool isDetected, int count, int openEnds) DetectBrokenPattern(
        Enums.PlayerType[,] board, int row, int col, int[] dir, Enums.PlayerType player)
    {
        int size = board.GetLength(0);
        int totalStones = 1;  // 현재 위치의 돌 포함
        int gapCount = 0;     // 빈칸 개수
        int openEnds = 0;     // 열린 끝 개수
        
        // 정방향 탐색
        int r = row, c = col;
        for (int i = 1; i <= 5; i++) 
        {
            r += dir[0];
            c += dir[1];
            
            if (r < 0 || r >= size || c < 0 || c >= size)
                break;
                
            if (board[r, c] == player)
            {
                totalStones++;
            }
            else if (board[r, c] == Enums.PlayerType.None)
            {
                if (gapCount < 1)  // 최대 1개 빈칸만 허용
                {
                    gapCount++;
                }
                else
                {
                    openEnds++;
                    break;
                }
            }
            else  // 상대방 돌
            {
                break;
            }
        }
        
        // 역방향 탐색
        r = row; c = col;
        for (int i = 1; i <= 5; i++)
        {
            r -= dir[0];
            c -= dir[1];
            
            if (r < 0 || r >= size || c < 0 || c >= size)
                break;
                
            if (board[r, c] == player)
            {
                totalStones++;
            }
            else if (board[r, c] == Enums.PlayerType.None)
            {
                if (gapCount < 1)
                {
                    gapCount++;
                }
                else
                {
                    openEnds++;
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        // 깨진 패턴 감지: 총 돌 개수 ≥ 4 그리고 빈칸이 1개
        bool isDetected = (totalStones >= 4 && gapCount == 1);
        
        return (isDetected, totalStones, openEnds);
    }
    
    // 깨진 패턴 점수 계산 함수
    private static float EvaluateBrokenPattern(int count, int openEnds)
    {
        if (count >= 5)  // 5개 이상 돌이 있으면 승리
        {
            return PatternScore.FIVE_IN_A_ROW;
        }
        else if (count == 4)  // 깨진 4
        {
            return (openEnds == 2) ? PatternScore.OPEN_FOUR * 0.8f : 
                (openEnds == 1) ? PatternScore.HALF_OPEN_FOUR * 0.8f : 
                PatternScore.CLOSED_FOUR * 0.7f;
        }
        else if (count == 3)  // 깨진 3
        {
            return (openEnds == 2) ? PatternScore.OPEN_THREE * 0.7f : 
                (openEnds == 1) ? PatternScore.HALF_OPEN_THREE * 0.7f : 
                PatternScore.CLOSED_THREE * 0.6f;
        }
        
        return 0;
    }
    
    #endregion
    
    #region Evaluate Move Position
    // 이동 평가 함수
    public static float EvaluateMove(Enums.PlayerType[,] board, int row, int col, Enums.PlayerType AIPlayer)
    {
        float score = 0;
        Enums.PlayerType opponentPlayer = (AIPlayer == Enums.PlayerType.PlayerA) ? 
                                      Enums.PlayerType.PlayerB : Enums.PlayerType.PlayerA;
        
        // 복합 패턴 감지를 위한 위치 저장 리스트
        List<(int[] dir, int count, int openEnds)> aiPatterns = new List<(int[], int, int)>();
        List<(int[] dir, int count, int openEnds)> opponentPatterns = new List<(int[], int, int)>();
        
        board[row, col] = AIPlayer;
        
        foreach (var dir in Directions)
        {
            float directionScore = 0;
            
            var (count, openEnds) = MiniMaxAIController.CountStones(board, row, col, dir, AIPlayer, false);
            aiPatterns.Add((dir, count, openEnds));
            
            float normalScore = 0;
            if (count >= 4) 
            {
                normalScore = PatternScore.FIVE_IN_A_ROW / 10;
            }
            else if (count == 3)
            {
                normalScore = (openEnds == 2) ? PatternScore.OPEN_THREE / 3 : 
                         (openEnds == 1) ? PatternScore.HALF_OPEN_THREE / 5 : 
                                           PatternScore.CLOSED_THREE / 5;
            }
            else if (count == 2)
            {
                normalScore = (openEnds == 2) ? PatternScore.OPEN_TWO / 2 : 
                         (openEnds == 1) ? PatternScore.HALF_OPEN_TWO / 3 : 
                                           PatternScore.CLOSED_TWO / 5;
            }
            else if (count == 1)
            {
                normalScore = (openEnds == 2) ? PatternScore.OPEN_ONE : 
                                           PatternScore.CLOSED_ONE;
            }
            
            // 깨진 패턴 평가
            var (isBroken, brokenCount, brokenOpenEnds) = DetectBrokenPattern(board, row, col, dir, AIPlayer);
            float brokenScore = 0;
            
            if (isBroken)
            {
                brokenScore = EvaluateBrokenPattern(brokenCount, brokenOpenEnds);
            }
            
            directionScore = Math.Max(normalScore, brokenScore);
            
            score += directionScore; // 공격 점수 누적
        }
        
        // AI 복합 패턴 점수 계산
        score += EvaluateComplexMovePatterns(aiPatterns, true);
        
        // 상대 관점에서 평가 (방어 가치)
        board[row, col] = opponentPlayer;
        
        foreach (var dir in Directions)
        {
            float directionScore = 0;
            
            var (count, openEnds) = MiniMaxAIController.CountStones(board, row, col, dir, opponentPlayer, false);
            opponentPatterns.Add((dir, count, openEnds));
            
            float normalScore = 0;
            // 상대 패턴 차단에 대한 가치 (약간 낮은 가중치) AI는 공격지향적으로
            if (count >= 4)
            {
                normalScore = PatternScore.FIVE_IN_A_ROW / 12.5f; 
            }
            else if (count == 3)
            {
                normalScore = (openEnds == 2) ? PatternScore.OPEN_THREE / 3.75f : 
                         (openEnds == 1) ? PatternScore.HALF_OPEN_THREE / 6.25f : 
                                           PatternScore.CLOSED_THREE / 6.25f;
            }
            else if (count == 2)
            {
                normalScore = (openEnds == 2) ? PatternScore.OPEN_TWO / 2.5f : 
                         (openEnds == 1) ? PatternScore.HALF_OPEN_TWO / 3.75f : 
                                           PatternScore.CLOSED_TWO / 5f;
            }
            else if (count == 1)
            {
                normalScore = (openEnds == 2) ? PatternScore.OPEN_ONE / 1.25f : 
                                           PatternScore.CLOSED_ONE;
            }
            
            var (isBroken, brokenCount, brokenOpenEnds) = DetectBrokenPattern(board, row, col, dir, opponentPlayer);
            float brokenScore = 0;
            
            if (isBroken)
            {
                // 깨진 패턴은 일반 패턴보다 좀 더 높은 가중치 할당
                brokenScore = EvaluateBrokenPattern(brokenCount, brokenOpenEnds) * 0.9f;
            }
            
            directionScore = Math.Max(normalScore, brokenScore);
            
            score += directionScore; // 방어 점수 누적
        }
        
        score += EvaluateComplexMovePatterns(opponentPatterns, false);
        
        board[row, col] = Enums.PlayerType.None; // 복원
        
        int size = board.GetLength(0);
        float centerDistance = Math.Max(
            Math.Abs(row - (size - 1) / 2.0f), // 중앙 위치 계산
            Math.Abs(col - (size - 1) / 2.0f)
        );
        float centerBonus = 1.0f - (centerDistance / ((size - 1) / 2.0f)) * 0.3f; // 중앙과 가장자리의 점수 차이를 30%로 설정
        
        return score * centerBonus;
    }
    
    // 복합 패턴 평가를 위한 새로운 함수
    private static float EvaluateComplexMovePatterns(List<(int[] dir, int count, int openEnds)> patterns, bool isAI)
    {
        float score = 0;
    
        // 열린 3 패턴 및 4 패턴 찾기
        var openThrees = patterns.Where(p => p.count == 3 && p.openEnds == 2).ToList();
        var fours = patterns.Where(p => p.count == 4 && p.openEnds >= 1).ToList();
    
        // 3-3 패턴 감지
        if (openThrees.Count >= 2)
        {
            for (int i = 0; i < openThrees.Count; i++)
            {
                for (int j = i + 1; j < openThrees.Count; j++)
                {
                    if (!AreParallelDirections(openThrees[i].dir, openThrees[j].dir))
                    {
                        float threeThreeScore = PatternScore.DOUBLE_THREE / 4; // 복합 패턴 가중치
                        score += isAI ? threeThreeScore : threeThreeScore;
                        break;
                    }
                }
            }
        }
    
        // 4-4 패턴 감지
        if (fours.Count >= 2)
        {
            for (int i = 0; i < fours.Count; i++)
            {
                for (int j = i + 1; j < fours.Count; j++)
                {
                    if (!AreParallelDirections(fours[i].dir, fours[j].dir))
                    {
                        float fourFourScore = PatternScore.DOUBLE_FOUR / 4;
                        score += isAI ? fourFourScore : fourFourScore;
                        break;
                    }
                }
            }
        }
    
        // 4-3 패턴 감지
        if (fours.Count > 0 && openThrees.Count > 0)
        {
            float fourThreeScore = PatternScore.FOUR_THREE / 4;
            score += isAI ? fourThreeScore : fourThreeScore;
        }
    
        return score;
    }
    
    #endregion
}
