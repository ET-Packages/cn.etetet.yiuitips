﻿using System;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(TipsMessageViewComponent))]
    public static partial class TipsMessageViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this TipsMessageViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TipsMessageViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TipsMessageViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        [EntitySystem]
        private static async ETTask YIUIOpenTween(this TipsMessageViewComponent self)
        {
            await WindowFadeAnim.In(self.UIBase.OwnerGameObject);
        }

        [EntitySystem]
        private static async ETTask YIUICloseTween(this TipsMessageViewComponent self)
        {
            await WindowFadeAnim.Out(self.UIBase.OwnerGameObject);
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TipsMessageViewComponent self, ParamVo vo)
        {
            await ETTask.CompletedTask;
            var content = vo.Get<string>();
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError($"MessageTipsView 必须有消息内容 请检查");
                return false;
            }

            self.ExtraData = vo.Get(1, new MessageTipsExtraData());
            self.u_DataMessageContent.SetValue(content);
            self.u_DataShowCancel.SetValue(self.ExtraData.CancelCallBack != null);
            self.u_DataShowClose.SetValue(self.ExtraData.CloseCallBack != null);
            self.u_DataConfirmName.SetValue(string.IsNullOrEmpty(self.ExtraData.ConfirmName) ? "确定" : self.ExtraData.ConfirmName);
            self.u_DataCancelName.SetValue(string.IsNullOrEmpty(self.ExtraData.CancelName) ? "取消" : self.ExtraData.CancelName);
            return true;
        }

        #region YIUIEvent开始

        [YIUIInvoke(TipsMessageViewComponent.OnEventConfirmInvoke)]
        private static void OnEventConfirmInvoke(this TipsMessageViewComponent self)
        {
            self.ExtraData.ConfirmCallBack?.Invoke();
            self.UIView.Close();
        }

        [YIUIInvoke(TipsMessageViewComponent.OnEventCancelInvoke)]
        private static void OnEventCancelInvoke(this TipsMessageViewComponent self)
        {
            self.ExtraData.CancelCallBack?.Invoke();
            self.UIView.Close();
        }

        [YIUIInvoke(TipsMessageViewComponent.OnEventCloseInvoke)]
        private static void OnEventCloseInvoke(this TipsMessageViewComponent self)
        {
            self.ExtraData.CloseCallBack?.Invoke();
            self.UIView.Close();
        }

        #endregion YIUIEvent结束
    }
}
