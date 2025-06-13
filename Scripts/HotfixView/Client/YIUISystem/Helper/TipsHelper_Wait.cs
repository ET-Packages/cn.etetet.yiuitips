using System;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 可等待的弹窗
    /// </summary>
    public static partial class TipsHelper
    {
        public static async ETTask<HashWaitError> OpenWait<T>(Scene scene, params object[] paramMore) where T : Entity, IYIUIBind, IYIUIOpen<ParamVo>
        {
            var vo = ParamVo.Get(paramMore);
            var error = await OpenWaitToParent<T>(scene, vo);
            ParamVo.Put(vo);
            return error;
        }

        public static async ETTask<HashWaitError> OpenWaitToParent<T>(Scene scene, Entity parent, params object[] paramMore) where T : Entity, IYIUIBind, IYIUIOpen<ParamVo>
        {
            var vo = ParamVo.Get(paramMore);
            var error = await OpenWaitToParent<T>(scene, vo, parent);
            ParamVo.Put(vo);
            return error;
        }

        public static async ETTask<HashWaitError> OpenWaitToParent<T>(Scene scene, ParamVo vo, Entity parent = null) where T : Entity, IYIUIBind, IYIUIOpen<ParamVo>
        {
            EntityRef<Entity> parentRef = parent;
            EntityRef<Scene> sceneRef = scene;
            EntityRef<CoroutineLock> coroutineLock = await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.YIUIFramework, typeof(TipsHelper).GetHashCode());
            var guid = IdGenerater.Instance.GenerateId();
            scene = sceneRef;
            var yiuiMgrRoot = scene.YIUIMgrRoot();
            var hashWait = yiuiMgrRoot.GetComponent<HashWait>().Wait(guid);
            await yiuiMgrRoot.OpenPanelAsync<TipsPanelComponent, Type, Entity, long, ParamVo>(typeof(T), parentRef.Entity, guid, vo);
            coroutineLock.Entity.Dispose();
            return await hashWait;
        }
    }
}