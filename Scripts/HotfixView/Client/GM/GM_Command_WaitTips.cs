using System.Collections.Generic;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    [GM(EGMType.Tips, 0, "等待窗口测试 不关闭会一直等待逻辑 Success")]
    public class GM_TipsTest0 : IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new();
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            Test().NoContext();
            await ETTask.CompletedTask;
            return true;
        }

        private async ETTask Test()
        {
            Log.Error($"打开 等待弹窗测试");
            var result = await TipsHelper.OpenWait<TipsMessageViewComponent>("回调测试", new MessageTipsExtraData() { ConfirmCallBack = () => { Debug.LogError($"回调测试, 确定按钮"); }, CancelCallBack = () => { Debug.LogError($"回调测试, 取消按钮"); } });
            Log.Error($"等待弹窗测试等待完毕 继续执行: {result}");
        }
    }

    [GM(EGMType.Tips, 0, "等待窗口测试 取消测试 Cancel")]
    public class GM_TipsTest0_1 : IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new()
            {
                new GMParamInfo(EGMParamType.Long, "取消时间", "2000"),
            };
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            var cancelTime = paramVo.Get<long>();
            var cancel     = new ETCancellationToken();
            Test().WithContext(cancel);
            WaitCancel().WithContext(cancel);
            await ETTask.CompletedTask;
            return true;

            async ETTask WaitCancel()
            {
                //模拟等待X秒后取消
                await clientScene.Root().GetComponent<TimerComponent>().WaitAsync(cancelTime);
                cancel.Cancel();
            }
        }

        private async ETTask Test()
        {
            Log.Error($"打开 等待弹窗测试 取消测试");
            var result = await TipsHelper.OpenWait<TipsMessageViewComponent>("回调测试", new MessageTipsExtraData() { ConfirmCallBack = () => { Debug.LogError($"回调测试, 确定按钮"); }, CancelCallBack = () => { Debug.LogError($"回调测试, 取消按钮"); } });
            Log.Error($"等待弹窗测试等待完毕 继续执行 取消测试: {result}");
        }
    }

    [GM(EGMType.Tips, 0, "等待窗口测试 超时测试 TimeOut")]
    public class GM_TipsTest0_2 : IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new()
            {
                new GMParamInfo(EGMParamType.Long, "超时时间", "2000"),
            };
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            var timeout = paramVo.Get<long>();
            var cancel  = new ETCancellationToken();
            Test(timeout).WithContext(cancel);
            await ETTask.CompletedTask;
            return true;
        }

        private async ETTask Test(long timeout)
        {
            Log.Error($"打开 等待弹窗测试 超时测试");
            var result = await TipsHelper.OpenWait<TipsMessageViewComponent>("回调测试", 
                new MessageTipsExtraData()
                {
                    ConfirmCallBack = () => { Debug.LogError($"回调测试, 确定按钮"); }, 
                    CancelCallBack = () => { Debug.LogError($"回调测试, 取消按钮"); }
                }).TimeoutAsync(timeout);
            Log.Error($"等待弹窗测试等待完毕 继续执行 超时测试: {result}");
        }
    }

    [GM(EGMType.Tips, 0, "等待窗口测试 取消超时测试 Cancel+TimeOut")]
    public class GM_TipsTest0_3 : IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new()
            {
                new GMParamInfo(EGMParamType.Long, "取消时间", "2000"),
                new GMParamInfo(EGMParamType.Long, "超时时间", "3000"),
            };
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            var cancelTime = paramVo.Get<long>();
            var timeout    = paramVo.Get<long>(1);
            var cancel     = new ETCancellationToken();
            Test(timeout).WithContext(cancel);
            WaitCancel().WithContext(cancel);
            await ETTask.CompletedTask;
            return true;

            async ETTask WaitCancel()
            {
                //模拟等待X秒后取消
                await clientScene.Root().GetComponent<TimerComponent>().WaitAsync(cancelTime);
                cancel.Cancel();
            }
        }

        private async ETTask Test(long timeout)
        {
            Log.Error($"打开 等待弹窗测试 取消超时测试");
            var result = await TipsHelper.OpenWait<TipsMessageViewComponent>("回调测试", 
                new MessageTipsExtraData()
                {
                    ConfirmCallBack = () => { Debug.LogError($"回调测试, 确定按钮"); }, 
                    CancelCallBack = () => { Debug.LogError($"回调测试, 取消按钮"); }
                }).TimeoutAsync(timeout);
            Log.Error($"等待弹窗测试等待完毕 继续执行 取消超时测试: {result}");
        }
    }
}
