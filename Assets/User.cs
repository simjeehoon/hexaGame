using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexaBlock
{
    private int[] cells;
    int blockLength;
    HexaMap hexaMap;
    private int w;
    private int h;
    
    // 블록의 순서를 바꾼다.
    private void ReorderCells(bool reverse=false)
    {
        if(cells != null)
        {
            int i, temp;

            if(reverse) // 역방향
            {
                temp = cells[blockLength - 1]; // 마지막 셀
                for(i = blockLength - 1 ; i > 0 ; --i)
                {
                    cells[i] = cells[i - 1]; // --> 이동
                }
                cells[i] = temp;  //첫 셀은 마지막셀로
            }
            else
            {
                temp = cells[0];  // 첫 셀
                for(i = 0 ; i < blockLength - 1 ; ++i)
                {
                    cells[i] = cells[i + 1];  // <-- 이동
                }
                cells[i] = temp;  // 마지막 셀은 첫 셀로
            }
        }
    }
    
    // 블럭이 맵에 놓일 수 있나?
    private bool CanBeLocatedOnMap(int w, int h)
    {
        // 세로 셀들을 모두 검사
        for(int i = 0 ; i < blockLength ; ++i)
        {
            // 공간이 없다면 false 리턴
            if(!hexaMap.Empty(w, h+i))
                return false;
        }
        return true;
    }

    // 블럭을 맵에 놓는다.
    private void PutBlockOnMap(int w, int h)
    {
        // 블럭 좌표를 새로 설정
        this.w = w;
        this.h = h;
        // 세로 셀들을 모두 놓는다.
        for(int i = 0 ; i < blockLength ; ++i)
        {
            hexaMap.PutCell(this.w, this.h + i, cells[i]);
        }
    }

    // 블럭을 맵에서 제거한다.
    private void RemoveBlockFromMap()
    {
        // 세로 셀들을 모두 제거한다.
        for(int i = 0 ; i < blockLength ; ++i)
        {
            if(!hexaMap.RemoveCell(this.w, this.h+i))
            {
                Debug.Log($"Cell({w}, {h+i}) was empty.");
            }
        }
    }

    public HexaBlock(HexaMap hexaMap, int[] cells, int blockLength=3)
    {
        this.hexaMap = hexaMap;
        this.cells = cells;
        this.blockLength = blockLength;
    }

    // 맵상의 블럭 위치를 반환한다.
    public (int, int) GetPosition()
    {
        return (w, h);
    }

    // 가장 위에 놓는다.
    public bool PutOnTop()
    {
        // 정중앙 최상단
        w = hexaMap.width / 2;
        // 처음엔 맵 안에 셀들이 모두 놓인다.
        h = hexaMap.checkHeight;

        for(;h>=0;--h) // 가능한 모든 경우를 검사한다.
        {
            // 놓을 수 있다면?
            if(CanBeLocatedOnMap(w, h)) 
            {
                // 놓고 true 리턴
                PutBlockOnMap(w, h);
                return true;
            }
        }
        // 맵 밖으로 벗어난 경우다.
        return false;
    }

    // 왼쪽 이동
    public bool MoveLeft()
    {   
        // 놓일 수 있다면
        if(CanBeLocatedOnMap(w - 1, h))
        {
            RemoveBlockFromMap();
            PutBlockOnMap(w - 1, h);
            return true;
        }
        else
        {
            // 놓일 수 없으면 false
            return false;
        }
    }

    // 오른쪽 이동
    public bool MoveRight()
    {
        // 놓일 수 있다면
        if(CanBeLocatedOnMap(w + 1, h))
        {
            RemoveBlockFromMap();
            PutBlockOnMap(w + 1, h);
            return true;
        }
        else
        {
            // 놓일 수 없으면 false
            return false;
        }
    }

    // 아래로 이동
    public bool MoveDown()
    {
        RemoveBlockFromMap();
        // 놓일 수 있다면 놓는다.
        if(CanBeLocatedOnMap(w, h + 1))
        {
            PutBlockOnMap(w, h + 1);
            return true;
        }
        else
        {
            PutBlockOnMap(w, h);
            return false;
        }
    }

    // 순서를 바꿔서 다시 맵에 놓는다.
    public void ChangeOrder(bool reverse=false)
    {
        RemoveBlockFromMap();
        ReorderCells(reverse);
        PutBlockOnMap(w, h);
    }
}

public class User : MonoBehaviour
{
    public float fallTime = 1.0f;
    public float removingTime = 2.0f;
    public HexaMap hexaMap;
    public GameObject darkFilm;
    public int blockLength=3;
    public GameObject scoreObject;
    private Score score;

    private HexaBlock curHexaBlock;
    

    // 블록 셀 인덱스 배열을 랜덤하게 생성한다.
    private int[] GetRandomCells()
    {
        // 셀 초기화
        int[] cells = new int[blockLength];
        for(int i = 0 ; i < blockLength ; i++)
        {
            cells[i] = Random.Range(0, hexaMap.cellShapes.Length);
        }
        return cells;
    }

    SortedSet<(int,int)> contigiousPoints;

    // 연속된 셀의 포인트를 갱신한다.
    bool RenewContigiousPoints()
    {
        // 연속된 셀을 가져온다.
        contigiousPoints = hexaMap.CheckConsequtive();
        
        // 연속된 셀이 없으면 함수를 반환한다.
        if(contigiousPoints.Count == 0)
            return false;
        return true;
    }

    // 현재 존재하는 연속 cell들을 반짝인다.
    void BlinkContiguousPoints()
    {
        Cell cellComponent;

        if(contigiousPoints != null)
        {
            // cell들을 반짝인다.
            foreach((int,int) pointOfCell in contigiousPoints)
            {
                cellComponent = hexaMap.GetCell(pointOfCell.Item1, pointOfCell.Item2).GetComponent<Cell>();
                cellComponent.StartBlink();
            }
        }
    }

    // 공중에 떠있는 블럭들을 떨어뜨린다.
    void RemoveFloatingBlocks(int step)
    {
        // 해당 cells를 끌어내린다.
        foreach((int,int) pointOfCell in contigiousPoints)
        {
            Debug.Log($"remove : w{pointOfCell.Item1}, ");
            hexaMap.RemoveCell(pointOfCell.Item1, pointOfCell.Item2, true);
        }
        score.NormalIncrease(contigiousPoints.Count, step);
        // 연속 블럭 셋을 제거한다.
        contigiousPoints = null;
    }

    // 새 블록을 시작한다.
    void StartNewBlock()
    {
        // 블럭을 소환한다.
        curHexaBlock = new HexaBlock(hexaMap, GetRandomCells(), blockLength);
        // 블럭을 맵 위에 놓지 못하면?
        if(!curHexaBlock.PutOnTop())
        {
            // 게임 오버 상태 진입
            curState = new GameOverState(this);
        }
    }

    void Awake()
    {
        
    }

    void Start()
    {
        curState = new WaitKeyInputState(this);
        score = scoreObject.GetComponent<Score>();
    }

    // 상태 추상 클래스
    abstract class State
    {
        protected User user;
        public State(User user)
        {
            this.user = user;
        }
        abstract public void OnUpdate();
    }

    State curState;

    // 키 입력을 받는 상태일때 업데이트 전략
    class WaitKeyInputState : State
    {
        private float previousTime;
        // 수평 이동 및 순서를 바꿀때 약간의 여유 시간을 더한다.
        // 이동 카운트를 기록하는 변수이다.
        int stepMoveCount;
        public WaitKeyInputState (User user):base(user)
        {
            user.StartNewBlock();
            previousTime = Time.time;
            stepMoveCount = 0;
        }

        // 블럭을 아래로 다운시킬때 작동하는 코드
        private void OnBlockDown()
        {
            // 아래 이동을 시도한다.
            // 아래 이동이 불가능한 경우 if문 진입
            if(!user.curHexaBlock.MoveDown())
            {   
                // 연속 블럭들이 있다면?
                if(user.RenewContigiousPoints())
                {
                    // 셀을 반짝인다.
                    user.curState = new RemovingEmptyCellState(user, 1);
                }
                else
                {
                    // 제거할 셀이 없으면 새롭게 다시 시작한다.
                    user.StartNewBlock();
                    stepMoveCount = 0;
                }
            }
            // 블럭 내리는 시간 재갱신
            previousTime = Time.time;
            // 이동 카운트를 초기화
            stepMoveCount = 0;
        }

        // 이동으로 인한 보너스 시간을 반환한다
        float GetMoveTerm()
        {
            float bonusTerm = .0f;
            for(int i = 1 ; i <= stepMoveCount ; i++)
            {
                if(i == 1)
                {
                    bonusTerm += user.fallTime / 10;
                }
                else if(i < 4)
                {
                    bonusTerm += user.fallTime / 30 / i;
                }
                else
                    break;
            }
            return bonusTerm;
        }
        
        public override void OnUpdate()
        {
             // 왼쪽 이동
            if(Input.GetKeyDown(KeyCode.LeftArrow)) 
            {
                user.curHexaBlock.MoveLeft();
                // 이동 횟수 증가
                stepMoveCount++;
            }
            // 오른쪽 이동
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                user.curHexaBlock.MoveRight();
                // 이동 횟수 증가
                stepMoveCount++;
            }
            // 아래로 이동
            else if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                OnBlockDown();
            }
            // 순서 변경
            else if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                user.curHexaBlock.ChangeOrder();
            }
            // 시간이 지나면 자동으로 하강한다.
            if(Time.time - previousTime > user.fallTime + GetMoveTerm())
            {
                OnBlockDown();
            }
        }
    }

    // 블럭을 지울때 전략
    class RemovingEmptyCellState : State
    {
        private float previousTime;
        int step;
        public RemovingEmptyCellState(User user, int step) : base(user) 
        {
            this.step = step;
            previousTime = Time.time;
            user.BlinkContiguousPoints();
        }

        public override void OnUpdate()
        {
            // 반짝이는 시간이 초과했을 때?
            if(Time.time - previousTime > user.removingTime)
            {
                // 해당 블럭을 실제로 지우고 빈자리를 채운다.
                user.RemoveFloatingBlocks(step);

                // 이후에도 여전히 연속 블럭이 존재한다면?
                if(user.RenewContigiousPoints())
                {
                    // 다시 전략을 재생성한다.
                    user.curState = new RemovingEmptyCellState(user, step+1);
                }
                else
                {
                    // 키 입력을 받는 상태로 진입한다.
                    user.curState = new WaitKeyInputState(user);
                }
            }
        }
    }

    // 게임 오버 상태
    class GameOverState : State
    {
        public GameOverState(User user) : base(user)
        {
            Debug.Log("게임 오버");
            Instantiate(user.darkFilm);
            user.score.SaveHighScore();
        }

        public override void OnUpdate()
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(curState == null)
        {
            Debug.Log("ERR");
        }
        else
        {
            curState.OnUpdate();
        }
    }
}
