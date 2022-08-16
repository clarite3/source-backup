using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TooltipItem : MonoBehaviour {

    [SerializeField]
    private Camera uiCamera;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI agilityText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI intellectText;
    [SerializeField] private GameObject      valueLine_1;
    [SerializeField] private GameObject      valueLine_2;
    [SerializeField] private GameObject      valueLine_3;
    [SerializeField] private TextMeshProUGUI durability;
    [SerializeField] private TextMeshProUGUI abilityValue1;
    [SerializeField] private TextMeshProUGUI abilityValue2;
    [SerializeField] private TextMeshProUGUI abilityValue3;
    [SerializeField] private TextMeshProUGUI abilityValue4;
    [SerializeField] private TextMeshProUGUI abilityValue5;
    [SerializeField] private TextMeshProUGUI timeLimit;

    [SerializeField] private GameObject      setEffectArea;
//    [SerializeField] private SetAbilityTooltip      setEffectPanel;


    string year;
    string month;
    string day;
    string hour;
    string minute;
    string second;

    float halfwidth;
    RectTransform rt;

    public void Init()
    {
        //Debug.Log("ShowTooltip Init");
        halfwidth = InGameVariable.Inst.canvas.GetComponent<CanvasScaler>().referenceResolution.x * 0.5f;
        rt = GetComponent<RectTransform>();


        HideTooltip();
    }

    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
        transform.localPosition = localPoint;

        if(rt.anchoredPosition.x + rt.sizeDelta.x > halfwidth)
        {
            rt.pivot = new Vector2(1, 1);
        }
        else
        {
            rt.pivot = new Vector2(0, 1);
        }
    }

    public void ShowTooltip(Tables.data_item tableData, NDT.DataItem data, string itemName, int count, bool setEffect = false) 
    {
        abilityValue1.text = "";

        valueLine_2.SetActive(false);
        abilityValue2.text = "";
        abilityValue3.text = "";

        valueLine_3.SetActive(false);
        abilityValue4.text = "";
        abilityValue4.text = "";

        gameObject.SetActive(true);
        this.transform.SetAsLastSibling();

        Tables.data_item _tableData = tableData;
        NDT.DataItem _data = data;

        //Debug.Log("ShowTooltip");
        titleText.text = tableData.item_type < 1000 ? "(" + _tableData.index + ") " + "+" + data.m_byGrade + " " + itemName : titleText.text = "(" + _tableData.index + ") " + " " + itemName;
     
        countText.text = "수량 : " + count.ToString();
        if(DataMgr.Inst.PlayerEx.m_iFinalAgl >= _tableData.need_agility)
            agilityText.text =      "민 : " + _tableData.need_agility.ToString();
        else
            agilityText.text = UtilFunc.TextColoring(255, 0, 0, "민 : " + _tableData.need_agility.ToString());
        if(DataMgr.Inst.PlayerEx.m_iFinalStr >= _tableData.need_strength)
            strengthText.text =     "힘 : " + _tableData.need_strength.ToString();
        else
            strengthText.text = UtilFunc.TextColoring(255, 0, 0, "힘 : " + _tableData.need_strength.ToString());
        if(DataMgr.Inst.PlayerEx.m_iFinalVig >= _tableData.need_vigor)
            energyText.text =       "기 : " + _tableData.need_vigor.ToString();
        else
            energyText.text = UtilFunc.TextColoring(255, 0, 0, "기 : " + _tableData.need_vigor.ToString());
        if(DataMgr.Inst.PlayerEx.m_iFinalInt >= _tableData.need_intelligence)
            intellectText.text =    "지 : " + _tableData.need_intelligence.ToString();
        else
            intellectText.text = UtilFunc.TextColoring(255, 0, 0, "지 : " + _tableData.need_intelligence.ToString());

        float duraPer = (float)_data.m_iDurability / tableData.durability;

        if (tableData.period == 0 || _data.m_dtPeriod == 0)
        {
            durability.text = string.Format("내구도 : " + "{0:P0}", duraPer);
            timeLimit.text = null;
            timeLimit.gameObject.SetActive(false);
        }
        else
        {

            DateTime dt = setTimes(_data.m_dtPeriod);

            durability.text = "내구도 : ∞";
            timeLimit.text = string.Format("{0}년 {1}월 {2}일 {3}시 {4}분 까지\n사용가능합니다.", year, month, day, hour, minute);
            timeLimit.gameObject.SetActive(true);
        }


        durability.color = duraPer < 0.2f ? new Color32(255, 0, 0, 255) : new Color32(255, 255, 255, 255);

  
        List<Tables.data_ability>   data_ability_List = UtilFunc.Collect_data_ability(_tableData.ability_index);

        if(data_ability_List.Count > 0)
        {
            TextMeshProUGUI    text_obj = null;
            for(int i = 0; i < data_ability_List.Count; i++)
            {
                if(i >= 1)                              // 구분선 출력 체크
                    valueLine_2.SetActive(true);        
                else 
                    valueLine_2.SetActive(false);

                if(i >= 3)                              // 구분선 출력 체크
                    valueLine_3.SetActive(true);
                else 
                    valueLine_3.SetActive(false);

                switch(i)
                {
                    case 0:
                        text_obj = abilityValue1;
                        break;
                    case 1:
                        text_obj = abilityValue2;
                        break;
                    case 2:
                        text_obj = abilityValue3;
                        break;
                    case 3:
                        text_obj = abilityValue4;
                        break;
                    case 4:
                        text_obj = abilityValue5;
                        break;
                    default:
                        break;
                }
                if(text_obj != null)
                {
                    int value = 0;
                    value = CalculateItemAbilityValue(data_ability_List[i], data.m_byGrade);
                    text_obj.text = Tables.data_ability_type.Get(data_ability_List[i].ability_type).ability_effect + ": " + value.ToString();
                    if(data_ability_List[i].formula == 2)
                        text_obj.text += "%";
                }
            }
        }
        else
        {
            abilityValue1.text = "";

            valueLine_2.SetActive(false);
            abilityValue2.text = "";
            abilityValue3.text = "";

            valueLine_3.SetActive(false);
            abilityValue4.text = "";
            abilityValue4.text = "";
        }

        if(setEffect == true)
        {
            if(DataMgr.Inst.PlayerEx.m_arrSetItemIndex.Count != 0)
            {
                for(int i = 0; i < DataMgr.Inst.PlayerEx.m_arrSetItemIndex.Count; i++)
                {
                    SetAbilityTooltip   tmpSetEffect = Instantiate<SetAbilityTooltip>(InGameVariable.Inst.setAbilityTooltip);

                    tmpSetEffect.transform.SetParent(setEffectArea.transform);
                    tmpSetEffect.transform.localScale = UnityEngine.Vector3.one;
                    tmpSetEffect.transform.localPosition = UnityEngine.Vector3.zero;

                    tmpSetEffect.SetEffect(DataMgr.Inst.PlayerEx.m_arrSetItemIndex[i]);
                    tmpSetEffect.transform.gameObject.SetActive(true);
                }
            }
        }

        Update();
    }

    public void ShowSetAbilityPannel()
    {

    }

    private DateTime setTimes(Int64 item_period)
    {
        DateTime periodTime = new DateTime(item_period * 10000000);
        DateTime epoch = new DateTime(1970, 1, 1);
        Int64 period = periodTime.Ticks + epoch.Ticks;
        DateTime dt = new DateTime(period);

        dt = dt.ToLocalTime();

        year = dt.Year.ToString().Remove(0, 2);
        month = dt.Month.ToString();
        day = dt.Day.ToString();
        hour = dt.Hour.ToString();
        minute = dt.Minute.ToString();
        second = dt.Second.ToString();

        return dt;
    }

    public int CalculateItemAbilityValue(Tables.data_ability data, byte grade)
    {
        int value = 0;
	    switch ((DT.eAbilityFormula)data.formula)
	    {
		    // 값 덧셈 
		    case DT.eAbilityFormula.Value:
		    {
			    // 체력/마력/물공/물방/마공/마방
			    // Stat_MaxHp Stat_MaxMp Stat_AtkPow Stat_DefPow Stat_MAtkPow Stat_MDefPow
			    if ((int)DT.eAbilityType.Stat_MaxHp <= data.ability_type && data.ability_type <= (int)DT.eAbilityType.Stat_MDefPow)
				    value = (int)(data.value1 + (int)(data.value1 * grade) * 0.2);
			    // 그외
                else 
                    value = data.value1;			    
                return value;
		    }
            //break;
		    // 값 퍼센트
		    case DT.eAbilityFormula.Percent:
		    {
			    // 체력/마력/물공/물방/마공/마방
			    switch ((DT.eAbilityType)data.ability_type)
			    {
				    // 체력
				    case DT.eAbilityType.Stat_MaxHp:
					    return (int)((data.value1 + (grade * 2)) * 0.0001);
				    // 마력
				    case DT.eAbilityType.Stat_MaxMp:
					    return (int)((data.value1 + (grade * 2)) * 0.0001);
				    /// 물공
				    case DT.eAbilityType.Stat_AtkPow:
					    return (int)((data.value1 + (grade * 2)) * 0.0001);
				    // 물방
				    case DT.eAbilityType.Stat_DefPow:
					    return (int)((data.value1 + (grade * 2)) * 0.0001);
				    // 마공
				    case DT.eAbilityType.Stat_MAtkPow:
					    return (int)((data.value1 + (grade * 2)) * 0.0001);
				    // 마방
				    case DT.eAbilityType.Stat_MDefPow:
					    return (int)((data.value1 + (grade * 2)) * 0.01);
			    }

			    // 그외, 일반적인 퍼센트 증가
			    // 원본의 페센티지 적용된 값
			    float pctVal = (float)(data.value1 * 0.0001);

			    // 적용된 값에 강화도만큼 증가
			    return (int)(pctVal + (pctVal * (grade * 0.2)));
		    }
	    }
        return 0;
    }

    public void HideTooltip() {
        gameObject.SetActive(false);

        for(int i = 0; i < setEffectArea.transform.childCount; i++)
        {
            GameObject  tmp = setEffectArea.transform.GetChild(i).gameObject;
            Destroy(tmp);
        }

    }

}
