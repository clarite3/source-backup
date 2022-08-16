using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.ootii.Messages;

public class PanelBank : PanelBaseChild
{
    public TextMeshProUGUI systemMessage;
    public TextMeshProUGUI safe;
    public TextMeshProUGUI inventory;
    public TMP_InputField input;


    /// <summary>
    /// 열렸을 때 정보 받아오기
    /// </summary>
    private void OnEnable()
    {
        Init();
        MsgDisp.AddListener(DT.eMsg.UI_Refresh_HUD, Init);
    }

    private void Init()
    {
        inventory.text = DataMgr.Inst.dataGoods.m_iMoney.ToString();
        safe.text = DataMgr.Inst.dataGoods.m_iMoneyBank.ToString();
        input.text = null;
        input.ActivateInputField();
    }

    public void Init(IMessage msg)
    {
        Init();
    }

    private void OnDisable()
    {
        input.text = null;
        if (input.isFocused) input.Select();
        MsgDisp.RemoveListener(DT.eMsg.UI_Refresh_HUD, Init);
    }


    public void onDeposit()
    {

        int.TryParse(input.text, out int num);
        int.TryParse(inventory.text, out int inventoryMoney);
        if (num != 0 && inventoryMoney >= num)
        {
            NetMgr.Inst.reqDepositMoney(DataMgr.Inst.zoneNum, num);
            Debug.Log(num + "원 입금");
        }

    }

    public void onWithdraw()
    {
        int.TryParse(input.text, out int num);
        int.TryParse(safe.text, out int safeMoney);
        if (num != 0 && safeMoney >= num)
        {
            NetMgr.Inst.reqWithdrawMoney(DataMgr.Inst.zoneNum, num);
            Debug.Log(num + "원 출금");
        }
    }

    
}
