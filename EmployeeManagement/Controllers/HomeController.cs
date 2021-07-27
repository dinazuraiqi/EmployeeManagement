using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private Models.IEmployeeRepository _employeeRepository;
        private IHostingEnvironment hostingEnvironment;

        public HomeController(IEmployeeRepository employeeRepository,
                              IHostingEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous]
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees();
            return View(model);
        }
        [AllowAnonymous]
        public ViewResult Details(int? id)
        {
            //throw new Exception("Error in Details View");
            Employee employee = _employeeRepository.GetEmployee(id ?? 1);

            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id);
            }
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = _employeeRepository.GetEmployee(id ?? 1),
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {

               
                   string PhotoPath= ProcessUploadedFile(model);
                
                    Employee newEmployee = new Employee
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Department = model.Department,
                        //PhotoPath = uniqueFileName
                        PhotoPath = PhotoPath
                    };

                    _employeeRepository.Add(newEmployee);
                    return RedirectToAction("Details", new { id = newEmployee.Id });
                              
            }
            return View();
        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);                
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if (model.Photo != null)
                {// If a new photo is uploaded, the existing photo must be
                 // deleted. So check if there is an existing photo and delete
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    // Save the new photo in wwwroot/images folder and update
                    // PhotoPath property of the employee object which will be
                    // eventually saved in the database
                    employee.PhotoPath = ProcessUploadedFile(model);
                }                    

                    _employeeRepository.Update(employee);
                    return RedirectToAction("index");
                
            }
            return View();
        }

        string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                
                //// The image must be uploaded to the images folder in wwwroot
                //// To get the path of the wwwroot folder we are using the inject
                //// HostingEnvironment service provided by ASP.NET Core
                //string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                //// To make sure the file name is unique we are appending a new
                //// GUID value and and an underscore to the file name
                //uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                //string filePath = Path.Combine(uploadsFolder, uniqueFileName);                  
                //// Use CopyTo() method provided by IFormFile interface to
                //// copy the file to wwwroot/images folder
                //model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));


                var path = Path.Combine(hostingEnvironment.WebRootPath, "images");
                var stream = new FileStream(path, FileMode.Create);
                model.Photo.CopyToAsync(stream);               

            }
            return uniqueFileName;
        }

        [HttpGet]
        public ViewResult Delete(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeDeleteViewModel employeeDeleteViewModel = new EmployeeDeleteViewModel
            {
                Employee=employee
            };
            return View(employeeDeleteViewModel);
        }

        [HttpPost]
        public IActionResult Delete(EmployeeDeleteViewModel model)
        {
            if (model.Employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound");
            }           
            
            _employeeRepository.Delete(model.Employee.Id);
            return RedirectToAction("index");                       
           
        }


    }
}
