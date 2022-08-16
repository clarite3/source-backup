using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDMenu : MonoBehaviour
{
    public Button bt_CashStore;
    public Button bt_Mail;
    public Button bt_Inventory;
    public Button bt_Guild;
    public Button bt_Party;
    public Button bt_Ranking;
    public Button bt_Option;
    public Button bt_Exit;
    public Button bt_Status;
    public Button bt_Skill;
    public Button bt_Quest;
    public Button bt_FindPlayer;

    private void Awake()
    {

        //bt_CashStore.onClick.AddListener(delegate { onClickOpenUI(bt_CashStore.gameObject.name); });
        bt_Mail.onClick.AddListener(delegate 
        {
            if (InGameVariable.Inst.panelMail.gameObject.activeSelf == false)
                InGameVariable.Inst.panelMail.Active();
            else
                InGameVariable.Inst.panelMail.Close();
        } );
        bt_Inventory.onClick.AddListener(delegate { onClickOpenUI(bt_Inventory.gameObject.name); });
        bt_Guild.onClick.AddListener(delegate { InGameVariable.Inst.panelGuild.updateState(); });
        //bt_Party.onClick.AddListener(delegate { onClickOpenUI(bt_Party.gameObject.name); });
        //bt_Ranking.onClick.AddListener(delegate { onClickOpenUI(bt_Ranking.gameObject.name); });
        bt_Option.onClick.AddListener(delegate { onClickOpenUI(bt_Option.gameObject.name); });
        bt_Status.onClick.AddListener(delegate { onClickOpenUI(bt_Status.gameObject.name); });
        //bt_FindPlayer.onClick.AddListener(delegate { onClickOpenUI(bt_FindPlayer.gameObject.name); });
        bt_Exit.onClick.AddListener(delegate { onClickExitGame(); });
        bt_Skill.onClick.AddListener(delegate 
        {
            if (InGameVariable.Inst.panelSkill.gameObject.activeSelf == false)
                InGameVariable.Inst.panelSkill.Active();
            else
                InGameVariable.Inst.panelSkill.Close();
        });
        bt_Quest.onClick.AddListener(delegate 
        {
            if (InGameVariable.Inst.panelQuestNew.gameObject.activeSelf == false)
                InGameVariable.Inst.panelQuestNew.Active();
            else
                InGameVariable.Inst.panelQuestNew.Close();
        });
    }

    private void OnDestroy()
    {
        //bt_CashStore.onClick.RemoveAllListeners();
        bt_Mail.onClick.RemoveAllListeners();
        bt_Inventory.onClick.RemoveAllListeners();
        bt_Guild.onClick.RemoveAllListeners();
        //bt_Party.onClick.RemoveAllListeners();
        //bt_Ranking.onClick.RemoveAllListeners();
        bt_Option.onClick.RemoveAllListeners();
        bt_Exit.onClick.RemoveAllListeners();
        bt_Status.onClick.RemoveAllListeners();
        bt_Skill.onClick.RemoveAllListeners();
        bt_Quest.onClick.RemoveAllListeners();
        //bt_FindPlayer.onClick.RemoveAllListeners();
    }

    public void onClickOpenUI(string name)
    {
        GameObject go = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Panel " + name).gameObject;
        PanelBase pb = go.GetComponent<PanelBase>();

        if (pb != null)
        {
            if (go.activeSelf == false)
            {
                //go.SetActive(true);
                pb.Active();
            }
            else
            {
                //go.SetActive(false);
                pb.Close();
            }
        }
        else
            return;
    }

    public void onClickExitGame()
    {
#if UNITY_EDITOR
        ConfirmPopup.inst.Init("알림", "게임을 종료하시겠습니까?", ConfirmPopup.Type.YesOrNo, () => { UnityEditor.EditorApplication.isPlaying = false; } );
#elif UNITY_STANDALONE_WIN
        ConfirmPopup.inst.Init("알림", "게임을 종료하시겠습니까?", ConfirmPopup.Type.YesOrNo, () => { Application.Quit(); } );
#endif
    }
}
