using Hotelmania.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hotelmania.ViewModels
{
    public class IndexViewModel
    {
        public List<TypeofActivity> TypeofActivities { get; set; }
        public List<Restaurant> Restaurants { get; set; }
        public List<Comment> Comments { get; set; }
    }
}