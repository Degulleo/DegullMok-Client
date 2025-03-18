using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RenjuRuleChecker: ForbiddenDetectorBase
{
   // 8방향을 나타내는 델타 배열 (가로, 세로, 대각선 방향)
    private static readonly int[,] directions = new int[8, 2]
    {
        { 1, 0 },   // 오른쪽
        { 1, 1 },   // 오른쪽 아래
        { 0, 1 },   // 아래
        { -1, 1 },  // 왼쪽 아래
        { -1, 0 },  // 왼쪽
        { -1, -1 }, // 왼쪽 위
        { 0, -1 },  // 위
        { 1, -1 }   // 오른쪽 위
    };

    // 기본 삼(3) 패턴 (흑돌 = 1, 빈칸 = 0)
    private static readonly int[][] threePatterns = new int[][]
    {
        new int[] { 0, 1, 1, 1, 0 },       // _OOO_
        new int[] { 0, 1, 1, 0, 1, 0 },    // _OO_O_
        new int[] { 0, 1, 0, 1, 1, 0 }     // _O_OO_
    };

    // 기본 사(4) 패턴 (흑돌 = 1, 빈칸 = 0)
    private static readonly int[][] fourPatterns = new int[][]
    {
        new int[] { 0, 1, 1, 1, 1, 0 },    // _OOOO_
        new int[] { 0, 1, 1, 1, 0, 1, 0 }, // _OOO_O_
        new int[] { 0, 1, 1, 0, 1, 1, 0 }, // _OO_OO_
        new int[] { 0, 1, 0, 1, 1, 1, 0 }  // _O_OOO_
    };

    // 디버그 모드 활성화 (문제 해결 시 사용)
    private bool debugMode = false;

    /// <summary>
    /// 현재 보드 상태에서 흑돌(PlayerA)이 둘 수 없는 금수 위치 목록을 반환합니다.
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <returns>금수 위치 목록</returns>
    public List<Vector2Int> GetForbiddenMoves(Enums.PlayerType[,] board)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        List<Vector2Int> forbiddenMoves = new List<Vector2Int>();

        // 빈 칸마다 금수인지 확인
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 빈 칸만 검사
                if (board[x, y] != Enums.PlayerType.None)
                    continue;

                // 해당 위치에 흑돌을 임시로 놓았다고 가정
                Enums.PlayerType[,] simulatedBoard = (Enums.PlayerType[,])board.Clone();
                simulatedBoard[x, y] = Enums.PlayerType.PlayerA;

                // 장목(6목 이상) 확인
                if (IsOverline(simulatedBoard, x, y))
                {
                    forbiddenMoves.Add(new Vector2Int(x, y));
                    continue;
                }

                // 모든 방향별 패턴 정보 수집
                PatternInfo patternInfo = CollectAllPatterns(simulatedBoard, x, y);
                
                // 4-3 패턴 확인 (이 위치에 놓았을 때 열린 4가 1개, 열린 3이 1개 이상 생성되면 4-3 패턴)
                bool isFourThree = IsFourThreeAdvanced(patternInfo);
                if (isFourThree)
                {
                    // 4-3 패턴은 금수가 아님
                    continue;
                }

                // 쌍삼(3-3) 또는 사사(4-4) 확인
                bool isThreeThree = IsThreeThreeAdvanced(patternInfo);
                bool isFourFour = IsFourFourAdvanced(patternInfo);

                // 금수 여부 판정
                if (isThreeThree || isFourFour)
                {
                    forbiddenMoves.Add(new Vector2Int(x, y));
                }
            }
        }

        return forbiddenMoves;
    }

    /// <summary>
    /// 한 위치에서 모든 방향의 패턴 정보를 수집
    /// </summary>
    private PatternInfo CollectAllPatterns(Enums.PlayerType[,] board, int x, int y)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        PatternInfo patternInfo = new PatternInfo();
        
        // 각 방향별로 패턴 수집
        for (int dir = 0; dir < 4; dir++)
        {
            // 해당 방향에서 연속된 돌의 배열 얻기
            List<Stone> line = GetLineOfStones(board, x, y, dir, width, height, 8);
            
            // 열린 3 패턴 찾기
            List<OpenThreePattern> threePatterns = FindOpenThreePatterns(line, x, y, dir);
            patternInfo.OpenThreePatterns.AddRange(threePatterns);
            
            // 열린 4 패턴 찾기
            List<OpenFourPattern> fourPatterns = FindOpenFourPatterns(line, x, y, dir);
            patternInfo.OpenFourPatterns.AddRange(fourPatterns);
            
            // 방향별 돌 배열 저장 (디버깅용)
            if (debugMode)
            {
                patternInfo.LinesByDirection[dir] = line;
            }
        }
        
        return patternInfo;
    }

    /// <summary>
    /// 고도화된 4-3 패턴 확인 로직
    /// </summary>
    private bool IsFourThreeAdvanced(PatternInfo patternInfo)
    {
        // 열린 4와 열린 3 패턴 확인
        List<OpenFourPattern> fourPatterns = patternInfo.OpenFourPatterns;
        List<OpenThreePattern> threePatterns = patternInfo.OpenThreePatterns;
        
        if (fourPatterns.Count == 0 || threePatterns.Count == 0)
        {
            // 열린 4 또는 열린 3이 없으면 4-3 패턴이 아님
            return false;
        }
        
        // 방향별로 패턴 그룹화
        var fourPatternsByDir = fourPatterns.GroupBy(p => p.Direction).ToList();
        var threePatternsByDir = threePatterns.GroupBy(p => p.Direction).ToList();
        
        // 열린 4가 있는 각 방향에 대해
        foreach (var fourGroup in fourPatternsByDir)
        {
            int fourDir = fourGroup.Key;
            
            // 열린 3이 있는 각 방향에 대해
            foreach (var threeGroup in threePatternsByDir)
            {
                int threeDir = threeGroup.Key;
                
                // 서로 다른 방향이면 (열린 4와 열린 3이 다른 방향에 있으면)
                if (fourDir != threeDir)
                {
                    // 4와 3 패턴이 서로 중첩되지 않는지 확인
                    bool patternsOverlap = false;
                    
                    foreach (var fourPattern in fourGroup)
                    {
                        foreach (var threePattern in threeGroup)
                        {
                            // 중심 돌 외에 다른 공유 돌이 있는지 확인
                            if (PatternsShareStones(fourPattern, threePattern))
                            {
                                patternsOverlap = true;
                                break;
                            }
                        }
                        if (patternsOverlap) break;
                    }
                    
                    // 중첩되지 않는 4-3 패턴 발견됨
                    if (!patternsOverlap)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 두 패턴이 중심 돌 외에 다른 돌을 공유하는지 확인
    /// </summary>
    private bool PatternsShareStones(OpenFourPattern fourPattern, OpenThreePattern threePattern)
    {
        // 중심 돌 외에 공유하는 돌이 있는지 확인
        foreach (Vector2Int pos4 in fourPattern.StonePositions)
        {
            foreach (Vector2Int pos3 in threePattern.StonePositions)
            {
                // 두 패턴이 같은 좌표의 돌을 공유하고, 그것이 중심 돌이 아니면
                if (pos4.x == pos3.x && pos4.y == pos3.y && 
                    !(pos4.x == fourPattern.CenterX && pos4.y == fourPattern.CenterY))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 고도화된 삼삼 확인 로직
    /// </summary>
    private bool IsThreeThreeAdvanced(PatternInfo patternInfo)
    {
        List<OpenThreePattern> threePatterns = patternInfo.OpenThreePatterns;
        
        // 열린 3 패턴이 2개 미만이면 삼삼이 아님
        if (threePatterns.Count < 2)
            return false;
        
        // 방향별로 패턴 그룹화
        var patternsByDir = threePatterns.GroupBy(p => p.Direction).ToList();
        
        // 서로 다른 방향의 패턴 쌍을 검사
        for (int i = 0; i < patternsByDir.Count; i++)
        {
            for (int j = i + 1; j < patternsByDir.Count; j++)
            {
                // 첫 번째 방향의 모든 패턴
                var patternsDir1 = patternsByDir[i].ToList();
                // 두 번째 방향의 모든 패턴
                var patternsDir2 = patternsByDir[j].ToList();
                
                // 두 방향의 패턴들이 서로 중첩되지 않는지 확인
                bool foundNonOverlapping = false;
                
                foreach (var pattern1 in patternsDir1)
                {
                    if (foundNonOverlapping) break;
                    
                    foreach (var pattern2 in patternsDir2)
                    {
                        // 중심 돌 외에 다른 공유 돌이 없으면 비중첩 삼삼
                        if (!PatternsOverlap(pattern1, pattern2))
                        {
                            foundNonOverlapping = true;
                            break;
                        }
                    }
                }
                
                // 비중첩 삼삼 발견
                if (foundNonOverlapping)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 두 열린 3 패턴이 중첩되는지 확인
    /// </summary>
    private bool PatternsOverlap(OpenThreePattern pattern1, OpenThreePattern pattern2)
    {
        // 중심 돌 외에 공유하는 돌이 있는지 확인
        foreach (Vector2Int pos1 in pattern1.StonePositions)
        {
            foreach (Vector2Int pos2 in pattern2.StonePositions)
            {
                // 두 패턴이 같은 좌표의 돌을 공유하고, 그것이 중심 돌이 아니면 중첩
                if (pos1.x == pos2.x && pos1.y == pos2.y && 
                    !(pos1.x == pattern1.CenterX && pos1.y == pattern1.CenterY))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 고도화된 사사 확인 로직
    /// </summary>
    private bool IsFourFourAdvanced(PatternInfo patternInfo)
    {
        List<OpenFourPattern> fourPatterns = patternInfo.OpenFourPatterns;
        
        // 열린 4 패턴이 2개 미만이면 사사가 아님
        if (fourPatterns.Count < 2)
            return false;
        
        // 방향별로 패턴 그룹화
        var patternsByDir = fourPatterns.GroupBy(p => p.Direction).ToList();
        
        // 서로 다른 방향의 패턴이 2개 이상이면 사사
        if (patternsByDir.Count >= 2)
            return true;
        
        // 같은 방향에서도 중첩되지 않는 패턴이 있는지 확인
        foreach (var group in patternsByDir)
        {
            var patterns = group.ToList();
            
            for (int i = 0; i < patterns.Count - 1; i++)
            {
                for (int j = i + 1; j < patterns.Count; j++)
                {
                    // 중심 돌 외에 다른 공유 돌이 없으면 비중첩 사사
                    if (!FourPatternsOverlap(patterns[i], patterns[j]))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 두 열린 4 패턴이 중첩되는지 확인
    /// </summary>
    private bool FourPatternsOverlap(OpenFourPattern pattern1, OpenFourPattern pattern2)
    {
        // 중심 돌 외에 공유하는 돌이 있는지 확인
        foreach (Vector2Int pos1 in pattern1.StonePositions)
        {
            foreach (Vector2Int pos2 in pattern2.StonePositions)
            {
                // 두 패턴이 같은 좌표의 돌을 공유하고, 그것이 중심 돌이 아니면 중첩
                if (pos1.x == pos2.x && pos1.y == pos2.y && 
                    !(pos1.x == pattern1.CenterX && pos1.y == pattern1.CenterY))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 한 방향의 돌 배열 얻기 (지정된 범위만큼 확장)
    /// </summary>
    private List<Stone> GetLineOfStones(Enums.PlayerType[,] board, int x, int y, int dirIndex, int width, int height, int extensionRange = 5)
    {
        List<Stone> line = new List<Stone>();
        
        // 중앙에 놓인 현재 돌 추가
        line.Add(new Stone { Type = board[x, y], X = x, Y = y, RelativePosition = 0 });
        
        // 양방향으로 지정된 칸수만큼 확장
        for (int extendDir = 0; extendDir < 2; extendDir++)
        {
            int actualDirIndex = extendDir == 0 ? dirIndex : dirIndex + 4; // 반대 방향으로도 확장
            int dx = directions[actualDirIndex, 0];
            int dy = directions[actualDirIndex, 1];
            
            // 지정된 칸수만큼 확장
            for (int i = 1; i <= extensionRange; i++)
            {
                int nx = x + (dx * i);
                int ny = y + (dy * i);
                
                if (!IsInBounds(nx, ny))
                    break;
                
                Stone stone = new Stone
                {
                    Type = board[nx, ny],
                    X = nx,
                    Y = ny,
                    RelativePosition = extendDir == 0 ? i : -i
                };
                
                if (extendDir == 0)
                    line.Add(stone); // 정방향은 뒤에 추가
                else
                    line.Insert(0, stone); // 역방향은 앞에 추가
            }
        }
        
        return line;
    }

    /// <summary>
    /// 주어진 라인에서 열린 3 패턴 찾기 (백돌로 막혀있는 경우 고려)
    /// </summary>
    private List<OpenThreePattern> FindOpenThreePatterns(List<Stone> line, int centerX, int centerY, int dirIndex)
    {
        List<OpenThreePattern> result = new List<OpenThreePattern>();
        int lineLength = line.Count;
        
        // 각 열린 3 패턴에 대해 검사
        foreach (int[] pattern in threePatterns)
        {
            int patternLength = pattern.Length;
            
            // 라인을 슬라이딩 윈도우로 검사
            for (int i = 0; i <= lineLength - patternLength; i++)
            {
                bool patternMatches = true;
                List<Vector2Int> stonePositions = new List<Vector2Int>();
                List<int> stoneRelativePositions = new List<int>();
                
                // 패턴과 라인이 일치하는지 확인
                for (int j = 0; j < patternLength; j++)
                {
                    Stone stone = line[i + j];
                    
                    // 패턴의 1은 흑돌(PlayerA)과 일치해야 함
                    if (pattern[j] == 1)
                    {
                        if (stone.Type != Enums.PlayerType.PlayerA)
                        {
                            patternMatches = false;
                            break;
                        }
                        stonePositions.Add(new Vector2Int(stone.X, stone.Y));
                        stoneRelativePositions.Add(stone.RelativePosition);
                    }
                    // 패턴의 0은 빈 칸(None)과 일치해야 함
                    else if (pattern[j] == 0 && stone.Type != Enums.PlayerType.None)
                    {
                        patternMatches = false;
                        break;
                    }
                }
                
                // 패턴이 일치하고, 중심 돌이 패턴에 포함되는 경우에만 열린 3으로 인정
                if (patternMatches && stonePositions.Any(pos => pos.x == centerX && pos.y == centerY))
                {
                    // 패턴 양쪽의 확장성 검사 (백돌로 막혀있는지 확인)
                    bool isReallyOpen = IsPatternReallyOpen(line, i, i + patternLength - 1);
                    
                    // 진짜 열린 3인 경우에만 추가
                    if (isReallyOpen)
                    {
                        result.Add(new OpenThreePattern
                        {
                            Direction = dirIndex,
                            StonePositions = stonePositions,
                            RelativePositions = stoneRelativePositions,
                            CenterX = centerX,
                            CenterY = centerY
                        });
                    }
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// 주어진 라인에서 열린 4 패턴 찾기 (백돌로 막혀있는 경우 고려)
    /// </summary>
    private List<OpenFourPattern> FindOpenFourPatterns(List<Stone> line, int centerX, int centerY, int dirIndex)
    {
        List<OpenFourPattern> result = new List<OpenFourPattern>();
        int lineLength = line.Count;
        
        // 각 열린 4 패턴에 대해 검사
        foreach (int[] pattern in fourPatterns)
        {
            int patternLength = pattern.Length;
            
            // 라인을 슬라이딩 윈도우로 검사
            for (int i = 0; i <= lineLength - patternLength; i++)
            {
                bool patternMatches = true;
                List<Vector2Int> stonePositions = new List<Vector2Int>();
                
                // 패턴과 라인이 일치하는지 확인
                for (int j = 0; j < patternLength; j++)
                {
                    Stone stone = line[i + j];
                    
                    // 패턴의 1은 흑돌(PlayerA)과 일치해야 함
                    if (pattern[j] == 1)
                    {
                        if (stone.Type != Enums.PlayerType.PlayerA)
                        {
                            patternMatches = false;
                            break;
                        }
                        stonePositions.Add(new Vector2Int(stone.X, stone.Y));
                    }
                    // 패턴의 0은 빈 칸(None)과 일치해야 함
                    else if (pattern[j] == 0 && stone.Type != Enums.PlayerType.None)
                    {
                        patternMatches = false;
                        break;
                    }
                }
                
                // 패턴이 일치하고, 중심 돌이 패턴에 포함되는 경우에만 열린 4로 인정
                if (patternMatches && stonePositions.Any(pos => pos.x == centerX && pos.y == centerY))
                {
                    // 패턴 양쪽의 확장성 검사 (백돌로 막혀있는지 확인)
                    bool isReallyOpen = IsPatternReallyOpen(line, i, i + patternLength - 1);
                    
                    // 진짜 열린 4인 경우에만 추가
                    if (isReallyOpen)
                    {
                        result.Add(new OpenFourPattern
                        {
                            Direction = dirIndex,
                            StonePositions = stonePositions,
                            CenterX = centerX,
                            CenterY = centerY
                        });
                    }
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// 패턴이 정말로 열려있는지 확인 (백돌로 막혀있지 않은지)
    /// </summary>
    private bool IsPatternReallyOpen(List<Stone> line, int startIdx, int endIdx)
    {
        int lineLength = line.Count;
        bool leftSideOpen = false;
        bool rightSideOpen = false;
        
        // 좌측 확장성 검사
        int leftCheckIdx = startIdx - 1;
        int leftExtendableSpaces = 0;
        
        while (leftCheckIdx >= 0)
        {
            if (line[leftCheckIdx].Type == Enums.PlayerType.None)
            {
                leftExtendableSpaces++;
                leftCheckIdx--;
            }
            else if (line[leftCheckIdx].Type == Enums.PlayerType.PlayerB)
            {
                // 백돌로 막혀있음
                break;
            }
            else
            {
                // 흑돌이 있음 (이미 다른 패턴의 일부일 수 있음)
                break;
            }
        }
        
        // 우측 확장성 검사
        int rightCheckIdx = endIdx + 1;
        int rightExtendableSpaces = 0;
        
        while (rightCheckIdx < lineLength)
        {
            if (line[rightCheckIdx].Type == Enums.PlayerType.None)
            {
                rightExtendableSpaces++;
                rightCheckIdx++;
            }
            else if (line[rightCheckIdx].Type == Enums.PlayerType.PlayerB)
            {
                // 백돌로 막혀있음
                break;
            }
            else
            {
                // 흑돌이 있음 (이미 다른 패턴의 일부일 수 있음)
                break;
            }
        }
        
        // 양쪽이 모두 백돌로 막혀있다면 진짜 열린 3이 아님
        if (leftCheckIdx >= 0 && rightCheckIdx < lineLength &&
            line[leftCheckIdx].Type == Enums.PlayerType.PlayerB &&
            line[rightCheckIdx].Type == Enums.PlayerType.PlayerB)
        {
            return false;
        }
        
        // 적어도 한쪽에 빈 공간이 있으면 열린 패턴으로 간주
        leftSideOpen = leftCheckIdx < 0 || line[leftCheckIdx].Type != Enums.PlayerType.PlayerB;
        rightSideOpen = rightCheckIdx >= lineLength || line[rightCheckIdx].Type != Enums.PlayerType.PlayerB;
        
        // 한쪽이라도 열려있어야 열린 패턴으로 인정
        return leftSideOpen || rightSideOpen;
    }

    /// <summary>
    /// 장목(6목 이상) 금수 검사
    /// </summary>
    private bool IsOverline(Enums.PlayerType[,] board, int x, int y)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        // 4방향(가로, 세로, 대각선1, 대각선2)에 대해 6목 이상 검사
        for (int dir = 0; dir < 4; dir++)
        {
            int count = 1; // 현재 위치 포함
            
            // 정방향 탐색
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];
            int nx = x + dx;
            int ny = y + dy;
            
            while (IsInBounds(nx, ny) && board[nx, ny] == Enums.PlayerType.PlayerA)
            {
                count++;
                nx += dx;
                ny += dy;
            }
            
            // 역방향 탐색
            dx = directions[dir + 4, 0];
            dy = directions[dir + 4, 1];
            nx = x + dx;
            ny = y + dy;
            
            while (IsInBounds(nx, ny) && board[nx, ny] == Enums.PlayerType.PlayerA)
            {
                count++;
                nx += dx;
                ny += dy;
            }
            
            // 6목 이상이면 장목(금수)
            if (count >= 6)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 돌 정보를 저장하는 구조체
    /// </summary>
    private struct Stone
    {
        public Enums.PlayerType Type;
        public int X;
        public int Y;
        public int RelativePosition; // 중심 돌로부터의 상대적 위치
    }

    /// <summary>
    /// 열린 3 패턴 정보를 저장하는 클래스
    /// </summary>
    private class OpenThreePattern
    {
        public int Direction;
        public List<Vector2Int> StonePositions;
        public List<int> RelativePositions; // 중심 돌로부터의 상대적 위치
        public int CenterX;
        public int CenterY;
    }

    /// <summary>
    /// 열린 4 패턴 정보를 저장하는 클래스
    /// </summary>
    private class OpenFourPattern
    {
        public int Direction;
        public List<Vector2Int> StonePositions;
        public int CenterX;
        public int CenterY;
    }

    /// <summary>
    /// 한 위치의 모든 패턴 정보를 저장하는 클래스
    /// </summary>
    private class PatternInfo
    {
        public List<OpenThreePattern> OpenThreePatterns = new List<OpenThreePattern>();
        public List<OpenFourPattern> OpenFourPatterns = new List<OpenFourPattern>();
        public Dictionary<int, List<Stone>> LinesByDirection = new Dictionary<int, List<Stone>>();
    }
}

