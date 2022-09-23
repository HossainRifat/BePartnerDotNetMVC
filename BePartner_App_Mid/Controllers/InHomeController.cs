using BePartner_App_Mid.EF;
using BePartner_App_Mid.Models;
using ClosedXML.Excel;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Core.ExcelPackage;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace BePartner_App_Mid.Controllers
{
    [Authorize]
    public class InHomeController : Controller
    {
        public ActionResult Index()
        {
            var db = new bePartnerCentralDatabaseEntities2();
            var data = (from I in db.Ideas where I.Status.Equals("Confirmed") select I).ToList();
            return View(data);
        }

        public ActionResult InProfile(string Id)
        {
            if (Session["In_Email"] != null)
            {
                var email = Session["In_Email"].ToString();
                var db = new bePartnerCentralDatabaseEntities2();
                var In = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();
                if(Id != null)
                {
                    if (Id.Equals("profile"))
                    {


                        ViewBag.Message = "profile";
                        return View(In);
                    }
                    else
                    {
                        ViewBag.Img = In.Img; 
                        ViewBag.Name = In.FirstName + " " + In.LastName;
                        var En = (from I in db.Ideas where I.In_Asp_Email.Equals(email) select I).ToList();
                        ViewBag.Message = "business";
                        ViewBag.image = In.Img;
                        return View(En);
                    }
                }
                else
                {
                    ViewBag.Message = "profile";
                    return View(In);
                }
                
                
            }
            return RedirectToAction("InLogin", "Investor");
        }

        [HttpGet]
        public ActionResult InStartups(string Id)
        {
            if(Id == null)
            {
                var db = new bePartnerCentralDatabaseEntities2();
                var data = (from I in db.Ideas where !I.Status.Equals("Confirmed") select I).ToList();
                return View(data);
            }
            else
            {
                if (Id.Equals("AtoZ"))
                {
                    var db = new bePartnerCentralDatabaseEntities2();
                    var data = (from I in db.Ideas where !I.Status.Equals("Confirmed") orderby I.Company_Name ascending select I).ToList();
                    return View(data);
                }
                else
                {
                    var db = new bePartnerCentralDatabaseEntities2();
                    var data = (from I in db.Ideas where !I.Status.Equals("Confirmed") orderby I.Company_Name descending select I).ToList();
                    return View(data);
                }
            }
            
        }

        [HttpPost]
        public ActionResult InStartups(InvestorSend In)
        {
            var db = new bePartnerCentralDatabaseEntities2();
            var data = (from I in db.Ideas where !I.Status.Equals("Confirmed") select I).ToList();
            return View(data);
        }



        [HttpGet]
        public ActionResult InEditPersonal()
        {
            var email = Session["In_Email"].ToString();
            var db = new bePartnerCentralDatabaseEntities2();
            var In = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();
            return View(In);
        }

        [HttpPost]
        public ActionResult InEditPersonal(InvestorPersonal In)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = "Data Updated not";
                if (InUpdatePer(In))
                {
                    ViewBag.Message = "Data Updated";
                    return RedirectToAction("InProfile");
                }

            }
            //ViewBag.Message = "Data Update Failed";
            return View(In);

        }

        [HttpGet]
        public ActionResult InEditProfessional()
        {
            var email = Session["In_Email"].ToString();
            var db = new bePartnerCentralDatabaseEntities2();
            var In = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();
            return View(In);
        }

        [HttpPost]
        public ActionResult InEditProfessional(InvestorProfational In)
        {
            if (ModelState.IsValid)
            {
                if (InUpdatePro(In))
                {
                    ViewBag.Message = "Data Updated";
                    return RedirectToAction("InProfile");
                }
            }
            var db = new bePartnerCentralDatabaseEntities2(); 
            var email = Session["In_Email"].ToString();
            var Ed = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();
            ViewBag.Message = "Data Update Failed";
            return View(Ed);
        }

        [HttpGet]
        public ActionResult InEditPass()
        {
            //var email = Session["In_Email"].ToString();
            //var db = new bePartnerCentralDatabaseEntities2();
            //var In = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult InEditPass(InvestorEditPass In)
        {
            if (ModelState.IsValid)
            {
                if (InUpdatePass(In))
                {
                    ViewBag.Message = "Data Updated";
                    return RedirectToAction("InProfile");
                }
            }
            ViewBag.Message = "Data Update Failed";
            return View();
        }

        [HttpGet]
        public ActionResult InMessenger()
        {
            var email = Session["In_Email"].ToString();
            var db = new bePartnerCentralDatabaseEntities2();
            var msgs = (from I in db.Messages where I.Sender.Equals(email) || I.Receiver.Equals(email) select I).ToList();

            List<InvestorMessage> listmsg = new List<InvestorMessage>();
            
            //ViewBag.Message = msgs.Count;
            foreach(var m in msgs)
            {
                var msg = new InvestorMessage();
                msg.MsgId = m.MsgId;
                msg.Sender = m.Sender;
                msg.Receiver = m.Receiver;
                msg.Message = m.Message1;
                msg.Status = m.Status;
                msg.Ttime = m.Time;

                if (m.Sender.Equals(email))
                {
                    var en = (from I in db.Entrepreneurs where I.En_Email.Equals(m.Receiver) select I).FirstOrDefault();
                    msg.ReceiverName = en.FirstName + " " + en.LastName;
                    msg.ReceiverOccupation = en.Occupation;
                    msg.ReceiverImg = en.Img;
                    msg.ReceiverPhone = en.Phone;

                    var com = (from I in db.Ideas where I.En_Post_Email.Equals(m.Receiver) select I).FirstOrDefault();
                    msg.ReceiverCompany = com.Company_Name;

                    msg.SenderName = null;
                    msg.SenderOccupation = null;
                    msg.SenderImg = null;
                    msg.SenderCompany = null;
                }
                else
                {
                    var en = (from I in db.Entrepreneurs where I.En_Email.Equals(m.Sender) select I).FirstOrDefault();
                    msg.SenderName = en.FirstName + " " + en.LastName;
                    msg.SenderOccupation = en.Occupation;
                    msg.SenderImg = en.Img;
                    msg.SenderPhone = en.Phone;

                    var com = (from I in db.Ideas where I.En_Post_Email.Equals(m.Sender) select I).FirstOrDefault();
                    msg.SenderCompany = com.Company_Name;

                    msg.ReceiverName = null;
                    msg.ReceiverOccupation = null;
                    msg.ReceiverImg = null;
                    msg.ReceiverCompany = null;
                }
                listmsg.Add(msg);
            }
            //ViewBag.Message = listmsg.Count + "ss";
            return View(listmsg);
        }


        [HttpPost]
        public ActionResult InMessenger(InvestorSend InSend)
        {

            var email = Session["In_Email"].ToString();
            var db = new bePartnerCentralDatabaseEntities2();

            if (ModelState.IsValid)
            {
                string[] arr = InSend.Receiver.Split(' ');
                InSend.time = DateTime.Now.ToString();
                InSend.Status = "Sent";
                InSend.Sender = email;
                InSend.Receiver = arr[1];

                if (InSendMessage(InSend))
                {
                    ViewBag.Message = "Data Inserted";
                }
                else
                {
                    ViewBag.Message = "Something Went Wrong";
                }

            }
            else
            {
                ViewBag.Message = "Invalid";
            }
            
            
            var msgs = (from I in db.Messages where I.Sender.Equals(email) || I.Receiver.Equals(email) select I).ToList();

            List<InvestorMessage> listmsg = new List<InvestorMessage>();


            foreach (var m in msgs)
            {
                var msg = new InvestorMessage();
                msg.MsgId = m.MsgId;
                msg.Sender = m.Sender;
                msg.Receiver = m.Receiver;
                msg.Message = m.Message1;
                msg.Status = m.Status;
                msg.Ttime = m.Time;

                if (m.Sender.Equals(email))
                {
                    var en = (from I in db.Entrepreneurs where I.En_Email.Equals(m.Receiver) select I).FirstOrDefault();
                    msg.ReceiverName = en.FirstName + " " + en.LastName;
                    msg.ReceiverOccupation = en.Occupation;
                    msg.ReceiverImg = en.Img;
                    msg.ReceiverPhone = en.Phone;

                    var com = (from I in db.Ideas where I.En_Post_Email.Equals(m.Receiver) select I).FirstOrDefault();
                    msg.ReceiverCompany = com.Company_Name;

                    msg.SenderName = null;
                    msg.SenderOccupation = null;
                    msg.SenderImg = null;
                    msg.SenderCompany = null;
                }
                else
                {
                    var en = (from I in db.Entrepreneurs where I.En_Email.Equals(m.Sender) select I).FirstOrDefault();
                    msg.SenderName = en.FirstName + " " + en.LastName;
                    msg.SenderOccupation = en.Occupation;
                    msg.SenderImg = en.Img;
                    msg.SenderPhone = en.Phone;

                    var com = (from I in db.Ideas where I.En_Post_Email.Equals(m.Sender) select I).FirstOrDefault();
                    msg.SenderCompany = com.Company_Name;

                    msg.ReceiverName = null;
                    msg.ReceiverOccupation = null;
                    msg.ReceiverImg = null;
                    msg.ReceiverCompany = null;
                }
                listmsg.Add(msg);
            }

            return View(listmsg);
        }

        [HttpGet]
        public ActionResult InBusinessDetails(string Id)
        {
            var db = new bePartnerCentralDatabaseEntities2();
            var Ed = (from I in db.Ideas where I.Company_Name.Equals(Id) select I).FirstOrDefault();
            if(Ed != null)
            {
                return View(Ed);
            }
            return RedirectToAction("InStartups");
        }

        [HttpPost]
        public ActionResult InBusinessDetails(InOffer In)
        {
            var email = Session["In_Email"].ToString();
            var db = new bePartnerCentralDatabaseEntities2();
            var Ed = (from I in db.Ideas where I.En_Post_Email.Equals(In.En_Email) select I).FirstOrDefault();
            In.In_Email = email;

            

            if(Ed.Offer_Share != null)
            {
                ViewBag.Message = "Something Went Wrong";
                var share = int.Parse(Ed.Offer_Share);
                var amount = int.Parse(Ed.Offer_Amount);
                if (share <= int.Parse(In.Share) && amount <= int.Parse(In.Value))
                {
                    Ed.Offer_Share = In.Share;
                    Ed.Offer_Amount = In.Value;
                    Ed.Message = In.Details;
                    Ed.Status = email;
                    db.SaveChanges();
                    ViewBag.Message = "Offered";
                    return View(Ed);
                }
            }
            else
            {
                Ed.Offer_Share = In.Share;
                Ed.Offer_Amount = In.Value;
                Ed.Message = In.Details;
                Ed.Status = email;
                db.SaveChanges();
                ViewBag.Message = "Offered";
                return View(Ed);
            }

            
            return View(Ed);
        }

        public ActionResult InConfirmDeal(String Id)
        {

            var db = new bePartnerCentralDatabaseEntities2();
            var email = Session["In_Email"].ToString();
            var Ed = (from I in db.Ideas where I.Company_Name.Equals(Id) select I).FirstOrDefault();

            Ed.In_Asp_Email = email;
            Ed.Status = "Confirmed";

            var InSend = new InvestorSend();
            InSend.time = DateTime.Now.ToString();
            InSend.Status = "Sent";
            InSend.Sender = email;
            InSend.Message = "Hello, I have confirmed your deal.";
            InSend.Receiver = Ed.En_Post_Email;
            InSendMessage(InSend);
            ////string subject = "Deal conformation";
            ////string body = "Hello, " + email + "has confirmed your deal.";
            ////WebMail.Send("rh160763@gmail.com", subject, body, null, null, null, true, null, null, null, null, null, null);

            //MailMessage msg = new MailMessage();

            //msg.From = new MailAddress("rh140025@gmail.com");
            //msg.To.Add("rh160763@gmail.com");
            //msg.Subject = "test";
            //msg.Body = "Test Content";
            //msg.Priority = MailPriority.High;

            //SmtpClient client = new SmtpClient();

            //client.Credentials = new NetworkCredential("rh140025@gmail.com", "dujmijrurwstslqz", "smtp.gmail.com");
            //client.Host = "smtp.gmail.com";
            //client.Port = 465;
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.EnableSsl = true;
            //client.UseDefaultCredentials = false;
            //client.Send(msg);
            try
            {
                db.SaveChanges();
               

                    return RedirectToAction("InProfile");


                



                return RedirectToAction("InStartups");

            }
            catch
            {
                return RedirectToAction("InStartups");
            }
            
        }

        public ActionResult InLogout()
        {
            FormsAuthentication.SignOut();
            Session.RemoveAll();
            return RedirectToAction("InLogin", "Investor");
        }


        //**************************************** DATABASE STARTS ****************************************

        public bool InUpdatePer(InvestorPersonal In)
        {
            var email = Session["In_Email"].ToString();
            var db = new bePartnerCentralDatabaseEntities2();
            var Ed = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();
            Ed.FirstName = In.FirstName;
            Ed.LastName = In.LastName;
            Ed.Gender = In.Gender;
            Ed.Dob = In.Dob;
            Ed.Phone = In.Phone;
            Ed.Nid = In.Nid;
            Ed.Address = In.Address;
            //Ed.In_Email = In.In_Email;

            try
            {
                db.SaveChanges();
                Session.RemoveAll();
                Session["In_Email"] = In.In_Email;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool InUpdatePro(InvestorProfational In)
        {
            var db = new bePartnerCentralDatabaseEntities2();
            var email = Session["In_Email"].ToString();
            var Ed = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();

            Ed.OrgName = In.OrgName;
            Ed.OrgEstablished = In.OrgEstablished;
            Ed.OrgLocation = In.OrgLocation;
            Ed.OrgEmail = In.OrgEmail;
            Ed.OrgPhone = In.OrgPhone;
            Ed.OrgLicense = In.OrgLicense;
            Ed.Tin = In.Tin;
            Ed.OrgSite = In.OrgSite;

            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool InUpdatePass(InvestorEditPass In)
        {
            var db = new bePartnerCentralDatabaseEntities2();
            var email = Session["In_Email"].ToString();
            var Ed = (from I in db.Investors where I.In_Email.Equals(email) select I).FirstOrDefault();

            Ed.Password = In.ConfirmNewPassword;

            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool InSendMessage(InvestorSend m)
        {
            var msg = new Message()
            {
                Sender = m.Sender,
                Receiver = m.Receiver,
                Message1 = m.Message,
                Status = m.Status,
                Time = m.time,
                
            };
            var db = new bePartnerCentralDatabaseEntities2();
            db.Messages.Add(msg);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //public ActionResult InDownload()
        //{
        //    var db = new bePartnerCentralDatabaseEntities2();
        //    var email = Session["In_Email"].ToString();
        //    var msgs = (from I in db.Messages where I.Sender.Equals(email) || I.Receiver.Equals(email) select I).ToList();

        //    ExcelPackage pck = new ExcelPackage();
        //    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");
        //    ws.Cells["A1"].Value = "MessageReport";

        //    return RedirectToAction("MessageReport");

        //}

        [HttpPost]
        public FileResult Download_investment()
        {
            var email = Session["In_Email"].ToString();
            DataTable DT_A_A = new DataTable("Grid");
            DT_A_A.Columns.AddRange(new DataColumn[9]
            {
               new DataColumn("Company Name"),
               new DataColumn("Entrepreneur Email"),
               new DataColumn("Moto"),
               new DataColumn("Category"),
               new DataColumn("Description"),
               new DataColumn("Investment"),
               new DataColumn("Share"),
               new DataColumn("Valuation"),
               new DataColumn("Total Investment")
            });
            var db = new bePartnerCentralDatabaseEntities2();
            var In_data = (from I in db.Ideas where I.In_Asp_Email.Equals(email) select I).ToList();
            int T = 0;
            foreach (var In in In_data)
            {
                T = T + int.Parse(In.Asking_Amount);
                DT_A_A.Rows.Add(
                    In.Company_Name,
                    In.En_Post_Email,
                    In.Moto,
                    In.Category,
                    In.Description,
                    In.Asking_Amount,
                    In.Asking_Share,
                    In.Valuation,
                    T.ToString()
                    );
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(DT_A_A);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "My Investment.xlsx");
                }
            }



        }
        [AllowAnonymous]
        public ActionResult test()
        {
            var db = new bePartnerCentralDatabaseEntities2();
            var data = (from I in db.Ideas where I.Status.Equals("Posted") select I).ToList();
            return View(data);
        }

        [AllowAnonymous]
        public JsonResult En_Search(string Id)
        {
            var db = new bePartnerCentralDatabaseEntities2();
            List<Idea> idea = (from I in db.Ideas where I.Company_Name.Contains(Id) && !I.Status.Equals("Confirmed") select I).ToList();

            string value = string.Empty;
            value = JsonConvert.SerializeObject(idea, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InReport(Admin_Report r)
        {
           
                var db = new bePartnerCentralDatabaseEntities2();
                var email = Session["In_Email"].ToString();
                var report = new BePartner_App_Mid.EF.Report()
                {
                    sender = email,
                    Receiver = r.Receiver,
                    Title = r.Subject,
                    Description = r.WMFH,
                    Status = "Sent",
                    Report_Time = DateTime.Now
                };
                db.Reports.Add(report);
                db.SaveChanges();

            

            return RedirectToAction("InProfile");
        }
    }
}