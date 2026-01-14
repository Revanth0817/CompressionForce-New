namespace CompressionForce.Domain.Entities
{
    public class GroupPrivilege
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = "";
        public string PrivilegeKey { get; set; } = "";
        public bool IsAllowed { get; set; }
    }
}
