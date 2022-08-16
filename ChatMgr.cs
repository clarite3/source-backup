using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class ChatMgr : Singleton<ChatMgr>
{
	// req(request)		��û : Ŭ�� -> ����
	
    /// <summary>
    /// ������ RMI �޽��� ������ ���� C2S proxy ��ü ����.
    /// </summary>
    ChatC2S.Proxy proxyChat = new ChatC2S.Proxy();
	public RmiProxy Proxy { get { return proxyChat; } }
	/// <summary>
	/// ������ ���� RMI �޽����� �ޱ� ���� S2C Stub ��ü ����.
	/// </summary>
	ChatS2C.Stub stubChat = new ChatS2C.Stub();
	public RmiStub Stub { get { return stubChat; } }


	// Start is called before the first frame update
	IEnumerator Start()
    {
		while (true)
		{
			if (proxyChat == null)
				proxyChat = new ChatC2S.Proxy();
			if (stubChat == null)
				stubChat = new ChatS2C.Stub();

			if (proxyChat != null && stubChat != null)
				break;

			Debug.LogError("ChatMgr start ����");
			yield return new WaitForEndOfFrame();
		}

		// s2cStub RMI �Լ� ��������Ʈ�� �� �Լ� ����.
		stubChat.NormalChat = bctNormalChatting;
		stubChat.SingleChat = ackSingleChatting;
		stubChat.PartyChat = bctPartyChatting;
		stubChat.GuildChat = bctGuildChatting;
		stubChat.SystemNotice = bctSystemNotice;
		//stubChat.REC_PlayerInfo = askPlayerInfo;

		isCompleteLoading = true;
    }

	///////////////////////////////////////////////////////
	/// ����
	public void reqNormalChatting(string txt)
    {
		proxyChat.NormalChat(HostID.HostID_Server, RmiContext.ReliableSend, txt);
	}
	public void reqSingleChatting(string targetNick, string txt)
	{
		proxyChat.SingleChat(HostID.HostID_Server, RmiContext.ReliableSend, targetNick, txt);
    }
    public void reqPartyChatting(string txt)
    {
        proxyChat.PartyChat(HostID.HostID_Server, RmiContext.ReliableSend, txt);
    }
    public void reqGuildChatting(string txt)
    {
        proxyChat.GuildChat(HostID.HostID_Server, RmiContext.ReliableSend, DataMgr.Inst.dataGuild.m_iUid, txt);
    }
	///////////////////////////////////////////////////////
	// ����
	// ack(ack)			���� : ���� -> Ŭ��
	// bct(broadcast)	��� : ���� -> Ŭ��
	/// <summary>
	/// ���: �Ϲ� ä��
	/// </summary>
	bool bctNormalChatting(HostID remote, RmiContext rmiContext, byte chatType, string nick, string text)
    {

		string data;

		data = nick + "/" + text;

		if ((NDT.eNormalChatType)chatType == NDT.eNormalChatType.Normal)
        {
			
			MsgDisp.SendMsg(DT.eMsg.Net_NormalChatting, data);
		}
		else
        {
			MsgDisp.SendMsg(DT.eMsg.Net_Megaphone, data);
        }

		//Debug.Log("[�Ϲ�]" + nick + ": " + text);
		return true;
    }
	/// <summary>
	/// ����: �ӼӸ�
	/// </summary>
	bool ackSingleChatting(HostID remote, RmiContext rmiContext, string nick, string text)
	{
		string data;

		data = nick + "/" + text;
		MsgDisp.SendMsg(DT.eMsg.Net_SingleChatting, data);

		//Debug.Log("[�ӼӸ�]" + nick + ": " + text);
		return true;
	}
	/// <summary>
	/// ���: ��Ƽ ä��
	/// </summary>
	bool bctPartyChatting(Nettention.Proud.HostID remote, Nettention.Proud.RmiContext rmiContext, Nettention.Proud.HostID hid, string text)
    {

		string data = hid.ToString() + "/" + text;
		MsgDisp.SendMsg(DT.eMsg.Net_PartyChatting, data);
		//Debug.Log("[��Ƽ]" + nick + ": " + text);
		return true;
	}
	/// <summary>
	/// ���: ��� ä��
	/// </summary>
	bool bctGuildChatting(Nettention.Proud.HostID remote, Nettention.Proud.RmiContext rmiContext, Nettention.Proud.HostID hid, string text)
	{

		string data = hid.ToString() + "/" + text;
		MsgDisp.SendMsg(DT.eMsg.Net_GuildChatting, data);
		//Debug.Log("[���]" + nick + ": " + text);
		return true;
	}
	/// <summary>
	/// ���: ����
	/// </summary>
	bool bctSystemNotice(HostID remote, RmiContext rmiContext,  string text)
	{
		MsgDisp.SendMsg(DT.eMsg.Net_SystemChatting, text);
		//Debug.Log("[����]: " + text);
		return true;
	}
}
