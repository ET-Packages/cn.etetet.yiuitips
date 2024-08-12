namespace ET.Client
{
    [ComponentOf(typeof(YIUIWindowComponent))]
    public partial class TipsViewComponent : Entity, IAwake, IYIUIWindowClose
    {
    }

    /// <summary>
    /// 通用弹窗view关闭事件
    /// </summary>
    public struct EventPutTipsView
    {
        public Entity View;
    }
}