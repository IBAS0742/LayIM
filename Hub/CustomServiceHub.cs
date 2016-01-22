﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using LayIM.BLL;
using LayIM.Model;

namespace LayIM
{
    [HubName("csHub")]
    public class CustomServiceHub : Hub
    {
        /// <summary>
        /// 当前连接ID
        /// </summary>
        private string CurrentUserConnectionId
        {
            get
            {
                return Context.ConnectionId;
            }
        }
        public Task Join()
        {
            return Clients.All.receiveMessage("某某人加入了");
        }

        /// <summary>
        /// 人对人聊天 连接服务器
        /// </summary>
        /// <param name="sendid">发送人</param>
        /// <param name="receiveid">接收人</param>
        /// <returns></returns>
        public Task ClientToClient(string sendid, string receiveid)
        {
            if (sendid == null || receiveid == null) { throw new ArgumentNullException("sendid or receiveid can't be null"); }
            //获取组名
            string groupName = MessageUtils.GetGroupName(sendid, receiveid);
            //将当前用户添加到此组织内
            Groups.Add(CurrentUserConnectionId, groupName);
            //构建系统连接成功消息
            var msg = MessageUtils.GetSystemMessage(groupName, MessageConfig.ClientToClientConnectedSucceed, new { currentid = sendid, receiveid = receiveid });
            //将消息推送到当前组 （A和B聊天的组） 同样调用receiveMessage方法
            return Clients.Caller.receiveMessage(msg);
        }

        /// <summary>
        /// 发送消息 ，服务器接收的是CSChatMessage实体，他包含发送人，接收人，消息内容等信息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Task ClientSendMsgToClient(CSChatMessage msg)
        {
            var groupName = MessageUtils.GetGroupName(msg.fromuser.userid.ToString(), msg.touser.userid.ToString());
            /*
            中间处理一下消息直接转发给（A,B所在组织，即聊天窗口）
            */
            msg.msgtype = CSMessageType.Custom;//消息类型为普通消息
            return Clients.Group(groupName).receiveMessage(msg);
        }
    }
}