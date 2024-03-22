using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEditor.PackageManager;
using UnityEngine;

public class HexaMap : MonoBehaviour
{
    // 맵 너비, 높이
    public int width=7;
    public int height=15;

    // 검사를 시작할 높이
    public int checkHeight=2;
    
    // 제거할 연속 블럭의 길이
    public int removeLength = 3;

    // 셀 인스턴스 생성시 사용할 포지션
    private float cellStartPositionX;
    private float cellStartPositionY;
    
    // 셀 모양
    public GameObject[] cellShapes;

    // 셀 정보 맵
    private GameObject[,] cellInstanceMap;

    // Cell을 획득합니다.
    public GameObject GetCell(int w, int h)
    {
        if(!(0 <= w && w < width && 0 <= h && h < height))
        {
            Debug.Log($"GetCell({w},{h}) is wrong");
            return null;
        }
        return cellInstanceMap[h, w];
    }

    // Cell에 해당하는 인덱스를 리턴합니다.
    public int GetCellIndex(GameObject gameObject)
    {
        // null 이면 -1
        if(gameObject == null)
        {
            return -1;
        }
        
        // 오브젝트 비교
        for(int i = 0 ; i < cellShapes.Length ; ++i)
        {
            Cell cellShape = cellShapes[i].GetComponent<Cell>();
            Cell shapeOfCell = gameObject.GetComponent<Cell>();
            if(cellShape.cellIndex == shapeOfCell.cellIndex)
            {
                return i;
            }
        }

        // 없는 오브젝트일 경우 -1
        return -1;
    }

    void Awake()
    {
        // 맵 초기화
        cellInstanceMap = new GameObject[height, width];

        // 셀 인스턴스 생성시 사용할 포지션
        cellStartPositionX = transform.position.x - width/2;
        cellStartPositionY = transform.position.y + (height-checkHeight)/2;
    }

    // 맵이 비어있는가?
    public bool Empty()
    {
        for(int h = 0 ; h < height ; h++)
        {
            for(int w = 0 ; w < width ; w++)
            {
                if(cellInstanceMap[h, w] != null)
                    return false;
            }
        }
        return true;
    }

    // 맵의 해당 셀 위치가 빈공간인가?
    public bool Empty(int w, int h)
    {
        if(w < 0 || w >= width || h < 0 || h >= height)
        {
            return false;
        }
        if(cellInstanceMap[h, w] != null)
        {
            return false;
        }
        return true;       
    }

    // Cell을 놓습니다.
    public void PutCell(int w, int h, int cellIndex)
    {
        // 이미 해당 위치에 셀이 존재하면 삭제한다.
        if(cellInstanceMap[h, w] != null)
        {
            Debug.Log($"PutCell({w},{h}): map({w},{h}) is not empty!");
            Destroy(cellInstanceMap[h, w]);
        }

        // transform 결정
        Vector3 position = new Vector3(cellStartPositionX + w, cellStartPositionY - h + checkHeight, transform.position.z - 1);
        Quaternion rotation = Quaternion.Euler(0, 0, 0);

        // 해당 위치에 셀을 인스턴스화
        GameObject newCell = Instantiate(cellShapes[cellIndex], position, rotation);

        // 이름 및 Parent 설정
        newCell.name = $"Cell(x={w},y={h})({cellShapes[cellIndex].name})";
        newCell.transform.SetParent(transform);

        // 셀 인스턴스 맵에 저장
        cellInstanceMap[h, w] = newCell;
    }

    // Cell을 제거합니다.
    public bool RemoveCell(int w, int h, bool gravity=false)
    {
        bool removed = false;
        // 이미 해당 위치에 셀이 존재하면 삭제한다.
        if (cellInstanceMap[h, w] != null)
        {
            Destroy(cellInstanceMap[h, w]);
            removed = true;
        }

        // 중력 옵션일 경우
        if(gravity)
        {
            while(h > 0)
            {
                // 한 층 위의 셀이 존재할 경우
                if (cellInstanceMap[h-1, w] != null)
                {
                    // position을 아래로 내린다.
                    cellInstanceMap[h-1, w].transform.position += new Vector3(0, -1, 0);
                }
                // 맵에서 아래 위치에 위의 셀 인스턴스를 덮어쓴다.
                cellInstanceMap[h, w] = cellInstanceMap[h-1, w];
                --h;
            }
        }
        
        // null로 안전하게 덮어씌우기
        cellInstanceMap[h, w] = null;
        return removed;
    }

    
    // 연속된 셀들의 포인트를 가져옵니다.
    public SortedSet<(int,int)> CheckConsequtive()
    {
        SortedSet<(int,int)> consequtivePoints = new();
        
        void AddConsequitivePoints
        (
            SortedSet<(int,int)> consequtivePoints, 
            int curW, int curH, int consequtiveCount, 
            int incW, int incH
        )
        {
            if(consequtiveCount >= removeLength)
            {
                int w = curW; 
                int h = curH; 
                int c = 0;
                while(c < consequtiveCount)
                {
                    consequtivePoints.Add((w, h));
                    w -= incW;
                    h -= incH;
                    ++c;
                }
            }
        }

        // 가로, 세로 인덱스
        int h, w;

        // 연속 카운트
        int consequtiveCount;

        // 셀 비교를 위한 변수
        int prevIndex, curIndex;

        // 가로 검사
        for(h = checkHeight; h < height; ++h)
        {
            // 가로선의 첫번째 셀의 정보 저장
            prevIndex = GetCellIndex(cellInstanceMap[h, 0]);
            
            // 연속 셀 수 1로 초기화
            consequtiveCount = 1;

            // 1개의 가로선 비교 시작
            for(w = 1; w < width; ++w)
            {
                curIndex = GetCellIndex(cellInstanceMap[h, w]); // 현재 셀 인덱스 저장
                if(prevIndex != -1) // 이전 셀이 존재할 경우에만 검사
                {
                    if(curIndex == prevIndex) // 현재 셀이 존재할 경우, 이전셀과 같은 블럭이라면?
                    {
                        // 연속 카운트 증가
                        ++consequtiveCount; 
                    }
                    else // 이전 블럭과 다르다면?
                    {
                        // 연속된 가로 셀들을 저장
                        AddConsequitivePoints(consequtivePoints, w-1, h, consequtiveCount, 1, 0); 
                        // 연속 카운트 초기화
                        consequtiveCount = 1; 
                    }
                }
                // 현재 블록을 이전 블록으로 저장
                prevIndex = curIndex; 
            }
            // 한 줄이 끝났으므로
            // 연속된 가로 셀들을 저장
            AddConsequitivePoints(consequtivePoints, w-1, h, consequtiveCount, 1, 0);
        }
        
        // 세로 검사
        for(w = 0; w < width; ++w)
        {
            // 세로선의 첫번째 셀의 정보 저장
            prevIndex = GetCellIndex(cellInstanceMap[checkHeight, w]);

            // 연속 셀 수 1로 초기화
            consequtiveCount = 1;

            // 1개의 세로선 비교 시작
            for(h = checkHeight + 1; h < height; ++h)
            {
                curIndex = GetCellIndex(cellInstanceMap[h, w]); // 현재 셀 인덱스 저장
                if(prevIndex != -1) // 이전 셀이 존재할 경우에만 검사
                {
                    if(curIndex == prevIndex) // 현재 셀이 존재할 경우, 이전셀과 같은 블럭이라면?
                    {
                        // 연속 카운트 증가
                        ++consequtiveCount; 
                    }
                    else // 이전 블럭과 다르다면?
                    {
                        // 연속된 세로 셀들을 저장
                        AddConsequitivePoints(consequtivePoints, w, h-1, consequtiveCount, 0, 1); 
                        // 연속 카운트 초기화
                        consequtiveCount = 1;
                    }
                }
                // 현재 블록을 이전 블록으로 저장
                prevIndex = curIndex; 
            }
            // 한 줄이 끝났으므로
            // 연속된 세로 셀들을 저장
            AddConsequitivePoints(consequtivePoints, w, h-1, consequtiveCount, 0, 1); 
        }
        
        int startW; // 시작 x 좌표
        int startH; // 시작 y 좌표
        
        // ↘(좌상우하) 대각선 검사
        startW = 0;  // 왼쪽
        startH = height - 1;  // 가장 아래
        while(true)
        {
            // 대각선의 첫번째 셀의 정보 저장
            prevIndex = GetCellIndex(cellInstanceMap[startH, startW]);

            // 연속 셀 수 1로 초기화
            consequtiveCount = 1;

            // 인덱스 초깃값을 2번째 셀으로 설정
            w = startW + 1;
            h = startH + 1;
            
            // 1개의 대각선 검사 시작
            while(0 <= w && w < width && checkHeight <= h && h < height) // 허용된 범위라면?
            {
                curIndex = GetCellIndex(cellInstanceMap[h, w]); // 현재 셀 인덱스 저장
                if(prevIndex != -1) // 이전 셀이 존재할 경우에만 검사
                {
                    if(curIndex == prevIndex) // 현재 셀이 존재할 경우, 이전셀과 같은 블럭이라면?
                    {
                        // 연속 카운트 증가
                        ++consequtiveCount; 
                    }
                    else // 이전 블럭과 다르다면?
                    {
                        // 연속된 대각선 셀들을 저장
                        AddConsequitivePoints(consequtivePoints, w-1, h-1, consequtiveCount, 1, 1); 
                        // 연속 카운트 초기화
                        consequtiveCount = 1; 
                    }
                }
                // 현재 블록을 이전 블록으로 저장
                prevIndex = curIndex; 

                // 다음 셀로 좌표 설정
                ++w;
                ++h;
            }
            // 한 대각선 줄이 끝났으므로
            // 연속된 대각선 셀들을 저장
            AddConsequitivePoints(consequtivePoints, w-1, h-1, consequtiveCount, 1, 1); 

            // 반복문 검사
            if(startH > 0) // 시작지점을 위로 이동시킨다.
            {
                --startH;
            }
            else if(startW < width - 1) // 시작지점을 오른쪽으로 이동시킨다.
            {
                ++startW;
            }
            else // 모든 곳을 검사하였다.
            {
                break;
            }
        }

        // ↙(우상좌하) 대각선 검사
        startW = 0;  // 맨 왼쪽
        startH = checkHeight;  // 맨 위
        while(true)
        {
            // 대각선의 첫번째 셀의 정보 저장
            prevIndex = GetCellIndex(cellInstanceMap[startH, startW]);
            // 연속 셀 수 1로 초기화
            consequtiveCount = 1;

            // 인덱스 초깃값을 2번째 셀으로 설정
            w = startW - 1;
            h = startH + 1;
            
            // 1개의 대각선 검사 시작
            while(0 <= w && w < width && checkHeight <= h && h < height) // 허용된 범위라면?
            {
                curIndex = GetCellIndex(cellInstanceMap[h, w]); // 현재 셀 인덱스 저장
                if(prevIndex != -1) // 이전 셀이 존재할 경우에만 검사
                {
                    if(curIndex == prevIndex) // 현재 셀이 존재할 경우, 이전셀과 같은 블럭이라면?
                    {
                        // 연속 카운트 증가
                        ++consequtiveCount; 
                    }
                    else // 이전 블럭과 다르다면?
                    {
                        // 연속된 대각선 셀들을 저장
                        AddConsequitivePoints(consequtivePoints, w+1, h-1, consequtiveCount, -1, 1); 
                        // 연속 카운트 초기화
                        consequtiveCount = 1; 
                    }
                }
                // 현재 블록을 이전 블록으로 저장
                prevIndex = curIndex; 

                // 다음 셀로 좌표 설정
                --w;
                ++h;
            }
            // 한 대각선 줄이 끝났으므로
            // 연속된 대각선 셀들을 저장
            AddConsequitivePoints(consequtivePoints, w+1, h-1, consequtiveCount, -1, 1); 

            // 반복문 검사
            if(startW < width - 1) // 시작지점을 오른쪽으로 이동시킨다.
            {
                ++startW;
            }
            else if(startH < height - 1) // 시작지점을 아래로 이동시킨다.
            {
                ++startH;
            }
            else // 모든 곳을 검사하였다.
            {
                break;
            }
        }

        return consequtivePoints;
    }
}
