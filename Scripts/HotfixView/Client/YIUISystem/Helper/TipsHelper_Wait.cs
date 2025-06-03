using System;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 可等待的弹窗
    /// </summary>
    public static partial class TipsHelper
    {
        public static async ETTask<HashWaitError> OpenWait<T>(params object[] paramMore) where T : Entity, IYIUIBind, IYIUIOpen<ParamVo>
        {
            var vo = ParamVo.Get(paramMore);
            var error = await OpenWaitToParent<T>(vo);
            ParamVo.Put(vo);
            return error;
        }

        public static async ETTask<HashWaitError> OpenWaitToParent<T>(Entity parent, params object[] paramMore) where T : Entity, IYIUIBind, IYIUIOpen<ParamVo>
        {
            var vo = ParamVo.Get(paramMore);
            var error = await OpenWaitToParent<T>(vo, parent);
            ParamVo.Put(vo);
            return error;
        }

        public static async ETTask<HashWaitError> OpenWaitToParent<T>(ParamVo vo, Entity parent = null) where T : Entity, IYIUIBind, IYIUIOpen<ParamVo>
        {
            EntityRef<Entity> parentRef = parent;

            EntityRef<CoroutineLock> coroutineLock = await YIUIMgrComponent.Inst.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.YIUIFramework, typeof(TipsHelper).GetHashCode());

            var guid = IdGenerater.Instance.GenerateId();

            var hashWait = YIUIMgrComponent.Inst.Root.GetComponent<HashWait>().Wait(guid);

            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<TipsPanelComponent, Type, Entity, long, ParamVo>(typeof(T), parentRef.Entity, guid, vo);

            coroutineLock.Entity.Dispose();

            return await hashWait;
        }
    }
}