//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hotelmania.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Bill
    {
        public int BID { get; set; }
        public int UserID { get; set; }
        public int ReservationID { get; set; }
        public int PaymentID { get; set; }
    
        public virtual Payment Payment { get; set; }
        public virtual Reservation Reservation { get; set; }
        public virtual User User { get; set; }
    }
}