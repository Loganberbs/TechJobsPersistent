﻿using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TechJobsPersistent.Models;
using TechJobsPersistent.ViewModels;
using TechJobsPersistent.Data;
using Microsoft.EntityFrameworkCore;


namespace TechJobsPersistent.Controllers
{
    
    public class HomeController : Controller
    {
        
        private JobDbContext context;

                public HomeController(JobDbContext dbContext)
                {
                    context = dbContext;
                }

                public IActionResult Index()
                {
                    List<Job> jobs = context.Jobs.Include(j => j.Employer).ToList();

                    return View(jobs);
                }

       [HttpGet("/Add")]
                
        public IActionResult AddJob()
                {
                    List<Employer> allEmployers = context.Employers.ToList();
                    List<Skill> allSkills = context.Skills.ToList();
                    AddJobViewModel addJobViewModel = new AddJobViewModel(allEmployers, allSkills);
                    return View(addJobViewModel);
                }

        [HttpPost]
               
        public IActionResult ProcessAddJobForm(AddJobViewModel addJobViewModel, string[] selectedSkills)
                {
                    if (ModelState.IsValid)
                    {
                        Job newJob = new Job
                        {
                            Name = addJobViewModel.Name,
                            Employer = context.Employers.Find(addJobViewModel.EmployerId),
                            EmployerId = addJobViewModel.EmployerId,
                        };

                        foreach (var item in selectedSkills)
                        {
                            int skillId = int.Parse(item);
                            int jobId = newJob.Id;

                            List<JobSkill> existingTableItems = context.JobSkills
                                 .Where(js => js.JobId == jobId)
                                .Where(js => js.SkillId == skillId)
                                .ToList();

                            if (existingTableItems.Count == 0)
                            {
                                JobSkill jobSkill = new JobSkill
                                {
                                    JobId = jobId,
                                    Job = newJob,
                                    SkillId = skillId,
                                    Skill = context.Skills.Find(skillId)
                                };
                                context.JobSkills.Add(jobSkill);
                            }
                        }
                        context.Jobs.Add(newJob);
                        context.SaveChanges();

                        return Redirect("/Home");
                    }

                    return View(addJobViewModel);
                }

        public IActionResult Detail(int id)
        {
            Job theJob = context.Jobs
                .Include(j => j.Employer)
                .Single(j => j.Id == id);

            List<JobSkill> jobSkills = context.JobSkills
                .Where(js => js.JobId == id)
                .Include(js => js.Skill)
                .ToList();

            JobDetailViewModel viewModel = new JobDetailViewModel(theJob, jobSkills);
            return View(viewModel);
        }
    }
}
