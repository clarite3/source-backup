using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class ChatMgr : Singleton<ChatMgr>
{
	// req(request)		요청 : 클라 -> 서버
	
    /// <summary>
    /// 서버로 RMI 메시지 전송을 위해 C2S proxy 객체 선언.
    /// </summary>
    ChatC2S.Proxy proxyChat = new ChatC2S.Proxy();
	public RmiProxy Proxy { get { return proxyChat; } }
	/// <summary>
	/// 서버로 부터 RMI 메시지를 받기 위해 S2C Stub 객체 선언.
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

			Debug.LogError("ChatMgr start 실패");
			yield return new WaitForEndOfFrame();
		}

		// s2cStub RMI 함수 딜리게이트에 각 함수 셋팅.
		stubChat.NormalChat = bctNormalChatting;
		stubChat.SingleChat = ackSingleChatting;
		stubChat.PartyChat = bctPartyChatting;
		stubChat.GuildChat = bctGuildChatting;
		stubChat.SystemNotice = bctSystemNotice;
		//stubChat.REC_PlayerInfo = askPlayerInfo;

		isCompleteLoading = true;
    }

	///////////////////////////////////////////////////////
	/// 보냄
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
	// 받음
	// ack(ack)			응답 : 서버 -> 클라
	// bct(broadcast)	방송 : 서버 -> 클라
	/// <summary>
	/// 방송: 일반 채팅
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

		//Debug.Log("[일반]" + nick + ": " + text);
		return true;
    }
	/// <summary>
	/// 응답: 귓속말
	/// </summary>
	bool ackSingleChatting(HostID remote, RmiContext rmiContext, string nick, string text)
	{
		string data;

		data = nick + "/" + text;
		MsgDisp.SendMsg(DT.eMsg.Net_SingleChatting, data);

		//Debug.Log("[귓속말]" + nick + ": " + text);
		return true;
	}
	/// <summary>
	/// 방송: 파티 채팅
	/// </summary>
	bool bctPartyChatting(Nettention.Proud.HostID remote, Nettention.Proud.RmiContext rmiContext, Nettention.Proud.HostID hid, string text)
    {

		string data = hid.ToString() + "/" + text;
		MsgDisp.SendMsg(DT.eMsg.Net_PartyChatting, data);
		//Debug.Log("[파티]" + nick + ": " + text);
		return true;
	}
	/// <summary>
	/// 방송: 길드 채팅
	/// </summary>
	bool bctGuildChatting(Nettention.Proud.HostID remote, Nettention.Proud.RmiContext rmiContext, Nettention.Proud.HostID hid, string text)
	{

		string data = hid.ToString() + "/" + text;
		MsgDisp.SendMsg(DT.eMsg.Net_GuildChatting, data);
		//Debug.Log("[길드]" + nick + ": " + text);
		return true;
	}
	/// <summary>
	/// 방송: 공지
	/// </summary>
	bool bctSystemNotice(HostID remote, RmiContext rmiContext,  string text)
	{
		MsgDisp.SendMsg(DT.eMsg.Net_SystemChatting, text);
		//Debug.Log("[공지]: " + text);
		return true;
	}
}
