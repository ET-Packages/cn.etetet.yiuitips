using System;
using System.Collections.Generic;
using YIUIFramework;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(TipsViewComponent))]
    [FriendOf(typeof(TipsViewComponent))]
    public static partial class TipsViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TipsViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask YIUIWindowClose(this TipsViewComponent self, bool viewCloseResult)
        {
            if (viewCloseResult)
            {
                await self.DynamicEvent(new EventPutTipsView() { View = self?.GetParent<YIUIWindowComponent>()?.OwnerUIEntity });
            }
            else
            {
                Log.Info($"View {self?.GetParent<YIUIWindowComponent>()?.UIBase?.OwnerGameObject.name} 被关闭，但其父级未关闭 所以不触发回收Tips 请注意");
            }
        }
    }
}