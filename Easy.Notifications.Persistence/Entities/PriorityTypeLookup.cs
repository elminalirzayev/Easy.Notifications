using Easy.Notifications.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy.Notifications.Persistence.EntityFramework.Entities
{
    
    [Table("PriorityTypes")]
    public class PriorityTypeLookup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public NotificationPriority Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}