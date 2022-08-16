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
    /// ���� �Լ� �����ϴ� Ŭ����
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
/// �ൿ ������ �̳�
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
/// ������ Ű �� ������ ��ųʸ�
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
    /// Ű������ Ű�ڵ� ����
    /// </summary>
    KeyCode[] defaultKeys = new KeyCode[]
    {KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5,
    KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10,
    KeyCode.Insert, KeyCode.Home, KeyCode.PageUp, KeyCode.Delete,
    KeyCode.End, KeyCode.PageDown, KeyCode.SysReq, KeyCode.Escape,
    KeyCode.M };

    eActions pressedKey = eActions.Max;

    /// <summary>
    /// �� Ű�� �°� Ŀ�ǵ� Ŭ���� ����
    /// </summary>

    Command F1, F2, F3, F4, F5, F6, F7, F8, F9, F10,
        Del, End, PgDown, Ins, Home, PgUp, Prt, Esc,
        M;

    Actor actor;

    [SerializeField]private Sprite[] Cursors; // ��������Ʈ ����
    private List<Texture2D> cursorTexture; // �ؽ���2D�� ��ȯ�� Ŀ��
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
    /// Sprite -> Texture2D ��ȯ���ִ� �Լ�
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
    /// Ŀ�ǵ� Ŭ���� �ʱ�ȭ
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
    /// PlayerPrefs�� �����س��� Ű���� �ε�
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
        // Ű �� �޾ƿͼ� �� �ൿ ����
        Command command = GetCommand();
        if (command != null)
        {
            command.Execute(actor);
        }

        UpdateMouseCursor();

        //�� ������ �׽�Ʈ�� ����. ���� ¥����
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == DT.eSceneType.InGame.ToString())
        {
            UpdateTargetPosition();
        }
    }
    /// <summary>
    /// ���콺 Ŀ��
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
    /// Ű �Է����� �� �����س��� ���� ���� Ŀ�ǵ� Ŭ������ ��ȯ���ִ� �Լ�
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
    /// ���� Ű �Է� �޾Ƽ� �̳ѿ� �� ����
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

    public ItemDropZone pointerEnter_ItemDropZone; //����� ��� ����������Ʈ�� ��ġ���ִ���
    public void UpdateTargetPosition()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
            if (Input.GetMouseButtonDown(0))
        {

            isDown = true;
        }

        //������ Ȯ��
      
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDown == false) return;
            isDown = false;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                onMouseLeftClick();
            }
        }
        else if(Input.GetMouseButtonUp(1))  // ���콺 ��Ŭ��
        {
            //if (isDown == false) return;
            //isDown = false;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 arrivePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //������ ����Ȯ��
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
    //������ ��� �����Ÿ��ȿ� ������ ȣ���Ͽ� ó��
    public bool ProcesseInputTargetInRange(Transform mine, Transform target, bool isLeftClick)
    {        
        Player p = CharacterMgr.Inst.myPlayer;
        DT.eTagType targetTag = UtilFunc.ParseEnum<DT.eTagType>(target.tag);

        bool useSkill = SetSkillToTarget(target, isLeftClick);
        //��ų�� ����Ѵٸ� return
        if (useSkill == true)
        {
            return true;
        }
        //�ڽ��̶�� return (�ڽſ��� ���� ��ų�� �����������Ƿ� ��ų��� �ؿ� �־��Ѵ�.
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
				// ������ ���� ���Ѵ�
				return true;
			}


			bool isEnableAttack = false;
            /// ����
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
                //Debug.Log("���� myTile X: " + myTile.x + " Y: " + myTile.y + " target X: " + targetTile.x + " Y: "+ targetTile.y);

				// ��Ÿ�� ������ ���� ����
				if(CharacterMgr.Inst.myPlayer.bGetOn == true)
				{
					return true;
				}

                //NetMgr.Inst.reqAttack(DataMgr.Inst.zoneNum, (int)_target.id);
                CharacterMgr.Inst.myPlayer.SetAttack(_target);
                return true;
            }


        }
       // else if(targetTag == DT.eTagType.NPC)   // NPC Ŭ���� �޴��� �������
       // {
       //     // NPC ��ɸ��� ������ϴ� �޴��� ������ ����
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

        //������ ����Ȯ��
        Ray2D ray = new Ray2D(arrivePos, Vector2.zero);
        //�巡�� �Ҷ�
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, 1 << LayerMask.NameToLayer(DT.eLayerType.Default.ToString()));

        if (hit.transform == CharacterMgr.Inst.myPlayer.transform || hit.transform == null)
        {
            return;
        }

        if (DataMgr.Inst.isPKMode == false)      // PK Mode �� �ƴϰ� �ٸ��÷��̾� �ϰ�� �÷��̾� �޴��� �������Ѵ�
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
        //��ų�� ������ ���°� ���� �ʾҴٸ�
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
            if (npcIndex != 0)     // ����Ʈ�� ���� NPC���� Ȯ��
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
            if(target.m_npc.m_dataNPlayer.m_iIndex == 292 ||        // ���ں�
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
            case NDT.eNpcType.Merchant: // ���� (����, �Ǹ�)
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
            case NDT.eNpcType.BankClerk: // ���� (�Ա�, ���, â��)
                openMenu = GameObject.FindGameObjectWithTag("Canvas").transform.Find("bankMenu").gameObject;
                openMenu.SetActive(true);
                break;
            case NDT.eNpcType.Blacksmith: // �������� (��ȭ, ����, ����)
                openMenu = GameObject.FindGameObjectWithTag("Canvas").transform.Find("blacksmithMenu").gameObject;
                openMenu.SetActive(true);
                break;
            case NDT.eNpcType.ResetStats: // ���� �ʱ�ȭ
                item = ItemMgr.Inst.FindItemIn_byTableIndex(ItemMgr.Inst.inventoryItems, 744); // �κ��丮���� ������ ���� �˻���
                if (item != null)
                {
                    ConfirmPopup.inst.Init("�˸�", "������ ������ ����Ͻðڽ��ϱ�?", ConfirmPopup.Type.YesOrNo, 
                        delegate { NetMgr.Inst.reqUseItem(CharacterMgr.Inst.myPlayer.m_dataPlayer.m_iZone, item.dataItem.m_iUid); });
                }
                else ConfirmPopup.inst.Init("�˸�", "������ ������ �����ϰ� ���� �ʽ��ϴ�.", ConfirmPopup.Type.Ok);
                break;
            case NDT.eNpcType.ChangeGender: // ���� ����
                item = ItemMgr.Inst.FindItemIn_byTableIndex(ItemMgr.Inst.inventoryItems, 277);
                if (item != null)
                {
                    ConfirmPopup.inst.Init("�˸�", "������ �����Ͻðڽ��ϱ�?", ConfirmPopup.Type.YesOrNo,
                        delegate { NetMgr.Inst.reqUseItem(CharacterMgr.Inst.myPlayer.m_dataPlayer.m_iZone, item.dataItem.m_iUid); });
                }
                else ConfirmPopup.inst.Init("�˸�", "���� ������ �����ϰ� ���� �ʽ��ϴ�.", ConfirmPopup.Type.Ok);
               
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

        //���̵�ó��, ���� ���� ���������� ó���ؾ���.
        //if (LandMgr.Inst.IsWarpZone(arrivePos) == true)
        //{

        //    int block2 = LandMgr.Inst.GetBlockNumber(arrivePos);
        //    NetMgr.Inst.reqJoinZone(DataMgr.Inst.zoneNum, block2 + 1);
        //    return;
        //}

        //Ŭ���� ������ ���� ��ġ��� ����
        int block1 = LandMgr.Inst.GetBlockNumber(arrivePos);
        int block2 = LandMgr.Inst.GetBlockNumber(CharacterMgr.Inst.myPlayer.position);
        if(block1 == block2)
        {
            return;
        }

        //������ ����Ȯ��
        Ray2D ray = new Ray2D(arrivePos, Vector2.zero);
        //�巡�� �Ҷ�
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, 1 << LayerMask.NameToLayer(DT.eLayerType.Default.ToString()));
        bool isAction = false;
        if (hit.transform != null)
        {
            inputTargetTr = hit.transform;
            arrivePos = hit.transform.position; //����� ��Ʈ�ϸ� ����� �߾���ġ�� ���������� ����

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

        //�ٴ��̸� �̵�ó��
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
    /// ��ũ���� ��� �Լ�. �����ͳ� WIN �̿� �÷������� �ٸ��� ����ؾ���
    /// </summary>
    public void Screenshot()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ScreenCapture.CaptureScreenshot("ScreenShot" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
        Debug.Log("��ũ��������");
#endif
    }

    public void OpenAndCloseUI(string str)
    {

    }

    //��ų����Ҽ������� ���
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

				// �����Ը� ���� �ϴ� ��ų
				if (val.Data_skilltable.target_type == (int)NDT.eSkillTargetType.AllyMy)
				{
					hostId = CharacterMgr.Inst.myPlayer.id;
				}
				// Ÿ���� ������ ������ �����ϴ� ��ų
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
				// Ÿ���� �� �ʿ��� ��ų
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
    /// âȭ�� ��üȭ�� �ٲٴ� �Լ�
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
