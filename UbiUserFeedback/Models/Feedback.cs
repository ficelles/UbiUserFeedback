//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UbiUserFeedback.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Feedback
    {
        public string SessionID { get; set; }
        public string UserID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public Nullable<System.DateTime> SavedOn { get; set; }
    }
}
