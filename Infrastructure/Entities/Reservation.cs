using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Entities
{
    public class Reservation
    {
        [DbManager.DbColumn("reservation_id")]
        public int ReservationId { get; set; }
        [DbManager.DbColumn("reservation_name")]
        public string ReservationName { get; set; }

        [DbManager.DbColumn("created_by")]
        public int CreatedBy { get; set; }
    }
}
