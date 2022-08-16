using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using com.ootii.Messages;
using UnityEngine.EventSystems;

public class Actor
{
    /// <summary>
    /// 실제 함수 실행하는 클래스
    /// </summary>

    public void clearSkill() 
	{ 
		InGameVariable.Inst.panelSkill.ResetSelectSkill(); 
	}
    public void setSkill_1st() 
    { 
        InGameVariable.Inst.panelSkill.HotKeySelectItem(KeyCode.F2);
        InputMgr.Inst.UseSkillIfUseable(KeyCode.F2);
    }
    public void setSkill_2nd() 
    { 
        InGameVariable.Inst.panelSkill.HotKeySelectItem(KeyCode.F3);
        InputMgr.Inst.UseSkillIfUseable(KeyCode.F3);
    }
    public void setSkill_3rd() 
    { 
        InGameVariable.Inst.panelSkill.HotKeySelectItem(KeyCode.F4);
        InputMgr.Inst.UseSkillIfUseable(KeyCode.F4);
    }
    public void setSkill_4th() 
    { 
        InGameVariable.Inst.panelSkill.HotKeySelectItem(KeyCode.F5);
        InputMgr.Inst.UseSkillIfUseable(KeyCode.F5);
    }
    public void setSkill_5th() 
    { 
        InGameVariable.Inst.panelSkill.HotKeySelectItem(KeyCode.F6);
        InputMgr.Inst.UseSkillIfUseable(KeyCode.F6);
    }
    public void setSkill_6th() 
    { 
        InGameVariable.Inst.panelSkill.HotKeySelectItem(KeyCode.F7);
        InputMgr.Inst.UseSkillIfUseable(KeyCode.F7);
    }
    public void setSkill_7th() 
    { 
        InGameVariable.Inst.panelSkill.HotKeySelectItem(KeyCode.F8);
        InputMgr.Inst.UseSkillIfUseable(KeyCode.F8);
    }
    public void useQuickSlot_Left() { InGameVariable.Inst.itemHotKeyArea.useLeftItem(); }
    public void useQuickSlot_Right() { InGameVariable.Inst.itemHotKeyArea.useRightItem(); }
    public void setUIStatus() { InputMgr.Inst.OpenAndCloseUI("Status"); }
    public void setUIInventory() { InputMgr.Inst.OpenAndCloseUI("Inventory"); }
    public void setUISkill() { InputMgr.Inst.OpenAndCloseUI("Skill"); }
    public void setUIFindPlayer() { /*InputMgr.Inst.OpenAndCloseUI("FindPlayer");*/ }
    public void setUIParty() { /*InputMgr.Inst.OpenAndCloseUI("Party");*/InputMgr.Inst.onClickZoneListButton(); }
    public void setUIOption() { InputMgr.Inst.OpenAndCloseUI("Option"); }

    public void setUIMap() 
    {
        GameObject go = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Panel Map").gameObject;
        if (!go.activeSelf) go.SetActive(true);
        else go.SetActive(false);
    }
    public void setMap() { }
    public void printScreenshot() { InputMgr.Inst.Screenshot(); }
    public void closeUI()
    {
        if(PopupMgr.Instance.popupStack.Count == 0)
        {
            InGameVariable.Inst.editorConsole.Active();
            return;
        }

        for (int i = PopupMgr.Instance.popupStack.Count; i > 0; i--)
        {
            PopupMgr.Instance.Close();
        }
    }

}

/// <summary>
/// 행동 지정용 이넘
/// </summary>
public enum eActions
{
    ClearSkill,
    SetSkill1st,
    SetSkill2nd,
    SetSkill3rd,
    SetSkill4th,
    SetSkill5th,
    SetSkill6th,
    SetSkill7th,
    UseQuickSlot_Left,
    UseQuickSlot_Right,
    OpenUIStatus,
    OpenUIInventory,
    OpenUISkill,
    OpenUIFindPlayer,
    OpenUIParty,
    OpenUIOption,
    OpenUIMap,
    PrintScreenshot,
    CloseUI,
    Max,
}

/// <summary>
/// 세팅한 키 값 저장할 딕셔너리
/// </summary>
public static class KeySetting 
{ 
	public static Dictionary<eActions, KeyCode> keys = new Dictionary<eActions, KeyCode>();
}

public class InputMgr : Singleton<InputMgr>
{

    public Transform inputTargetTr;

    bool isDown;

    /// <summary>
    /// 키세팅할 키코드 지정
    /// </summary>
    KeyCode[] defaultKeys = new KeyCode[]
    {KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5,
    KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10,
    KeyCode.Insert, KeyCode.Home, KeyCode.PageUp, KeyCode.Delete,
    KeyCode.End, KeyCode.PageDown, KeyCode.SysReq, KeyCode.Escape,
    KeyCode.M };

    eActions pressedKey = eActions.Max;

    /// <summary>
    /// 각 키에 맞게 커맨드 클래스 생성
    /// </summary>

    Command F1, F2, F3, F4, F5, F6, F7, F8, F9, F10,
        Del, End, PgDown, Ins, Home, PgUp, Prt, Esc,
        M;

    Actor actor;

    [SerializeField]private Sprite[] Cursors; // 스프라이트 모음
    private List<Texture2D> cursorTexture; // 텍스쳐2D로 변환한 커서
    public Dictionary<int, Sprite> cursorDic = new Dictionary<int, Sprite>();
    public enum CURSORSTATE
    {
        None,
        Normal,
        MoveZone,
        Trade,
        Repair,
        Max,
    }

    public CURSORSTATE cursorState = CURSORSTATE.None;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < (int)eActions.Max; i++)
        {
            KeySetting.keys.Add((eActions)i, defaultKeys[i]);
        }
        actor = new Actor();
        LoadCursor();
        SetCommand();
        SetKeySetting();
        useGUILayout = false;
        isCompleteLoading = true;
    }

    void LoadCursor()
    {
        Cursors = Resources.LoadAll<Sprite>("Cursor/cursor");
        cursorTexture = new List<Texture2D>();
        for (int i = 0; i < Cursors.Length; i++)
        {
            cursorTexture.Add(ConvertSpriteToTexture(Cursors[i]));
            cursorDic[i] = Cursors[i];
        }
    }

    /// <summary>
    /// Sprite -> Texture2D 변환해주는 함수
    /// </summary>
    Texture2D ConvertSpriteToTexture(Sprite sprite)
    {
        try
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                int x = Mathf.FloorToInt(sprite.textureRect.x);
                int y = Mathf.FloorToInt(sprite.textureRect.y);
                int width = Mathf.FloorToInt(sprite.textureRect.width);
                int height = Mathf.FloorToInt(sprite.textureRect.height);

                Texture2D newText = new Texture2D(width, height);
                Color[] newColors = sprite.texture.GetPixels(x, y, width, height);

                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }
        catch
        {
            return sprite.texture;
        }
    }

    /// <summary>
    /// 커맨드 클래스 초기화
    /// </summary>
    void SetCommand()
    {

        F1  = new CommandClearSkill();
        F2  = new CommandSetSkill1st();
        F3  = new CommandSetSkill2nd();
        F4  = new CommandSetSkill3rd();
        F5  = new CommandSetSkill4th();
        F6  = new CommandSetSkill5th();
        F7  = new CommandSetSkill6th();
        F8  = new CommandSetSkill7th();
        F9  = new CommandUseQuickSlotLeft();
        F10 = new CommandUseQuickSlotRight();
        Ins = new CommandUIStatus();
        Home = new CommandUIInventory();
        PgUp = new CommandUISkill();
        Del = new CommandUIFindPlayer();
        End = new CommandUIParty();
        PgDown = new CommandUIOption();
        M = new CommandUIMap();
        Prt = new CommandPrintScreenshot();
        Esc = new CommandCloseUI();



    }

    /// <summary>
    /// PlayerPrefs로 저장해놓은 키세팅 로드
    /// </summary>

    void SetKeySetting()
    {
        if(PlayerPrefs.GetString("ClearSkill") != null)
        {
            for (int i = 0; i < KeySetting.keys.Count; i++)
            {
                eActions act = (eActions)i;
                string str = PlayerPrefs.GetString(act.ToString());
                System.Enum.TryParse(str, out KeyCode key);
                KeySetting.keys[(eActions)i] = key;
            }
        }
        else
        {
            for (int i = 0; i < (int)eActions.Max; i++)
            {
                KeySetting.keys[(eActions)i] = defaultKeys[i];
            }
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        // 키 값 받아와서 각 행동 실행
        Command command = GetCommand();
        if (command != null)
        {
            command.Execute(actor);
        }

        UpdateMouseCursor();

        //이 구조는 테스트를 위함. 새로 짜야함
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == DT.eSceneType.InGame.ToString())
        {
            UpdateTargetPosition();
        }
    }
    /// <summary>
    /// 마우스 커서
    /// </summary>
    void UpdateMouseCursor()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (cursorState == CURSORSTATE.None)
        {
            Cursor.SetCursor(cursorTexture[0], new Vector2(0, 0), CursorMode.Auto);
            cursorState = CURSORSTATE.Normal;
        }
            
    }

    /// <summary>
    /// 키 입력했을 때 세팅해놓은 값에 맞춰 커맨드 클래스로 반환해주는 함수
    /// </summary>
    Command GetCommand()
    {
        if (IsPressed(eActions.ClearSkill)) return F1;
        if (IsPressed(eActions.SetSkill1st)) return F2;
        if (IsPressed(eActions.SetSkill2nd)) return F3;
        if (IsPressed(eActions.SetSkill3rd)) return F4;
        if (IsPressed(eActions.SetSkill4th)) return F5;
        if (IsPressed(eActions.SetSkill5th)) return F6;
        if (IsPressed(eActions.SetSkill6th)) return F7;
        if (IsPressed(eActions.SetSkill7th)) return F8;
        if (IsPressed(eActions.UseQuickSlot_Left)) return F9;
        if (IsPressed(eActions.UseQuickSlot_Right)) return F10; 
        if (IsPressed(eActions.OpenUIStatus)) return Ins;
        if (IsPressed(eActions.OpenUIInventory)) return Home;
        if (IsPressed(eActions.OpenUISkill)) return PgUp;
        if (IsPressed(eActions.OpenUIFindPlayer)) return Del;
        if (IsPressed(eActions.OpenUIParty)) return End;
        if (IsPressed(eActions.OpenUIOption)) return PgDown;
        if (IsPressed(eActions.OpenUIMap)) return M;
        if (IsPressed(eActions.PrintScreenshot)) return Prt;
        if (IsPressed(eActions.CloseUI)) return Esc;
        return null;
    }


    /// <summary>
    /// 실제 키 입력 받아서 이넘에 값 넣음
    /// </summary>
    bool IsPressed(eActions key)
    {
        pressedKey = eActions.Max;

        if (Input.GetKeyDown(KeySetting.keys[eActions.ClearSkill]))
            pressedKey = eActions.ClearSkill;
        if (Input.GetKeyDown(KeySetting.keys[eActions.SetSkill1st]))
            pressedKey = eActions.SetSkill1st;
        if (Input.GetKeyDown(KeySetting.keys[eActions.SetSkill2nd]))
            pressedKey = eActions.SetSkill2nd;
        if (Input.GetKeyDown(KeySetting.keys[eActions.SetSkill3rd]))
            pressedKey = eActions.SetSkill3rd;
        if (Input.GetKeyDown(KeySetting.keys[eActions.SetSkill4th]))
            pressedKey = eActions.SetSkill4th;
        if (Input.GetKeyDown(KeySetting.keys[eActions.SetSkill5th]))
            pressedKey = eActions.SetSkill5th;
        if (Input.GetKeyDown(KeySetting.keys[eActions.SetSkill6th]))
            pressedKey = eActions.SetSkill6th;
        if (Input.GetKeyDown(KeySetting.keys[eActions.SetSkill7th]))
            pressedKey = eActions.SetSkill7th;
        if (Input.GetKeyDown(KeySetting.keys[eActions.UseQuickSlot_Left]))
            pressedKey = eActions.UseQuickSlot_Left;
        if (Input.GetKeyDown(KeySetting.keys[eActions.UseQuickSlot_Right]))
            pressedKey = eActions.UseQuickSlot_Right;
        if (Input.GetKeyDown(KeySetting.keys[eActions.OpenUIStatus]))
            pressedKey = eActions.OpenUIStatus;
        if (Input.GetKeyDown(KeySetting.keys[eActions.OpenUIInventory]))
            pressedKey = eActions.OpenUIInventory;
        if (Input.GetKeyDown(KeySetting.keys[eActions.OpenUISkill]))
            pressedKey = eActions.OpenUISkill;
        if (Input.GetKey(KeySetting.keys[eActions.OpenUIFindPlayer]))
            pressedKey = eActions.OpenUIFindPlayer;
        if (Input.GetKeyDown(KeySetting.keys[eActions.OpenUIParty]))
            pressedKey = eActions.OpenUIParty;
        if (Input.GetKeyDown(KeySetting.keys[eActions.OpenUIOption]))
            pressedKey = eActions.OpenUIOption;
        if (Input.GetKeyDown(KeySetting.keys[eActions.OpenUIMap]))
            pressedKey = eActions.OpenUIMap;
        if (Input.GetKeyDown(KeySetting.keys[eActions.PrintScreenshot]))
            pressedKey = eActions.PrintScreenshot;
        if (Input.GetKeyDown(KeySetting.keys[eActions.CloseUI]))
            pressedKey = eActions.CloseUI;
        return (key == pressedKey);
    }

    public ItemDropZone pointerEnter_ItemDropZone; //드랍존 어디에 마수스포인트가 위치해있는지
    public void UpdateTargetPosition()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
            if (Input.GetMouseButtonDown(0))
        {

            isDown = true;
        }

        //워프존 확인
      
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDown == false) return;
            isDown = false;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                onMouseLeftClick();
            }
        }
        else if(Input.GetMouseButtonUp(1))  // 마우스 우클릭
        {
            //if (isDown == false) return;
            //isDown = false;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 arrivePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //아이템 선택확인
                Ray2D ray = new Ray2D(arrivePos, Vector2.zero);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, 1 << LayerMask.NameToLayer(DT.eLayerType.Default.ToString()));

                var player = hit.transform?.GetComponent<Player>();

                //if (player != null && player.IsMine() == false  )
                //{
                //    //UISystem
                //    //panelOnPlayer
                //    InGameVariable.Inst.panelOnPlayer.Init(player);
                //
                //}
                onMouseRightClick();
            }
            else
            {
                InGameVariable.Inst.panelOnPlayer.gameObject.SetActive(false);
            }
        }
    }
    //선택한 대상 사정거리안에 있을때 호출하여 처리
    public bool ProcesseInputTargetInRange(Transform mine, Transform target, bool isLeftClick)
    {        
        Player p = CharacterMgr.Inst.myPlayer;
        DT.eTagType targetTag = UtilFunc.ParseEnum<DT.eTagType>(target.tag);

        bool useSkill = SetSkillToTarget(target, isLeftClick);
        //스킬을 사용한다면 return
        if (useSkill == true)
        {
            return true;
        }
        //자신이라면 return (자신에게 쓰는 스킬이 있을수있으므로 스킬사용 밑에 둬야한다.
        if(CharacterMgr.Inst.myPlayer.transform == target)
        {
            return false;
        }

        int mineBlock = LandMgr.Inst.GetBlockNumber(mine.transform.position);
        int targetBlock = LandMgr.Inst.GetBlockNumber(target.transform.position);

        Vector3Int tmpTile = LandMgr.Inst.WorldToCell(mine.transform.position);
        Vector2Int myTile = new Vector2Int(127 - tmpTile.y, 127 - tmpTile.x);
        tmpTile = LandMgr.Inst.WorldToCell(target.transform.position);
        Vector2Int targetTile = new Vector2Int(127 - tmpTile.y, 127 - tmpTile.x);

        //if (UtilFunc.IsDistance(mine.transform.position, CharacterMgr.Inst.myPlayer.GetNormalAttackRange() + 0.1f, target.transform.position) == false)
        if(UtilFunc.IsDistance(myTile, (int)CharacterMgr.Inst.myPlayer.GetNormalAttackRange(), targetTile) == false)
        {
            return false;
        }
		

		if (targetTag == DT.eTagType.MONSTER || targetTag == DT.eTagType.PLAYER)
        {

            Character _target = target.GetComponent<Character>();
			//NDT.eCharacterType eCharacterType = NDT.eCharacterType.Player;
			//if (targetTag == DT.eTagType.MONSTER || targetTag == DT.eTagType.NPC)
			//{
			//    eCharacterType = NDT.eCharacterType.Npc;
			//}

			if(_target.isDie == true)
			{
				// 죽으면 공격 안한다
				return true;
			}


			bool isEnableAttack = false;
            /// 상점
            if(targetTag == DT.eTagType.MONSTER)
            {
                NDT.eNpcType npcType = (NDT.eNpcType)(Tables.data_npc.Get(_target.m_npc.m_dataNPlayer.m_iIndex).npc_type);
                if ((npcType >= NDT.eNpcType.MonsterBossBegin && npcType <= NDT.eNpcType.MonsterBossEnd) || 
                    npcType >= NDT.eNpcType.MonsterNormalBegin && npcType <= NDT.eNpcType.MonsterNormalEnd)
                {
                    isEnableAttack = true;
                }

            }
            if (targetTag == DT.eTagType.PLAYER)
            {
                isEnableAttack = true;
            }


            if (isEnableAttack)
            {
                if(CharacterMgr.Inst.myPlayer.currentBlock != mineBlock)
                {
                    CharacterMgr.Inst.myPlayer.currentBlock = mineBlock;

                    NetMgr.Inst.reqMove(DT.eCharacterType.PC, DataMgr.Inst.zoneNum, p.transform.position.x, p.transform.position.y, (byte)DT.eMoveKind.ARRIVE, (int)DataMgr.Inst.PlayerEx.m_hostId);
                }
                //Debug.Log("공격 myTile X: " + myTile.x + " Y: " + myTile.y + " target X: " + targetTile.x + " Y: "+ targetTile.y);

				// 말타고 있으면 공격 안함
				if(CharacterMgr.Inst.myPlayer.bGetOn == true)
				{
					return true;
				}

                //NetMgr.Inst.reqAttack(DataMgr.Inst.zoneNum, (int)_target.id);
                CharacterMgr.Inst.myPlayer.SetAttack(_target);
                return true;
            }


        }
       // else if(targetTag == DT.eTagType.NPC)   // NPC 클릭시 메뉴를 띄워야함
       // {
       //     // NPC 기능마다 띄워야하는 메뉴의 종류를 따로
       //     
       //    
       // }
        else if (targetTag == DT.eTagType.ITEM)
        {
            NetMgr.Inst.reqMove(DT.eCharacterType.PC, DataMgr.Inst.zoneNum, p.transform.position.x, p.transform.position.y, (byte)DT.eMoveKind.ARRIVEITEM, (int)DataMgr.Inst.PlayerEx.m_hostId);

            Item hitItem = inputTargetTr.parent.parent.GetComponent<Item>();
            if (hitItem != null)
            {
                hitItem.SendGainDropItem();
                //hitItem.SetClick();
                return true;
            }
        }
        return false;
    }



    public void onMouseRightClick()
    {
        Vector3 arrivePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //아이템 선택확인
        Ray2D ray = new Ray2D(arrivePos, Vector2.zero);
        //드래그 할때
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, 1 << LayerMask.NameToLayer(DT.eLayerType.Default.ToString()));

        if (hit.transform == CharacterMgr.Inst.myPlayer.transform || hit.transform == null)
        {
            return;
        }

        if (DataMgr.Inst.isPKMode == false)      // PK Mode 가 아니고 다른플레이어 일경우 플레이어 메뉴를 띄워줘야한다
        {
            if(hit.transform.GetComponent<Player>() != null && !hit.transform.GetComponent<Player>().IsMine())
			{
                InGameVariable.Inst.panelOnPlayer.Init(hit.transform.GetComponent<Player>());
                return;
            }
        }

        inputTargetTr = hit.transform;
        ProcesseInputTargetInRange(CharacterMgr.Inst.myPlayer.transform, hit.transform, false);
        //SetSkillToTarget( hit, false);
    }
    bool SetSkillToTarget( Transform targetTr, bool isLeftClick)
    {
        Character target = targetTr.GetComponent<Character>();

        int skillid = isLeftClick ? PanelSkill.Inst.selectedLeftId : PanelSkill.Inst.selectedRightId;
        bool isEnableSkill = false;
        if (skillid != 0)
        {
            isEnableSkill = SkillMgr.Inst.IsEnableSkill(skillid, target);
        }
        //스킬을 선택한 상태고 죽지 않았다면
        if (isEnableSkill)
        {

			inputTargetTr = targetTr.transform;

			NetMgr.Inst.reqFireSkill(DataMgr.Inst.zoneNum, skillid, target.id);
            return true;

        }
        return false;
    }


    public DT.eTagType GetNPCTagType(NPC target)
    {
        NDT.eNpcType npcType = (NDT.eNpcType)(Tables.data_npc.Get(target.m_npc.m_dataNPlayer.m_iIndex).npc_type);


        switch (npcType)
        {
            case NDT.eNpcType.Merchant:
            case NDT.eNpcType.BankClerk:
            case NDT.eNpcType.Blacksmith:
            case NDT.eNpcType.Quest:
            case NDT.eNpcType.Police:
            case NDT.eNpcType.Simmani:
            case NDT.eNpcType.ChangeGender:
            case NDT.eNpcType.ResetStats:
            case NDT.eNpcType.Rebirth:
            case NDT.eNpcType.Incinerator:
            case NDT.eNpcType.SkillLearn:
            //case NDT.eNpcType.MonsterBegin:
                return DT.eTagType.NPC;
            case NDT.eNpcType.MonsterNormalBegin:
            case NDT.eNpcType.MonsterNormal01:
            case NDT.eNpcType.MonsterNormalEnd:
            case NDT.eNpcType.MonsterBossBegin:
            case NDT.eNpcType.MonsterBoss01:
            case NDT.eNpcType.MonsterBoss02:
            case NDT.eNpcType.MonsterBossEnd:
                return DT.eTagType.MONSTER;

        }
        return DT.eTagType.NPC;

    }

    void MeetNpc(NPC target)
    {
        if (target != null /*&& target.tag == DT.eTagType.NPC.ToString()*/)
        {
            //CharacterMgr.Inst.npcs[]
            int npcIndex = target.IsQuestNPC();
            if (npcIndex != 0)     // 퀘스트를 갖는 NPC인지 확인
            {
                PanelQuestNew.Inst.OnClickNPC(target);
                //if (target.m_npc.m_QuestMarkType == DT.eNPC_Mark.HaveQuest)
                //{
                //    //if (npcIndex == 2438) InGameVariable.Inst.panelQuest.PopupAccepteDailyQuest();
                //    //else InGameVariable.Inst.panelQuest.SendAcceptMainQuest(npcIndex);
                //}
                //else if (target.m_npc.m_QuestMarkType == DT.eNPC_Mark.DoQuest)
                //{
                //    //if (npcIndex == 2438) InGameVariable.Inst.panelQuest.SendDailyQuestComplete();
                //    //else InGameVariable.Inst.panelQuest.SendCompleteMainQuest(npcIndex);
                //}
                //return;
            }
            if(target.m_npc.m_dataNPlayer.m_iIndex == 292 ||        // 대자보
               target.m_npc.m_dataNPlayer.m_iIndex == 1252 || 
               target.m_npc.m_dataNPlayer.m_iIndex == 1305)
            {
                if(PanelPoster.Inst == null)
                    PanelPoster.Inst.SetInstance();
                PanelPoster.Inst.infotype = 0;
                PanelPoster.Inst.onActive();
            }
        }

        NDT.eNpcType npcType = (NDT.eNpcType)(Tables.data_npc.Get(target.m_npc.m_dataNPlayer.m_iIndex).npc_type);

        GameObject openMenu = new GameObject();
        Item item = null;
        switch (npcType)
        {
            
            case 0:
                break;
            case NDT.eNpcType.Merchant: // 상인 (구매, 판매)
                int shopType = Tables.data_npc.Get(target.m_npc.m_dataNPlayer.m_iIndex).shop_type;

                if (shopType != 0 && shopType < 3)
                {
                    openMenu = GameObject.FindGameObjectWithTag("Canvas").transform.Find("storeMenu").gameObject;
                    Button bt = openMenu.transform.Find("Button Buy").transform.GetComponent<Button>();
                    bt.onClick.RemoveAllListeners();

                    bt.onClick.AddListener(delegate { InGameVariable.Inst.panelStore.onClickOpenStore(shopType); });
                    openMenu.SetActive(true);
                    return;
                }
                break;
            case NDT.eNpcType.BankClerk: // 은행 (입금, 출금, 창고)
                openMenu = GameObject.FindGameObjectWithTag("Canvas").transform.Find("bankMenu").gameObject;
                openMenu.SetActive(true);
                break;
            case NDT.eNpcType.Blacksmith: // 대장장이 (강화, 제조, 수리)
                openMenu = GameObject.FindGameObjectWithTag("Canvas").transform.Find("blacksmithMenu").gameObject;
                openMenu.SetActive(true);
                break;
            case NDT.eNpcType.ResetStats: // 스텟 초기화
                item = ItemMgr.Inst.FindItemIn_byTableIndex(ItemMgr.Inst.inventoryItems, 744); // 인벤토리에서 망각의 샘물 검색함
                if (item != null)
                {
                    ConfirmPopup.inst.Init("알림", "망각의 샘물을 사용하시겠습니까?", ConfirmPopup.Type.YesOrNo, 
                        delegate { NetMgr.Inst.reqUseItem(CharacterMgr.Inst.myPlayer.m_dataPlayer.m_iZone, item.dataItem.m_iUid); });
                }
                else ConfirmPopup.inst.Init("알림", "망각의 샘물을 보유하고 있지 않습니다.", ConfirmPopup.Type.Ok);
                break;
            case NDT.eNpcType.ChangeGender: // 성별 변경
                item = ItemMgr.Inst.FindItemIn_byTableIndex(ItemMgr.Inst.inventoryItems, 277);
                if (item != null)
                {
                    ConfirmPopup.inst.Init("알림", "성별을 변경하시겠습니까?", ConfirmPopup.Type.YesOrNo,
                        delegate { NetMgr.Inst.reqUseItem(CharacterMgr.Inst.myPlayer.m_dataPlayer.m_iZone, item.dataItem.m_iUid); });
                }
                else ConfirmPopup.inst.Init("알림", "성별 변경을 보유하고 있지 않습니다.", ConfirmPopup.Type.Ok);
               
                break;

            default:
                break;
        }
    }
    public void InputTargetNull()
    {
        inputTargetTr = null;
    }
    public void onMouseLeftClick()
    {
        Vector3 arrivePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        InGameVariable.Inst.panelOnPlayer.gameObject.SetActive(false);

        //맵이동처리, 어디로 가는 워프존인지 처리해야함.
        //if (LandMgr.Inst.IsWarpZone(arrivePos) == true)
        //{

        //    int block2 = LandMgr.Inst.GetBlockNumber(arrivePos);
        //    NetMgr.Inst.reqJoinZone(DataMgr.Inst.zoneNum, block2 + 1);
        //    return;
        //}

        //클릭한 지점이 나의 위치라면 리턴
        int block1 = LandMgr.Inst.GetBlockNumber(arrivePos);
        int block2 = LandMgr.Inst.GetBlockNumber(CharacterMgr.Inst.myPlayer.position);
        if(block1 == block2)
        {
            return;
        }

        //아이템 선택확인
        Ray2D ray = new Ray2D(arrivePos, Vector2.zero);
        //드래그 할때
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, 1 << LayerMask.NameToLayer(DT.eLayerType.Default.ToString()));
        bool isAction = false;
        if (hit.transform != null)
        {
            inputTargetTr = hit.transform;
            arrivePos = hit.transform.position; //대상을 히트하면 대상의 중앙위치를 도착점으로 변경

            NPC target = inputTargetTr.GetComponent<NPC>();

            if (target != null && GetNPCTagType( target) == DT.eTagType.NPC)
            {
                MeetNpc(target);
            }
            else
            {
                isAction =  ProcesseInputTargetInRange(CharacterMgr.Inst.myPlayer.transform, hit.transform, true);
            }
        }
        else
        {
            InputTargetNull();
        }

        //바닥이면 이동처리
        arrivePos.z = 0;
        //bool isTestMove = false;
        //foreach(var a in CharacterMgr.Inst.players.Values)
        //{
        //    if (a.isMoveTest)
        //    {
        //        isTestMove = true;
        //        a.Send_Move(arrivePos);
        //    }
        //}
        //for (int i = 0; i < CharacterMgr.Inst.players.Count; i++)
        //{
        //    if (CharacterMgr.Inst.players[i].isMoveTest)
        //    {
        //        isTestMove = true;
        //        CharacterMgr.Inst.players[i].Send_Move(arrivePos);
        //    }
        //}
        //if (isTestMove == false)
        //{
#if UNITY_EDITOR
        //if(Input.GetKeyDown(KeyCode.LeftAlt))
        if(Input.GetKey(KeyCode.LeftAlt))
        {
            NetMgr.Inst.reqCheat_MoveBlock(CharacterMgr.Inst.myPlayer.m_dataPlayer.m_iZone, /*CharacterMgr.Inst.myPlayer.currentBlock*/block1);
            return;
        }
#endif

        if(isAction == false)
        {
            CharacterMgr.Inst.myPlayer.Send_Move(arrivePos);
        }
        //}

    }

    /// <summary>
    /// 스크린샷 찍는 함수. 에디터나 WIN 이외 플랫폼에선 다르게 사용해야함
    /// </summary>
    public void Screenshot()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ScreenCapture.CaptureScreenshot("ScreenShot" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
        Debug.Log("스크린샷저장");
#endif
    }

    public void OpenAndCloseUI(string str)
    {

    }

    //스킬사용할수있으면 사용
    public void UseSkillIfUseable(KeyCode pushKeyCode)
    {
        Vector3 arrivePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Ray2D ray = new Ray2D(arrivePos, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, 1 << LayerMask.NameToLayer(DT.eLayerType.Default.ToString()));

        Character character = hit.transform?.GetComponent<Character>();

        foreach (var val in PanelSkill.Inst.panelSkill_Items)
        {
            if (pushKeyCode == val.hotkey)
            {
				int hostId = 0;

				// 나에게만 시전 하는 스킬
				if (val.Data_skilltable.target_type == (int)NDT.eSkillTargetType.AllyMy)
				{
					hostId = CharacterMgr.Inst.myPlayer.id;
				}
				// 타겟이 없으면 나에게 시전하는 스킬
				else if(val.Data_skilltable.target_type == (int)NDT.eSkillTargetType.AllyOther
					|| val.Data_skilltable.target_type == (int)NDT.eSkillTargetType.AllyAll
					|| val.Data_skilltable.target_type == (int)NDT.eSkillTargetType.AllPlayer)
				{
					if (character == null)
					{
						hostId = CharacterMgr.Inst.myPlayer.id;
					}
					else
						hostId = character.id;
				}
				// 타겟이 꼭 필요한 스킬
				else
				{
					//if (character == null)
					//	return;
					//hostId = character.id;

					return;
				}

				NetMgr.Inst.reqFireSkill(DataMgr.Inst.zoneNum, val.tableIndex, hostId);
                break;
            }
        }

    }

    public void onClickZoneListButton()
    {
        InGameVariable.Inst.panelZonePlayerList.ClearAllPlayer();
        InGameVariable.Inst.panelZonePlayerList.UpdateAllPlayer();
        if(!InGameVariable.Inst.panelZonePlayerList.isActiveAndEnabled)
            InGameVariable.Inst.panelZonePlayerList.gameObject.SetActive(true);
        else
            InGameVariable.Inst.panelZonePlayerList.gameObject.SetActive(false);

    }

    /// <summary>
    /// 창화면 전체화면 바꾸는 함수
    /// </summary>
    private void setScreenMode()
    {
        int screenMode;
        screenMode = (int)Screen.fullScreenMode;
        if (screenMode == 0) screenMode = 3;
        else screenMode = 0;
        PlayerPrefs.SetInt("ScreenMode", screenMode);

        Screen.SetResolution(Screen.width, Screen.height, (FullScreenMode)screenMode);
    }
}
