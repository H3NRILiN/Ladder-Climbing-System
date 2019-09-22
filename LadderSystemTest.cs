using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityStandardAssets.Characters.FirstPerson;

public class LadderSystemTest : MonoBehaviour
{
    [Tag]
    public string m_PlayerTag;

    public Vector3 m_LowerPoint;
    public Vector3 m_UpperPoint;

    public float m_MoveSpeed;

    FirstPersonController m_FPSController;

    bool m_IsIn;
    float m_MoveAmount;

    Vector3 m_EnterPosition;


    Vector3 m_OffsetedLowerPoint;
    Vector3 m_OffsetedUpperPoint;

    float m_OffsetLength;

    float m_LowerPortion;
    float m_UpperPortion;

    Vector3 m_CurrentPosition;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsIn)
        {
            if (m_FPSController.m_IsJumpingButtonDown)
            {
                m_FPSController.m_JumpDesire = -m_FPSController.transform.forward;
                m_FPSController.m_CanPhisicsMove = true;
                m_MoveAmount = 0;
                m_IsIn = false;
            }
            if (m_FPSController.m_CurrentInputY == 0)
                return;



            m_MoveAmount += (m_FPSController.m_CurrentInputY > 0 ? 1 : -1) * Time.deltaTime *
            //依照比例來分配增量,比例少的增量多,反之則增量少
            1 / (m_MoveAmount == 0 ? 1 :
            m_MoveAmount > 0 && m_MoveAmount < 1 ? m_UpperPortion :
            m_MoveAmount < 0 && m_MoveAmount > -1 ? m_LowerPortion : 1) * 1 / m_OffsetLength * m_MoveSpeed;


            m_CurrentPosition = m_FPSController.transform.position;



            if (m_MoveAmount > 0 && m_MoveAmount < 1)
            {
                m_FPSController.transform.position = Vector3.Lerp(m_EnterPosition, m_OffsetedUpperPoint, m_MoveAmount);
            }
            else if (m_MoveAmount < 0 && m_MoveAmount > -1)
            {

                m_FPSController.transform.position = Vector3.Lerp(m_EnterPosition, m_OffsetedLowerPoint, Mathf.Abs(m_MoveAmount));
            }


            if (m_MoveAmount <= -1)
            {
                m_FPSController.m_CanPhisicsMove = true;
                m_MoveAmount = 0;
                m_IsIn = false;
            }
            if (m_MoveAmount >= 1)
            {
                m_FPSController.m_CanPhisicsMove = true;
                m_MoveAmount = 0;
                m_IsIn = false;
            }
        }
    }

    Vector3 LowPointToUpperPointVector()
    {
        return UpperPoint() - LowerPoint();
    }

    Vector3 LowerPoint()
    {
        return m_LowerPoint + transform.position;
    }
    Vector3 UpperPoint()
    {
        return m_UpperPoint + transform.position;
    }

    void GetProjectingPoint()
    {
        //先求進入點與最低點的向量,結果為"最低點到進入點向量"
        Vector3 lowPointToEnterPointV = m_EnterPosition - LowerPoint();
        //利用Vector3的向量投影功能將"最低點到進入點向量"投影到"最低點到最高點向量"
        Vector3 EnterPointProjectV = Vector3.Project(lowPointToEnterPointV, LowPointToUpperPointVector());

        //為了獲得在"最低點到最高點向量"上的投影點,只需要再加回最低點位置即可得到
        //EnterPointProgectPosition
        Vector3 EnterProgectP = EnterPointProjectV + LowerPoint();

        //接下來求"進入點到投影點向量",用進入點減投影點
        Vector3 EnterProjectPToEnterPointV = m_EnterPosition - EnterProgectP;

        //要平移整個向量,兩端點勢必要位移同樣的量,所以用"進入點到投影點向量"當作移動量
        //要獲得移動後的點,將"進入點到投影點向量"加上原本的點位置即可
        m_OffsetedLowerPoint = EnterProjectPToEnterPointV + LowerPoint();
        m_OffsetedUpperPoint = EnterProjectPToEnterPointV + UpperPoint();

        //計算平移後兩端點與進入點向量的比例
        m_OffsetLength = (m_OffsetedLowerPoint - m_OffsetedUpperPoint).magnitude;

        m_LowerPortion = (m_OffsetedLowerPoint - m_EnterPosition).magnitude / m_OffsetLength;

        m_UpperPortion = (m_OffsetedUpperPoint - m_EnterPosition).magnitude / m_OffsetLength;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == m_PlayerTag)
        {
            m_EnterPosition = other.transform.position;
            m_MoveAmount = 0;
            m_FPSController = other.transform.GetComponent<FirstPersonController>();
            m_IsIn = true;
            m_FPSController.m_CanPhisicsMove = false;
            GetProjectingPoint();
        }
    }


    //將數值視覺化,Debug用
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(LowerPoint() + Vector3.right * 0.3f, LowerPoint() - Vector3.right * 0.3f);
        Gizmos.DrawLine(LowerPoint() + Vector3.forward * 0.3f, LowerPoint() - Vector3.forward * 0.3f);
        Gizmos.DrawLine(LowerPoint(), UpperPoint());
        Gizmos.DrawLine(UpperPoint() + Vector3.right * 0.3f, UpperPoint() - Vector3.right * 0.3f);
        Gizmos.DrawLine(UpperPoint() + Vector3.forward * 0.3f, UpperPoint() - Vector3.forward * 0.3f);


        if (m_EnterPosition != Vector3.zero && m_OffsetedLowerPoint != Vector3.zero && m_OffsetedUpperPoint != Vector3.zero)
        {
            Gizmos.DrawWireSphere(m_EnterPosition, 0.2f);


            Gizmos.color = Color.blue;
            Gizmos.DrawCube(m_OffsetedUpperPoint, Vector3.one * 0.4f);
            Gizmos.DrawLine(m_EnterPosition, m_OffsetedUpperPoint);

            Gizmos.color = Color.green;
            Gizmos.DrawCube(m_OffsetedLowerPoint, Vector3.one * 0.4f);
            Gizmos.DrawLine(m_EnterPosition, m_OffsetedLowerPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawCube(m_CurrentPosition, Vector3.one * 0.4f);
        }
        if (m_MoveAmount != 0)
        {

            if (m_MoveAmount > 0 && m_MoveAmount < 1)
            {
                Gizmos.DrawLine(m_EnterPosition + Vector3.right * 0.3f, m_CurrentPosition + Vector3.right * 0.3f);
            }
            else if (m_MoveAmount < 0 && m_MoveAmount > -1)
            {

                Gizmos.DrawLine(m_EnterPosition + Vector3.right * 0.3f, m_CurrentPosition + Vector3.right * 0.3f);
            }
        }
    }
}
