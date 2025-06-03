using System;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 弹窗助手 不知道泛型 只知道UI资源名称时使用
    /// 缺点这里没办法判断类型是否正确 是否有 IYIUIOpen〈ParamVo〉 接口
    /// </summary>
    public static partial class TipsHelper
    {
        //在Tips界面打开任意一个View窗口
        public static async ETTask Open(string resName, params object[] paramMore)
        {
            var vo = ParamVo.Get(paramMore);
            await OpenToParent(resName, vo);
            ParamVo.Put(vo);
        }

        //扩展同步方法
        public static void OpenSync(string resName, params object[] paramMore)
        {
            Open(resName, paramMore).NoContext();
        }

        //在Tips界面打开任意一个View窗口
        public static async ETTask OpenToParent(string resName, Entity parent, params object[] paramMore)
        {
            var vo = ParamVo.Get(paramMore);
            await OpenToParent(resName, vo, parent);
            ParamVo.Put(vo);
        }

        //扩展同步方法
        public static void OpenToParentSync(string resName, Entity parent, params object[] paramMore)
        {
            OpenToParent(resName, parent, paramMore).NoContext();
        }

        //使用paramvo参数打开
        public static async ETTask OpenToParent(string resName, ParamVo vo, Entity parent = null)
        {
            var data = YIUIBindHelper.GetBindVoByResName(resName);
            if (data == null) return;
            var bindVo = data.Value;
            EntityRef<Entity> parentRef = parent;
            using var coroutineLock = await YIUIMgrComponent.Inst.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.YIUIFramework, typeof(TipsHelper).GetHashCode());
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<TipsPanelComponent, Type, Entity, ParamVo>(bindVo.ComponentType, parentRef.Entity, vo);
        }

        //使用paramvo参数 同步打开 内部还是异步 为了解决vo被回收问题
        public static void OpenToParentSync(string resName, ParamVo vo, Entity parent = null)
        {
            OpenToParent2NewVo(resName, vo, parent).NoContext();
        }

        //在外部vo会被回收 所以不能使用同对象 所以这里会创建一个新的防止空对象
        private static async ETTask OpenToParent2NewVo(string resName, ParamVo vo, Entity parent = null)
        {
            var data = YIUIBindHelper.GetBindVoByResName(resName);
            if (data == null) return;
            var bindVo = data.Value;
            EntityRef<Entity> parentRef = parent;
            using var coroutineLock = await YIUIMgrComponent.Inst.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.YIUIFramework, typeof(TipsHelper).GetHashCode());
            var newVo = ParamVo.Get(vo.Data);
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<TipsPanelComponent, Type, Entity, ParamVo>(bindVo.ComponentType, parentRef.Entity, newVo);
            ParamVo.Put(newVo);
        }
    }
}