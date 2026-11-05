using FcgNotificationsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FcgNotificationsApi.Data;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }
}
