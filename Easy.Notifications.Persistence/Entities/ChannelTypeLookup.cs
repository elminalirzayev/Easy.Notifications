using Easy.Notifications.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy.Notifications.Persistence.EntityFramework.Entities
{
    [Table("ChannelTypes")]
    public class ChannelTypeLookup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public NotificationChannelType Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}