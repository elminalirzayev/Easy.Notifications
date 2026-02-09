#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Easy.Notifications.Core.Models;
using Easy.Notifications.Persistence.EntityFramework.Entities;

namespace Easy.Notifications.Persistence.EntityFramework.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<NotificationDbContext>
    {
        public Configuration()
        {
            // Set to true only if you want EF to automatically update the DB schema
            AutomaticMigrationsEnabled = false;
        }

        /// <summary>
        /// This method will be called after migrating to the latest version.
        /// It populates lookup tables based on the C# Enums.
        /// </summary>
        protected override void Seed(NotificationDbContext context)
        {
            // 1. Seed ChannelTypes from NotificationChannelType Enum
            var channelTypes = Enum.GetValues(typeof(NotificationChannelType))
                .Cast<NotificationChannelType>()
                .Select(e => new ChannelTypeLookup
                {
                    Id = e,
                    Name = e.ToString()
                })
                .ToArray();

            // AddOrUpdate ensures no duplicates are created based on the Primary Key (Id)
            context.Set<ChannelTypeLookup>().AddOrUpdate(c => c.Id, channelTypes);

            // 2. Seed PriorityTypes from NotificationPriority Enum
            var priorityTypes = Enum.GetValues(typeof(NotificationPriority))
                .Cast<NotificationPriority>()
                .Select(e => new PriorityTypeLookup
                {
                    Id = e,
                    Name = e.ToString()
                })
                .ToArray();

            context.Set<PriorityTypeLookup>().AddOrUpdate(p => p.Id, priorityTypes);

            context.SaveChanges();
        }
    }
}

#endif