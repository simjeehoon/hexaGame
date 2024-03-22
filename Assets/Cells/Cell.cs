using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class Cell : MonoBehaviour
{
    public int cellIndex = 0;

    // 스프라이트 렌더러
    private SpriteRenderer spriteRenderer;
    
    // State에 따라서 Update에서 수행할 행동이 변화한다.
    abstract class CellState
    {
        // cell은 this가 된다.
        protected Cell cell;
        public CellState(Cell cell)
        {
            this.cell = cell;
        }
        abstract public void OnUpdate();
    }

    // 기본 상태이다.
    class StateNormal : CellState
    {
        public StateNormal(Cell cell) 
        : base(cell) 
        {
            cell.spriteRenderer.enabled = true;
        }

        public override void OnUpdate()
        { 
        }
    }

    // 깜빡이는 상태이다.
    class StateBlinking : CellState
    {   
        // visibility가 변환될 기준 시간
        float toggleTime;

        // visibility 변환을 위해 시간을 저장할 변수
        float checkPointTime;

        // blinking 효과를 위한 bool 변수
        bool visible=true;

        public StateBlinking(Cell cell, float toggleTime=0.2f) 
        : base(cell)
        {
            // 기준 시간 설정
            checkPointTime = Time.time;
            this.toggleTime = toggleTime;
            Debug.Log($"blinking: {this.GetHashCode()}");
        }

        public override void OnUpdate()
        {
            // 효과를 전환하기 위한 시간이 되었다면?
            if(Time.time - checkPointTime > toggleTime)
            {
                if(visible)
                {
                    // 렌더러를 끈다.
                    cell.spriteRenderer.enabled = false;
                }
                else
                {
                    // 렌더러를 켠다.
                    cell.spriteRenderer.enabled = true;
                }
                // 효과를 위한 계산 시간을 재설정
                checkPointTime = Time.time;
                // visible 전환
                visible = !visible;
            }
        }
    }

    // 기본 상태이다.
    class StateInvisible : CellState
    {
        public StateInvisible(Cell cell) 
        : base(cell) 
        {
            cell.spriteRenderer.enabled = false;
        }

        public override void OnUpdate()
        { 
        }
    }


    CellState cellState;

    public int value=0;

    public void SetNormal()
    {
        cellState = new StateNormal(this);
    }

    public void SetInvisible()
    {
        cellState = new StateInvisible(this);
    }

    public void StartBlink()
    {
        cellState = new StateBlinking(this);
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetNormal();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        cellState.OnUpdate();
    }
}
