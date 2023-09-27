using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.Concrete;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation.Results;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace MvcProje.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        // GET: User
        private Context db = new Context();

        UserProfileManager userProfile = new UserProfileManager();
        BlogManager bm = new BlogManager(new EfBlogDal());

        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult Partial1(string p)
        {
            p = (string)Session["Mail"];
            var profilevalues = userProfile.GetAuthorByMail(p);
            return PartialView(profilevalues);
        }
        public ActionResult UpdateUserProfile(Author p)
        {
            userProfile.EditAuthor(p);
            return RedirectToAction("Index");
        }
        public ActionResult BlogList(string p)
        {
            p = (string)Session["Mail"];
            Context c = new Context();
            int id = c.Authors.Where(x => x.Mail == p).Select(y => y.AuthorID).FirstOrDefault();
            var blogs = userProfile.GetBlogByAuthor(id);
            return View(blogs);
        }
        [HttpGet]
        public ActionResult UpdateBlog(int id)
        {
            Blog blog = bm.GetByID(id);
            Context c = new Context();
            List<SelectListItem> values = (from x in c.Categories.ToList()
                                           select new SelectListItem
                                           {
                                               Text = x.CategoryName,
                                               Value = x.CategoryID.ToString()
                                           }).ToList();
            ViewBag.values = values;

            List<SelectListItem> values2 = (from x in c.Authors.ToList()
                                            select new SelectListItem
                                            {
                                                Text = x.AuthorName,
                                                Value = x.AuthorID.ToString()
                                            }).ToList();
            ViewBag.values2 = values2;
            return View(blog);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateBlog([Bind(Include = "BlogID,BlogTitle,CategoryID,BlogImage,BlogDate,AuthorID,BlogContent")]Blog t, HttpPostedFileBase BlogImage, int? id)
        {
            if (ModelState.IsValid)
            {
                var s = db.Blogs.Where(x => x.BlogID == id).SingleOrDefault();
                if (BlogImage != null)
                {
                    WebImage img = new WebImage(BlogImage.InputStream);
                    FileInfo imginfo = new FileInfo(BlogImage.FileName);

                    string logoname = Guid.NewGuid().ToString() + imginfo.Extension;
                    img.Resize(500, 500);
                    img.Save("~/Uploads/BlogImage/" + logoname);

                    t.BlogImage = "/Uploads/BlogImage/" + logoname;
                }
                bm.TUpdate(t);
      
                db.SaveChanges();
                return RedirectToAction("BlogList");
            }
            return View(t);
        }
        [HttpGet]
        public ActionResult AddNewBlog()
        {
            Context c = new Context();
            List<SelectListItem> values = (from x in c.Categories.ToList()
                                           select new SelectListItem
                                           {
                                               Text = x.CategoryName,
                                               Value = x.CategoryID.ToString()
                                           }).ToList();
            ViewBag.values = values;

            List<SelectListItem> values2 = (from x in c.Authors.ToList()
                                            select new SelectListItem
                                            {
                                                Text = x.AuthorName,
                                                Value = x.AuthorID.ToString()
                                            }).ToList();
            ViewBag.values2 = values2;
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddNewBlog([Bind(Include = "BlogID,BlogTitle,CategoryID,BlogImage,BlogDate,AuthorID,BlogContent")]Blog t, HttpPostedFileBase BlogImage)
        {
            if (ModelState.IsValid)
            {
                if (BlogImage != null)
                {
                    WebImage img = new WebImage(BlogImage.InputStream);
                    FileInfo imginfo = new FileInfo(BlogImage.FileName);

                    string logoname = Guid.NewGuid().ToString() + imginfo.Extension;
                    img.Resize(500, 500);
                    img.Save("~/Uploads/BlogImage/" + logoname);

                    t.BlogImage = "/Uploads/BlogImage/" + logoname;
                }
                db.Blogs.Add(t);
                db.SaveChanges();
                return RedirectToAction("BlogList");
            }
            BlogValidator blogValidator = new BlogValidator();
            ValidationResult results = blogValidator.Validate(t);
            if (results.IsValid)
            {
                bm.TAdd(t);
                
                return RedirectToAction("BlogList");
            }
            else
            {
                foreach (var item in results.Errors)
                {
                    ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                }
            }

            return View(t);
        }

        public ActionResult DeleteBlog(int id)
        {
            Blog blog = bm.GetByID(id);
            bm.TDelete(blog);
            return RedirectToAction("BlogList");
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("AuthorLogin", "Login");
        }

    }
}