using System.ComponentModel.DataAnnotations;

namespace NexusERP.Api.DTOs
{
    public class RoleResponseDto
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class RoleUpsertDto
    {
        public int RoleId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();
    }
}
