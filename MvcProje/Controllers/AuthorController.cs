using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.EntityFramework;
using DataAccessLayer.Concrete;
using EntityLayer.Concrete;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MvcProje.Controllers
{
    public class AuthorController : Controller
    {
        // GET: Author
        private Context db = new Context();

        BlogManager blogmanager = new BlogManager(new EfBlogDal());
        AuthorManager authormanager = new AuthorManager(new EfAuthorDal());

        [AllowAnonymous]
        public PartialViewResult AuthorAbout(int id)
        {
            var authordetail = blogmanager.GetBlogByID(id);
            return PartialView(authordetail);
        }
        [AllowAnonymous]
        public PartialViewResult AuthorPopularPost(int id)
        {
            var blogauthorid = blogmanager.GetList().Where(x => x.BlogID == id).Select(y => y.AuthorID).FirstOrDefault();

            var authorblogs = blogmanager.GetBlogByAuthor(blogauthorid);
            return PartialView(authorblogs);
        }


        public ActionResult AuthorList()
        {
            var authorlists = authormanager.GetList();
            return View(authorlists);
        }

        [HttpGet]
        public ActionResult AddAuthor()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddAuthor([Bind(Include = "AuthorID,AuthorName,AuthorImage,AuthorAbout,AuthorTitle,AboutShort,Mail,Password,PhoneNumber,Blogs")]Author t, HttpPostedFileBase AuthorImage)
        {
            
            if (ModelState.IsValid)
            {
                if (AuthorImage != null)
                {
                    WebImage img = new WebImage(AuthorImage.InputStream);
                    FileInfo imginfo = new FileInfo(AuthorImage.FileName);

                    string logoname = Guid.NewGuid().ToString() + imginfo.Extension;
                    img.Resize(500, 500);
                    img.Save("~/Uploads/AuthorImage/" + logoname);

                    t.AuthorImage = "/Uploads/AuthorImage/" + logoname;
                }
                db.Authors.Add(t);
                db.SaveChanges();
                return RedirectToAction("AuthorList");
            }
            AuthorValidator authorValidator = new AuthorValidator();
            ValidationResult results = authorValidator.Validate(t);
            if (results.IsValid)
            {
                authormanager.TAdd(t);
                return RedirectToAction("AuthorList");
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
        [HttpGet]
        public ActionResult AuthorEdit(int id)
        {
            Author author = authormanager.GetByID(id);
            return View(author);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AuthorEdit([Bind(Include = "AuthorID,AuthorName,AuthorImage,AuthorAbout,AuthorTitle,AboutShort,Mail,Password,PhoneNumber,Blogs")]int? id, Author t, HttpPostedFileBase AuthorImage)
        {
         
            if (ModelState.IsValid)
            {
               
                var h = db.Authors.Where(x => x.AuthorID == id).SingleOrDefault();
                if (AuthorImage != null)
                {
                
                    WebImage img = new WebImage(AuthorImage.InputStream);
                    FileInfo imginfo = new FileInfo(AuthorImage.FileName);

                    string hizmetname = Guid.NewGuid().ToString() + imginfo.Extension;
                    img.Resize(500, 500);
                    img.Save("~/Uploads/AuthorImage/" + hizmetname);

                    h.AuthorImage = "/Uploads/AuthorImage/" + hizmetname;
                }
                authormanager.TUpdate(t);

                db.SaveChanges();
                return RedirectToAction("AuthorList");
            }
            return View(t);
        }
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var h = db.Authors.Find(id);
            if (h == null)
            {
                return HttpNotFound();
            }
            db.Authors.Remove(h);
            db.SaveChanges();
            return RedirectToAction("AuthorList");

        }
    }
}