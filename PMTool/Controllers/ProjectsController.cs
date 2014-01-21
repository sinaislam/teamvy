using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PMTool.Models;
using PMTool.Repository;
using System.Web.Security;

namespace PMTool.Controllers
{
    public class ProjectsController :BaseController
    {
        private UnitOfWork unitOfWork = new UnitOfWork();
        //
        // GET: /Projects/

        public ViewResult Index()
        {
            TempData["Message"] = TempData["Message"];
            return View(unitOfWork.ProjectRepository.AllbyUserIncluding((Guid)Membership.GetUser(User.Identity.Name).ProviderUserKey, project => project.Users).ToList());
        }

        //
        // GET: /Projects/Details/5

        public ViewResult Details(long id)
        {
            Project project = unitOfWork.ProjectRepository.Find(id);
            return View(project);
        }

        //
        // GET: /Projects/Create

        public ActionResult Create()
        {
            List<SelectListItem> allUsers = GetAllUser();
            ViewBag.PossibleUsers = allUsers;
            return View();
        }

        private List<SelectListItem> GetAllUser()
        {
            List<SelectListItem> allUsers = new List<SelectListItem>();
            List<User> userList = unitOfWork.UserRepository.All();
            foreach (User user in userList)
            {
                SelectListItem item = new SelectListItem { Value = user.UserId.ToString(), Text = user.FirstName + user.LastName };
                allUsers.Add(item);
            }
            return allUsers;
        }

        //
        // POST: /Projects/Create

        [HttpPost]
        public ActionResult Create(Project project)
        {
            project.ModificationDate = DateTime.Now;
            project.CreateDate = DateTime.Now;
            project.ActionDate = DateTime.Now;
            project.CreatedBy = (Guid)Membership.GetUser().ProviderUserKey;
            project.ModifieddBy = (Guid)Membership.GetUser().ProviderUserKey;

            if (ModelState.IsValid)
            {
                unitOfWork.ProjectRepository.InsertOrUpdate(project);
                AddAssignUser(project);
                unitOfWork.Save();
                
                return RedirectToAction("Index");
            }
            List<SelectListItem> allUsers = GetAllUser();
            ViewBag.PossibleUsers = allUsers;
            return View(project);
        }

        private void AddAssignUser(Project project)
        {
            project.Users = new List<User>();
            if (project.SelectedAssignedUsers != null)
            {
                foreach (string userID in project.SelectedAssignedUsers)
                {
                    User user = unitOfWork.UserRepository.GetUserByUserID(new Guid(userID));
                    project.Users.Add(user);

                }
            }
        }

        //
        // GET: /Projects/Edit/5

        public ActionResult Edit(long id)
        {
            Project project = unitOfWork.ProjectRepository.Find(id);
            List<SelectListItem> allUsers = GetAllUser();
            ViewBag.PossibleUsers = allUsers;
            project.SelectedAssignedUsers = project.Users.Select(u => u.UserId.ToString()).ToList();
            return View(project);
        }

        //
        // POST: /Projects/Edit/5

        [HttpPost]
        public ActionResult Edit(Project project)
        {
            project.ModificationDate = DateTime.Now;
            project.ActionDate = DateTime.Now;
            project.ModifieddBy = (Guid)Membership.GetUser().ProviderUserKey;
            List<User> userList = new List<User>();
            if (ModelState.IsValid)
            {
                AddAssignUser(project);
               userList= unitOfWork.ProjectRepository.InsertOrUpdate(project);
                unitOfWork.Save();
                string msg = "";
                foreach (User user in userList)
                {
                    msg = msg + user.FirstName + " " + user.LastName + ", ";
                }
                msg = msg + " user(s) are assigned in one or more task in this project;";
                if (msg != string.Empty)
                {
                    TempData["Message"]= msg;
                }
                return RedirectToAction("Index");
                
            }
            List<SelectListItem> allUsers = GetAllUser();
            ViewBag.PossibleUsers = allUsers;
           // project.SelectedAssignedUsers = project.Users.Select(u => u.UserId.ToString()).ToList();
            return View(project);

        }

        //
        // GET: /Projects/Delete/5

        public ActionResult Delete(long id)
        {
            Project project = unitOfWork.ProjectRepository.Find(id);
            return View(project);
           
        }

        //
        // POST: /Projects/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            unitOfWork.ProjectRepository.Delete(id);
            unitOfWork.Save();
            return RedirectToAction("Index");

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}