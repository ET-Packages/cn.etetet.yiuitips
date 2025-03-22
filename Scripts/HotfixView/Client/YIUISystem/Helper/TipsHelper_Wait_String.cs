using System;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 可等待的弹窗 不知道泛型 只知道UI资源名称时使用
    /// 缺点这里没办法判断类型是否正确 是否有 IYIUIOpen〈ParamVo〉 接口
    /// </summary>
    public static partial class TipsHelper
    {
        public static async ETTask<HashWaitError> OpenWait(string resName, params object[] paramMore)
        {
            var vo    = ParamVo.Get(paramMore);
            var error = await OpenWaitToParent(resName, vo);
            ParamVo.Put(vo);
            return error;
        }

        public static async ETTask<HashWaitError> OpenWaitToParent(string resName, Entity parent, params object[] paramMore)
        {
            var vo    = ParamVo.Get(paramMore);
            var error = await OpenWaitToParent(resName, vo, parent);
            ParamVo.Put(vo);
            return error;
        }

        public static async ETTask<HashWaitError> OpenWaitToParent(string resName, ParamVo vo, Entity parent = null)
        {
            var data = YIUIBindHelper.GetBindVoByResName(resName);
            if (data == null) return HashWaitError.Error;
            var bindVo = data.Value;

            var coroutineLock = await YIUIMgrComponent.Inst.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.YIUIFramework, typeof(TipsHelper).GetHashCode());

            var guid = IdGenerater.Instance.GenerateId();

            var hashWait = YIUIMgrComponent.Inst.Root.GetComponent<HashWait>().Wait(guid);

            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<TipsPanelComponent, Type, Entity, long, ParamVo>(bindVo.ComponentType, parent, guid, vo);

            coroutineLock.Dispose();
            return await hashWait;
        }
    }
}