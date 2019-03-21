using PagedList;
using StocksDecision.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace StocksDecision.Controllers
{
    [Authorize]
    public class BlogsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Blogs
        public ViewResult Index(string searchString, int? page)
        {
            IPagedList<Blog> bloglist = null;
            ViewBag.Search = searchString;
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var blogs = from b in db.Blogs
                         select b;
            if (!String.IsNullOrEmpty(searchString))
            {
                blogs = blogs.Where(b => b.Message.Contains(searchString) || b.User.Contains(searchString))
                   .OrderByDescending(b => b.Id);
                bloglist = blogs.ToPagedList(pageIndex, pageSize);
                return View(bloglist);
            }
            else
            {
                blogs = blogs.OrderByDescending(b => b.Id);
                bloglist = blogs.ToPagedList(pageIndex, pageSize);
                return View(bloglist);
            }

        }

        // GET: Stocks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            ViewBag.CurrentUser = User.Identity.Name;
            return View(blog);
        }

        // GET: Blogs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Message")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.CreatedOn = DateTime.UtcNow;
                blog.User = User.Identity.Name.ToString();
                db.Blogs.Add(blog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blog);
        }

        // GET: Blogs/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Blogs/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Blogs/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Blogs/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //[ChildActionOnly]
        public ActionResult Comment(int id)
        {
            Blog blog = db.Blogs.Find(id);
            return PartialView(blog);
        }

        // POST: Blogs/Create
        [HttpPost]
        public void PostComment(int id, string message) //[Bind(Include = "Id,Message")] Comment comment)
        {
            Blog blog = db.Blogs.Find(id);
            if (ModelState.IsValid && !string.IsNullOrEmpty(message))
            {
                Comment comment = new Comment
                {
                    CreatedOn = DateTime.UtcNow,
                    Message = message,
                    User = User.Identity.Name.ToString(),
                };
                blog.Comments.Add(comment);
                db.Entry(blog).State = EntityState.Modified;
                db.SaveChanges();
            }

            //return PartialView("Comments", blog);
        }

    }
}
