using System;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 弹窗助手
    /// 可打开任意View 建议命名Tips[XXX]View
    /// T 必须是View 且必须有 IYIUIOpen〈ParamVo〉 接口
    /// View关闭发送回收消息 EventPutTipsView
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/OdNgwu0KsiyJ6NkK8vCcwbjbn1g
    /// </summary>
    public static partial class TipsHelper
    {
        //在Tips界面打开任意一个View窗口
        public static async ETTask Open<T>(params object[] paramMore)
                where T : Entity
        {
            var vo = ParamVo.Get(paramMore);
            await OpenToParent<T>(vo);
            ParamVo.Put(vo);
        }

        //扩展同步方法
        public static void OpenSync<T>(params object[] paramMore)
                where T : Entity
        {
            Open<T>(paramMore).NoContext();
        }

        //在Tips界面打开任意一个View窗口
        public static async ETTask OpenToParent<T>(Entity parent, params object[] paramMore)
                where T : Entity
        {
            var vo = ParamVo.Get(paramMore);
            await OpenToParent<T>(vo, parent);
            ParamVo.Put(vo);
        }

        //扩展同步方法
        public static void OpenToParentSync<T>(Entity parent, params object[] paramMore)
                where T : Entity
        {
            OpenToParent<T>(parent, paramMore).NoContext();
        }

        //使用paramvo参数打开
        public static async ETTask OpenToParent<T>(ParamVo vo, Entity parent = null)
                where T : Entity
        {
            using var coroutineLock = await YIUIMgrComponent.Inst.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.YIUIFramework, typeof(TipsHelper).GetHashCode());

            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<TipsPanelComponent, Type, Entity, ParamVo>(typeof(T), parent, vo);
        }

        //使用paramvo参数 同步打开 内部还是异步 为了解决vo被回收问题
        public static void OpenToParentSync<T>(ParamVo vo, Entity parent = null)
                where T : Entity
        {
            OpenToParent2NewVo<T>(vo, parent).NoContext();
        }

        //在外部vo会被回收 所以不能使用同对象 所以这里会创建一个新的防止空对象
        private static async ETTask OpenToParent2NewVo<T>(ParamVo vo, Entity parent = null)
                where T : Entity
        {
            using var coroutineLock = await YIUIMgrComponent.Inst.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.YIUIFramework, typeof(TipsHelper).GetHashCode());

            var newVo = ParamVo.Get(vo.Data);
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<TipsPanelComponent, Type, Entity, ParamVo>(typeof(T), parent, newVo);
            ParamVo.Put(newVo);
        }
    }
}