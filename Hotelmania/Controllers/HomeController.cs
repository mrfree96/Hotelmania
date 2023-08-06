using Hotelmania.Models;
using Hotelmania.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Hotelmania.Controllers
{

    public class HomeController : Controller
    {
        HotelReservationDBEntities db = new HotelReservationDBEntities();

        // GET: Home
        public ActionResult Index()
        {
            List<TypeofActivity> activities = db.TypeofActivities.ToList();
            List<Restaurant> restaurants = db.Restaurants.ToList();
            List<Comment> comments = db.Comments.ToList();
            IndexViewModel model = new IndexViewModel
            {
                TypeofActivities = activities,
                Restaurants = restaurants,
                Comments = comments
            };
            return View(model);
        }

        public ActionResult Gallery()
        {
            return View();
        }

        //Continue...
        [Authorize]
        public ActionResult Reservation()
        {
            List<Room> rooms = db.Rooms.ToList();
            List<SelectListItem> roomTypes = new List<SelectListItem>();
            roomTypes.Add(new SelectListItem { Text = "Single", Value = "Single" });
            roomTypes.Add(new SelectListItem { Text = "Double", Value = "Double" });
            roomTypes.Add(new SelectListItem { Text = "Family", Value = "Family" });

            ViewBag.RoomList = roomTypes;

            Reservation emptyReservation = new Reservation();

            //Controlling whether session is exist or not
            if(Session["ReservationError"] != null)
            {
                ViewBag.ErrorMessage = Session["ReservationError"].ToString();
            }

            return View(emptyReservation);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            if(user.UserName == "Admin")
            {
                bool adminExist = false;
                List<Admin> admins = db.Admins.ToList();
                foreach (Admin a in admins)
                {
                    if (a.UserName == user.UserName && a.Password == user.Password)
                        adminExist = true;
                }
                //If admin exist in db
                if (adminExist)
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, false);
                    return RedirectToAction("Index", "Admin");
                }
                //if admin is not found...
                else
                {
                    return View();
                }
            }


            //user login
            if (ModelState.IsValid)
            {
                bool userExist = false;
                List<User> users = db.Users.ToList();
                foreach (User u in users)
                {
                    if (u.UserName == user.UserName && u.Password == user.Password)
                        userExist = true;
                }
                //If user exist in db
                if (userExist)
                {
                    ViewBag.UserNotFound = "false";
                    FormsAuthentication.SetAuthCookie(user.UserName, false);
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.UserNotFound = "true";
                    return View();
                }

            }
            return View();
        }

        private ActionResult RedirectToAction()
        {
            throw new NotImplementedException();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult Register()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [Authorize]
        public ActionResult Profiles()
        {

            string userName = User.Identity.Name;
            //comparing current logged in user with Users table
            var u = db.Users.FirstOrDefault(x => x.UserName == userName);
            return View(u);
        }

        [Authorize]
        public ActionResult ProfileEdit(int? id)
        {
            var u = db.Users.Find(id);
            string currentUser = User.Identity.Name;
            //if profile is not exist return notfound
            if (u == null)
            {
                return HttpNotFound();
            }
            else
            {
                //If you are trying to Edit someone else's profile, authorization error occurs!
                if (currentUser != u.UserName)
                {
                    return View("_UnauthorizedToAccess");
                }
                //if you are trying to edit your own profile
                else
                {
                    return View(u);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileEdit(int? id, User user)
        {
            if (ModelState.IsValid)
            {
                //getting user by id
                User u = db.Users.Find(id);
                //updating process...
                db.Entry(u).State = EntityState.Modified;
                u.FirstName = user.FirstName;
                u.LastName = user.LastName;
                u.Email = user.Email;
                u.PhoneNo = user.PhoneNo;
                u.UserName = user.UserName;
                u.Password = user.Password;
                db.SaveChanges();
                return RedirectToAction("Profiles");
            }
            return View();
        }

        [NonAction]
        public User GetCurrentUser()
        {
            User user = new User();
            string userName = User.Identity.Name;
            //comparing current logged in user with Users table
            var u = db.Users.FirstOrDefault(x => x.UserName == userName);
            return u;
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckAvailability(Reservation reservation)
        {
            //room list to process
            List<Room> rooms = db.Rooms.ToList();
            List<Reservation> reservations = db.Reservations.ToList();

            //current reservation
            Reservation thisReservation = new Reservation();

            //If validation fails...
            if(reservation.StartDate >= reservation.EndDate || reservation.StartDate < DateTime.Now )
            {
                //Creating session with an error message...
                Session["ReservationError"] = "Reservation Date interval is invalid!";
                return RedirectToAction("Reservation", thisReservation);
            }
            else
            {
                //validation is successfull so we don't need that session anymore you can remove it..
                if (Session["ReservationError"] != null)
                {
                    Session.Remove("ReservationError");
                }
                //searching for a room
                foreach (var r in rooms)
                {
                    //comparing roomType...
                    if(r.RoomType == reservation.Room.RoomType)
                    {
                        if (r.IsReserved)
                        {
                            //getting old reservation of the room
                            Reservation oldReservation = db.Reservations.FirstOrDefault(x => x.RoomID == r.RoomID);

                            //if you are trying to reserve the room a date before the oldReservation
                            if (reservation.StartDate < oldReservation.StartDate && reservation.EndDate < oldReservation.StartDate)
                            {
                                //creating reservation
                                thisReservation.RoomID = r.RoomID;
                                thisReservation.StartDate = reservation.StartDate;
                                thisReservation.EndDate = reservation.EndDate;
                                //calculating time interval..
                                TimeSpan span = thisReservation.EndDate.Subtract(thisReservation.StartDate);
                                //total price for reserved time interval..
                                decimal price = r.PricePerDay * span.Days;
                                //to display in View
                                ViewBag.TotalPrice = price;
                                thisReservation.Price = price;
                                thisReservation.UserID = GetCurrentUser().UserID;

                                db.Reservations.Add(thisReservation);
                                db.SaveChanges();
                                ViewBag.ReservationState = "successful";
                                return View(thisReservation);
                            }
                            //if you are tring to reserve the room a date after the oldReservation
                            else if (reservation.StartDate > oldReservation.EndDate && reservation.EndDate > oldReservation.EndDate)
                            {
                                //creating reservation
                                thisReservation.RoomID = r.RoomID;
                                thisReservation.StartDate = reservation.StartDate;
                                thisReservation.EndDate = reservation.EndDate;
                                //calculating time interval..
                                TimeSpan span = thisReservation.EndDate.Subtract(thisReservation.StartDate);
                                //total price for reserved time interval..
                                decimal price = r.PricePerDay * span.Days;
                                //to display in View
                                ViewBag.TotalPrice = price;
                                thisReservation.Price = price;
                                thisReservation.UserID = GetCurrentUser().UserID;

                                db.Reservations.Add(thisReservation);
                                db.SaveChanges();
                                ViewBag.ReservationState = "successful";
                                return View(thisReservation);
                            }
                            //if you cannot reserve the room because of a confliction
                            else
                            {
                                //...You cannot reserve the room...//
                                continue;
                            }
                        }
                        else
                        {
                            //updating room
                            db.Entry(r).State = EntityState.Modified;
                            r.IsReserved = true;
                            db.SaveChanges();

                            //creating reservation
                            thisReservation.RoomID = r.RoomID;
                            thisReservation.StartDate = reservation.StartDate;
                            thisReservation.EndDate = reservation.EndDate;
                            TimeSpan span = thisReservation.EndDate.Subtract(thisReservation.StartDate);
                            //total price
                            decimal price = r.PricePerDay * span.Days;
                            //to display in View
                            ViewBag.TotalPrice = price;
                            thisReservation.Price = price;
                            thisReservation.UserID = GetCurrentUser().UserID;

                            db.Reservations.Add(thisReservation);
                            db.SaveChanges();
                            ViewBag.ReservationState = "successful";
                            return View(thisReservation);
                        }
                    }
                    //if current room's roomtype is not desired room type continue to search for desired room type...
                    else
                    {
                        continue;
                    }
                }
            }
            //if you couldn't reserve a room
            ViewBag.ReservationState = "fail";
            return View(thisReservation);
            
        }

        [Authorize]
        public ActionResult Payment(int? id)
        {
            Reservation r = db.Reservations.Find(id);
            List<SelectListItem> months = new List<SelectListItem>();
            for (int i = 1; i < 13; i++)
            {
                months.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 19; i < 41; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewBag.Months = months;
            ViewBag.Years = years;

            if(Session["ErrMessage"] != null)
            {
                string errMess = Session["ErrMessage"].ToString();
                ViewBag.ErrorMessage = errMess;
            }
            
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayNow(string CardHolderName,string CardNo, string ExpirationMonth, 
            string ExpirationYear,string CVV ,string ReservationID)
        {
            //reservation...
            int resID = Convert.ToInt32(ReservationID);
            Reservation currentReservation = db.Reservations.Find(resID);

            //firstly validate the credit card
            CreditCard card = new CreditCard
            {
                CreditFullName = CardHolderName,
                CreditCardNo = CardNo,
                ExpirationDateMonth = Convert.ToInt32(ExpirationMonth),
                ExpirationDateYear = Convert.ToInt32(ExpirationYear),
                CVVNo = Convert.ToInt32(CVV)
            };

            CreditCard validatedCard = SearchForCreditCard(card);
            if (validatedCard != null)
            {
                //if session exist remove it...
                if(Session["ErrMessage"] != null)
                {
                    Session.Remove("ErrMessage"); //try
                }
                
                decimal paymentAmount = currentReservation.Price;
                //Creating payment...
                Payment payment = new Payment
                {
                    ReservationID = currentReservation.ReservationID,
                    IsSuccessful = true
                };
                //inserting payment to db...
                db.Payments.Add(payment);
                db.SaveChanges();

                //updating reservation isapproved part...
                db.Entry(currentReservation).State = EntityState.Modified;
                currentReservation.IsApproved = true;
                db.SaveChanges();

                //Bill creation process...
                Bill createdBill = CreateBill(currentReservation, payment);
                return RedirectToAction("PaymentResult");

            }
            //credit card is not valid...
            else
            {
                Session["ErrMessage"] = "Credit card is not valid!"; //try
                return RedirectToAction("Payment", "Home", new { id = currentReservation.ReservationID });
            }
        }

        [NonAction]
        public CreditCard SearchForCreditCard(CreditCard cc)
        {
            List<CreditCard> creditCards = db.CreditCards.ToList();
            foreach (var item in creditCards)
            {
                //if credit card infos correct returns true...
                if(item.CreditFullName == cc.CreditFullName && item.CreditCardNo == cc.CreditCardNo 
                    && item.ExpirationDateMonth == cc.ExpirationDateMonth && item.ExpirationDateYear == cc.ExpirationDateYear 
                    && item.CVVNo == cc.CVVNo)
                {
                    return item;
                }
            }
            return null;
        }

        [NonAction]
        public Bill CreateBill(Reservation res, Payment pay)
        {
            int uid = GetCurrentUser().UserID;

            Bill bill = new Bill
            {
                UserID = uid,
                ReservationID = res.ReservationID,
                PaymentID = pay.PaymentID
            };

            //DB inserting process...
            db.Bills.Add(bill);
            db.SaveChanges();

            return bill;
        }

        [Authorize]
        public ActionResult PaymentResult()
        {
            ViewBag.PaymentState = "Is Successful!";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(Comment comment)
        {
            db.Comments.Add(comment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}