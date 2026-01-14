namespace CompressionForce.Models
{
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string AccessLevel { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
