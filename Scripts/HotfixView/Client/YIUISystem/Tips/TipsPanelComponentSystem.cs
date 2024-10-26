using System;
using System.Collections.Generic;
using YIUIFramework;
using UnityEngine;
using UnityTime = UnityEngine.Time;

namespace ET.Client
{
    /// <summary>
    /// 公共弹窗界面
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/OdNgwu0KsiyJ6NkK8vCcwbjbn1g
    /// </summary>
    [FriendOf(typeof(TipsPanelComponent))]
    public static partial class TipsPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this TipsPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TipsPanelComponent self)
        {
            if (self._RefCount <= 0) return;
            var tempRefView = new List<EntityRef<Entity>>();
            tempRefView.AddRange(self._AllRefView);
            foreach (Entity view in tempRefView)
            {
                view?.Parent?.Dispose();
            }
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIClose(this TipsPanelComponent self)
        {
            if (self._RefCount <= 0) return true;
            var tempRefView = new List<EntityRef<Entity>>();
            tempRefView.AddRange(self._AllRefView);
            foreach (Entity view in tempRefView)
            {
                var viewComponent = view?.Parent?.GetComponent<YIUIViewComponent>();
                if (viewComponent != null)
                {
                    await viewComponent.CloseAsync(false);
                }
            }

            return true;
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TipsPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TipsPanelComponent self, Type viewType, Entity parent, ParamVo vo)
        {
            return await self.OpenTips(viewType, parent, vo);
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TipsPanelComponent self, Type viewType, Entity parent, long waitId, ParamVo vo)
        {
            return await self.OpenTips(viewType, parent, vo, waitId);
        }

        //消息 回收对象
        [EntitySystem]
        private static async ETTask DynamicEvent(this TipsPanelComponent self, EventPutTipsView message)
        {
            self.PutTips(message.View, message.Destroy);
            await ETTask.CompletedTask;
        }

        //对象池的实例化过程
        private static async ETTask<Entity> OnCreateViewRenderer(this TipsPanelComponent self, Type uiType, Entity parent, long waitId = 0)
        {
            var entity = await YIUIFactory.InstantiateAsync(uiType, parent ?? YIUIMgrComponent.Inst.Root, self.UIBase.OwnerRectTransform);
            if (entity != null)
            {
                var windowComponent = entity.GetParent<YIUIChild>()?.GetComponent<YIUIWindowComponent>();
                if (windowComponent == null)
                {
                    Log.Error($"{uiType.Name} 实例化的对象非 YIUIWindowComponent");
                }
                else
                {
                    windowComponent.AddComponent<TipsViewComponent>();
                    if (waitId != 0)
                    {
                        windowComponent.AddComponent<YIUIWaitComponent, long>(waitId);
                    }
                }
            }

            return entity;
        }

        //打开Tips对应的View
        [EnableAccessEntiyChild]
        private static async ETTask<bool> OpenTips(this TipsPanelComponent self, Type uiType, Entity parent, ParamVo vo, long waitId = 0)
        {
            if (!self._AllPool.ContainsKey(uiType))
            {
                async ETTask<EntityRef<Entity>> Create()
                {
                    return await self.OnCreateViewRenderer(uiType, parent, waitId);
                }

                self._AllPool.Add(uiType, new ObjAsyncCache<EntityRef<Entity>>(Create));
            }

            self.CheckAllPoolTips();
            var pool = self._AllPool[uiType];
            self._RefCount += 1; //加载前引用计数+1 防止加载过程中有人关闭 出现问题
            Entity view = await pool.Get();
            if (view == null)
            {
                self._RefCount -= 1;
                return self._RefCount > 0;
            }

            if (view is not IYIUIOpen<ParamVo>)
            {
                Debug.LogError($"{uiType.Name} 必须实现 IYIUIOpen<ParamVo> 才可用Tips");
                self._RefCount -= 1;
                return self._RefCount > 0;
            }

            var uiComponent = view.GetParent<YIUIChild>();
            if (uiComponent == null)
            {
                Debug.LogError($"{uiType.Name} 实例化的对象非 YIUIChild");
                self._RefCount -= 1;
                return self._RefCount > 0;
            }

            var windowComponent = uiComponent.GetComponent<YIUIWindowComponent>();
            if (windowComponent != null)
            {
                windowComponent.GetComponent<TipsViewComponent>()?.Reset();
                windowComponent.GetComponent<YIUIWaitComponent>()?.Reset(waitId);
            }

            var viewComponent = uiComponent.GetComponent<YIUIViewComponent>();
            if (viewComponent == null)
            {
                Debug.LogError($"{uiType.Name} 实例化的对象非 YIUIViewComponent");
                self._RefCount -= 1;
                return self._RefCount > 0;
            }

            uiComponent.OwnerRectTransform.SetAsLastSibling();

            uiComponent.SetParent(parent);

            self._AllRefView.Add(view);

            var result = await viewComponent.Open(vo);
            if (!result)
            {
                await viewComponent.CloseAsync(false);
            }

            return self._RefCount > 0;
        }

        //回收
        private static void PutTips(this TipsPanelComponent self, Entity view, bool destroy = false)
        {
            if (view == null)
            {
                Debug.LogError($"null对象 请检查");
                return;
            }

            if (!self._AllRefView.Remove(view))
            {
                //如果你确定这个东西是用tips打开的 这里又报错 大概率就是你在open过程中 回收了这个对象 导致的 说明你写法有问题
                //然后因为打开失败又会调用一次 所以重复回收也会提示 如果你open返回fase 可以不用手动回收 fase会自动回收
                Debug.LogError($"{view.GetType().Name} 无法回收一个不存在的对象 必须是之前打开过的对象 否则引用计数会混乱 请检查");
                return;
            }

            if (!destroy)
            {
                var uiType = view.GetType();
                if (!self._AllPool.ContainsKey(uiType))
                {
                    Log.Error($"TipsPanelComponent 没有找到 {uiType.Name} 的缓存池");
                    return;
                }

                self._AllPoolLastTime[uiType] = UnityTime.time;
                var pool = self._AllPool[uiType];
                pool.Put(view);
            }

            self._RefCount -= 1;
            self.CheckRefCount();
        }

        //检查引用计数 如果<=0 就自动关闭UI
        private static void CheckRefCount(this TipsPanelComponent self)
        {
            if (self._RefCount > 0) return;

            self._RefCount = 0;
            self.UIPanel.Close();
        }

        //有可能存在永远有tips被打开的情况
        //举个例子 在整个游戏过程中 可能会打开各种 A B C 这种大型的tips 使用频率很低
        //但是飘字 一些提示的频率很高  假设这个频率正好全覆盖 让TIPS永远没有真的被摧毁
        //那么就会造成那些大型Tips 一直没有被关闭又没人使用的情况 而没有被回收
        //所以这里需要做一些优化 让TIPS在一定时间内自动关闭 或者 让TIPS的打开频率降低
        //优化方式 将会给每个类型增加一个倒计时 如果超过一定时间没有打开 那么就回收相关的所有
        private static void CheckAllPoolTips(this TipsPanelComponent self)
        {
            var time = UnityTime.time;
            foreach (var uiType in self._AllPoolLastTime.Keys)
            {
                if (time - self._AllPoolLastTime[uiType] > self._AutoDestroyTime)
                {
                    if (self._AllPool.TryGetValue(uiType, out var objCache))
                    {
                        objCache.Clear((obj) => { ((Entity)obj)?.Parent?.Dispose(); });
                    }

                    //不需要很频繁的清理 一次只清理一个就行
                    //如果想高频率的自己用update 或者 倒计时回调之类的处理 自己实现吧
                    self._AllPoolLastTime.Remove(uiType);
                    break;
                }
            }
        }

        #region YIUIEvent开始

        #endregion YIUIEvent结束
    }
}