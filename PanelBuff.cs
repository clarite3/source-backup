using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.ootii.Messages;
using System.Linq;

public class PanelBuff : MonoBehaviour
{
    private Sprite[] sprites;
    private Dictionary<string, Sprite> dic = new Dictionary<string, Sprite>();
    public GameObject prefabBuff;
    public Transform tr;
    public List<GameObject> buffs = new List<GameObject>();


    private void Start()
    {
        sprites = Resources.LoadAll<Sprite>("UI/Buff");
        
        foreach(var val in sprites)
        {
            dic.Add(val.name, val);
        }

        //입장할때 받아왔던 시간이랑 현재 시간을 계산해서 버프 지속시간 차이나는걸 비슷하게 맞춰준다

        DateTime startTime = DateTime.Now;

        TimeSpan timeCal =  startTime - DataMgr.Inst.enterTime;
        

        foreach (var val in DataMgr.Inst.myBuff)
        {
            //세트 아이템 효과등 지속 버프면
            if (val.m_iDuration == int.MaxValue)
                continue;
            Sprite sprite = null;

            int abilityIndex = Tables.data_item.Get(val.m_iIndex).ability_index;

            int duration = val.m_iCurRemaingTime - timeCal.Seconds;

            Tables.data_ability data_Ability = Tables.data_ability.data_list.FirstOrDefault(x => x.Value.ability_index == abilityIndex).Value;

            dic.TryGetValue(data_Ability.item_icon, out sprite);

            GameObject go = Instantiate(prefabBuff, tr);

            Buff buff = go.GetComponent<Buff>();

            buff.setBuff(abilityIndex, duration, val.m_eAbType , val.m_iInstId, sprite);

            buffs.Add(go);
        }
        
        MsgDisp.AddListener(DT.eMsg.UI_Use_Buff, SetBuff);
    }

    private void OnDestroy()
    {
        MsgDisp.RemoveListener(DT.eMsg.UI_Use_Buff, SetBuff);
    }

    public void SetBuff(IMessage msg)
    {
        object[] data = msg.Data as object[];



        int abilityIndex = (int)data[0];
        int duration = (int)data[1];
        NDT.eAbilityType AbilityType = (NDT.eAbilityType)data[2];
        int instId = (int)data[3];
        bool isBuff = (bool)data[4];     

        // 검색해서 같은게 있으면 지속시간만 바꿔주고 리턴
        foreach(var val in buffs)
        {
            Buff checkBuff = val.GetComponent<Buff>();

            if (checkBuff.AbilityType == AbilityType)
            {
                checkBuff.duration = duration;
                return; 
            }
        }

        Sprite sprite = null;
        foreach (var val in Tables.data_skill.data_list)
        {
            if (val.Value.ability_idx == abilityIndex)
                dic.TryGetValue(val.Value.icon_name, out sprite);

        }

        //없으면 생성

        GameObject go = Instantiate(prefabBuff, tr);

        Buff buff = go.GetComponent<Buff>();

        buff.setBuff(abilityIndex, duration, AbilityType, instId, sprite);      

        buffs.Add(go);


    }
}
