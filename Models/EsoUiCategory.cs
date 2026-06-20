namespace Teron_Addon_Manager.Models
{
    public sealed record EsoUiCategory(long Id, string Title, long FileCount)
    {
        public override string ToString() => Title;
    }
}
