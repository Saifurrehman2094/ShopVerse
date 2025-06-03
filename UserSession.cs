using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Helpers
{
    public static class UserSession
    {
        public static int CustomerId { get; set; }
        public static int LogisticsId { get; set; }

        public static int SellerId { get; set; }

        public static int AdminId { get; set; }
    }
}
