using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Queries
{
    internal static class AnnouncementQueries
    {
        public const string GetAllAnnouncements = @"
            SELECT *
            FROM Announcements
            ORDER BY PublishStart DESC;
        ";
    }
}