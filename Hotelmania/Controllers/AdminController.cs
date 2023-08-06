using Hotelmania.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Hotelmania.Controllers
{
    [Authorize(Users = "Admin")]
    public class AdminController : Controller
    {
        HotelReservationDBEntities db = new HotelReservationDBEntities();
        public ActionResult Index()
        {
            int totalReservation = db.Reservations.Count();
            List<Reservation> reservations = db.Reservations.ToList();
            decimal totalEarnings = 0;
            foreach (var r in reservations)
            {
                if(r.IsApproved)
                {
                    totalEarnings += r.Price;
                }
            }

            ViewBag.TotalReservation = totalReservation;
            ViewBag.TotalEarnings = totalEarnings;

            return View(reservations);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login","Home");
        }

        public ActionResult Rooms()
        {
            List<Room> rooms = db.Rooms.ToList();
            return View(rooms);
        }

        public ActionResult Users()
        {
            List<User> users = db.Users.ToList();
            return View(users);
        }
    }
}