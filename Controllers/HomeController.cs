﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVP.Date;
using MVP.Date.Interfaces;
using MVP.Date.Models;
using MVP.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskStatus = MVP.Date.Models.TaskStatus;

namespace MVP.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDB _appDB;
        private readonly IPost _post;
        private readonly IRole _role;
        private readonly ITask _task;
        private readonly IProject _project;
        private readonly ILogistickTask _logistickTask;
        private readonly ILogisticProject _logistickProject;
        private readonly IStaff _staff;

        public HomeController(IPost post, IRole role,ITask task, IProject project, AppDB appDB, ILogistickTask logistick, IStaff staff, ILogisticProject logistickProject)
        {
            _post = post;
            _role = role;
            _task = task;
            _project = project;
            _appDB = appDB;
            _logistickTask = logistick;
            _staff = staff;
            _logistickProject = logistickProject;
        }

        public RedirectToActionResult RedactSatusTask(int id, string stat,int activTable, string staffTableFilter,
            string recipientProjectFilter,
            string supervisorProjectFilter,
            string porjectFiltr)
        {
            
            if (!_task.redactStatus(id, stat))
            {
                var msg = "Только одна задача может быть в работе! Проверьте статусы своих задачь!";
                return RedirectToAction("TaskTable", new { activTable = activTable, Taskid = id, meesage = msg, TaskRed = true,
                    staffTableFilter = staffTableFilter,
                    recipientProjectFilter = recipientProjectFilter,
                    supervisorProjectFilter = supervisorProjectFilter,
                    porjectFiltr = porjectFiltr
                });
            }
            var roleSession = new SessionRoles();
            try
            {
                roleSession = JsonConvert.DeserializeObject<SessionRoles>(HttpContext.Session.GetString("Session"));
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Index");
            }
            var supervisor = _appDB.DBTask.FirstOrDefault(p => p.id == id).supervisor;
            LogistickTask item = new LogistickTask()
            {
                ProjectCode = _appDB.DBTask.FirstOrDefault(p => p.id == id).projectCode,
                TaskId = id,
                descTask = _appDB.DBTask.FirstOrDefault(p => p.id == id).desc,
                supervisorId = _appDB.DBStaff.FirstOrDefault(p => p.name == supervisor).id,
                resipienId = supervisor != null ? _appDB.DBStaff.FirstOrDefault(p => p.name == supervisor).id : -1,
                dateRedaction = DateTime.Now,
                planedTime = _appDB.DBTask.FirstOrDefault(p => p.id == id).plannedTime,
                actualTime = new TimeSpan(),
                CommitorId = _appDB.DBStaff.FirstOrDefault(p => p.name == roleSession.SessionName).id,
                taskStatusId = _appDB.DBTaskStatus.FirstOrDefault(p => p.name == _appDB.DBTask.FirstOrDefault(p => p.id == id).status).id,
                comment = $"Стату задачи изменен на: {stat}"
            };
            _logistickTask.addToDB(item);

            return RedirectToAction("TaskTable", new { activTable = activTable,
                staffTableFilter = staffTableFilter,
                recipientProjectFilter = recipientProjectFilter,
                supervisorProjectFilter = supervisorProjectFilter,
                porjectFiltr = porjectFiltr
            });
        }

        public RedirectToActionResult RedactProjectToDB(
            int iid,
            string arhive,
            string link,
            string supervisor,
            int priority,
            string allStages,
            string comment,
            int activTable,
            string staffTableFilter,
            string recipientProjectFilter,
            string supervisorProjectFilter,
            string porjectFiltr
            )
        {
            _project.redactToDB(iid, arhive,link,supervisor,priority,allStages);

            var roleSession = new SessionRoles();
            try
            {
                roleSession = JsonConvert.DeserializeObject<SessionRoles>(HttpContext.Session.GetString("Session"));
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Index");
            }
            LogisticProject item = new LogisticProject()
            {
                arhive = arhive,
                projectId = iid,
                link = link,
                supervisor = supervisor,
                priority = priority,
                allStages = allStages,
                CommitorId = _appDB.DBStaff.FirstOrDefault(p => p.name == roleSession.SessionName).id,
                dateRedaction = DateTime.Now,
                comment = comment
            };

            _logistickProject.addToDB(item);
            return RedirectToAction("TaskTable", new { activTable = activTable ,Projid = iid,
                staffTableFilter = staffTableFilter,
                recipientProjectFilter = recipientProjectFilter,
                supervisorProjectFilter = supervisorProjectFilter,
                porjectFiltr = porjectFiltr
            });

        }
        public async Task<RedirectToActionResult> RedactTaskToDB(
            int iid,
            DateTime date,
            string status,
            string comment,
            string supervisor,
            string recipient,
            int pririty,
            TimeSpan plannedTime,
            DateTime start,
            DateTime finish,
            int activTable,
            string staffTableFilter,
            string recipientProjectFilter,
            string supervisorProjectFilter,
            string porjectFiltr
)
        {
            var roleSession = new SessionRoles();
            try
            {
                roleSession = JsonConvert.DeserializeObject<SessionRoles>(HttpContext.Session.GetString("Session"));
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Index");
            }
            if (!_task.redactToDB(iid, date, status, comment != null ? $"{roleSession.SessionName}: {comment}\n" : null, supervisor, recipient, pririty, plannedTime, start, finish))
            {
                var msg = "Только одна задача может быть в работе! Проверьте статусы своих задачь!";
                return RedirectToAction("TaskTable", new { activTable = activTable, Taskid = iid, meesage = msg, TaskRed = true,
                    staffTableFilter = staffTableFilter,
                    recipientProjectFilter = recipientProjectFilter,
                    supervisorProjectFilter = supervisorProjectFilter,
                    porjectFiltr = porjectFiltr
                });
            }
            else
            { 
                var projCod = _appDB.DBTask.FirstOrDefault(p => p.id == iid).projectCode;
                var projId = _appDB.DBProject.FirstOrDefault(p => p.code == projCod) != null ? _appDB.DBProject.FirstOrDefault(p => p.code == projCod).id : -1;
                _project.NextStage(projId);

                
                LogistickTask item = new LogistickTask()
                {
                    ProjectCode = _appDB.DBTask.FirstOrDefault(p => p.id == iid).projectCode,
                    TaskId = iid,
                    descTask = _appDB.DBTask.FirstOrDefault(p => p.id == iid).desc,
                    supervisorId = _appDB.DBStaff.FirstOrDefault(p => p.name == supervisor).id,
                    resipienId = supervisor != null? _appDB.DBStaff.FirstOrDefault(p => p.name == supervisor).id : -1,
                    dateRedaction = DateTime.Now,
                    planedTime = plannedTime,
                    actualTime = new TimeSpan(),
                    CommitorId = _appDB.DBStaff.FirstOrDefault(p => p.name == roleSession.SessionName).id,
                    taskStatusId = _appDB.DBTaskStatus.FirstOrDefault(p => p.name == status).id,
                    comment = comment
                };
                _logistickTask.addToDB(item);
                if (status == "Создана") await TimerPauseTask(iid);
                return RedirectToAction("TaskTable", new { activTable = activTable, Taskid = iid,
                    staffTableFilter = staffTableFilter,
                    recipientProjectFilter = recipientProjectFilter,
                    supervisorProjectFilter = supervisorProjectFilter,
                    porjectFiltr = porjectFiltr
                });
            }

        }

        public async Task TimerPauseTask(int idTask)
        {
            await Task.Delay(43200000);
            Tasks el =  _appDB.DBTask.FirstOrDefault(p => p.id == idTask);
            el.status = "В работе";
            await _appDB.SaveChangesAsync();
        }

        public RedirectToActionResult addProjectToDB(
            string link,
            string code,
            string supervisor,
            int priority,
             DateTime plannedFinishDate,
            string shortName,
            string name,
            string allStages,
            int activTable,
            string staffTableFilter,
            string recipientProjectFilter,
            string supervisorProjectFilter,
            string porjectFiltr
            )
        {
            var roleSession = new SessionRoles();
            try
            {
                roleSession = JsonConvert.DeserializeObject<SessionRoles>(HttpContext.Session.GetString("Session"));
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Index");
            }
            var item = new Project
            {
                code = code,
                name = name,
                shortName = shortName,
                priority = priority,
                dateStart = DateTime.Now,
                plannedFinishDate = plannedFinishDate,
                supervisor = supervisor,
                link = link,
                archive = "Нет",
                nowStage = allStages == null? "" : allStages.Split(',')[0],
                allStages = allStages,
                history = $"{DateTime.Now} - Проект создан"
            };

            LogisticProject log = new LogisticProject()
            {
                arhive = "Нет",
                projectId = item.id,
                link = link,
                supervisor = supervisor,
                priority = priority,
                allStages = allStages,
                CommitorId = _appDB.DBStaff.FirstOrDefault(p => p.name == roleSession.SessionName).id,
                dateRedaction = DateTime.Now,
                comment = "Проект создан"
            };
            _logistickProject.addToDB(log);
            _project.addToDB(item);
            return RedirectToAction("TaskTable", new { activTable = activTable,
                staffTableFilter = staffTableFilter,
                recipientProjectFilter = recipientProjectFilter,
                supervisorProjectFilter = supervisorProjectFilter,
                porjectFiltr = porjectFiltr
            });
        }

        public RedirectToActionResult addTaskToDB(
            string code,
            string desc,
            string projectCode,
            string supervisor,
            string recipient,
            string comment,
            TimeSpan plannedTime,
            DateTime date,
            string Stage,
            string liteTask,
            int activTable,
            string staffTableFilter,
            string recipientProjectFilter,
            string supervisorProjectFilter,
            string porjectFiltr
            )
        {
            var roleSession = new SessionRoles();
            try
            {
                roleSession = JsonConvert.DeserializeObject<SessionRoles>(HttpContext.Session.GetString("Session"));
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Index");
            }

            try
            {
                if (projectCode != _appDB.DBProject.FirstOrDefault(p => p.code == projectCode).code)
                {
                    return RedirectToAction("TaskTable", new { activTable = activTable, meesage = "Не коррестный код проекта!",
                        staffTableFilter = staffTableFilter,
                        recipientProjectFilter = recipientProjectFilter,
                        supervisorProjectFilter = supervisorProjectFilter,
                        porjectFiltr = porjectFiltr
                    });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("TaskTable", new { activTable = activTable, meesage = "Не коррестный код проекта!",
                    staffTableFilter = staffTableFilter,
                    recipientProjectFilter = recipientProjectFilter,
                    supervisorProjectFilter = supervisorProjectFilter,
                    porjectFiltr = porjectFiltr
                });
            }

            if (plannedTime == TimeSpan.Zero)
            {
                return RedirectToAction("TaskTable", new { activTable = activTable, meesage = "Не указан срок исполнения задачи!",
                    staffTableFilter = staffTableFilter,
                    recipientProjectFilter = recipientProjectFilter,
                    supervisorProjectFilter = supervisorProjectFilter,
                    porjectFiltr = porjectFiltr
                });
            }

            TimeSpan SumTimeTaskToDay = plannedTime;
            var maxPriority = 0;
            var tasksSuper = _appDB.DBTask.Where(p => p.supervisor == supervisor).Where(p => p.date.Date == date.Date).OrderBy(p => p.plannedTime);
            foreach (var task in tasksSuper)
            {
                SumTimeTaskToDay += task.plannedTime;
                maxPriority = task.priority > maxPriority ? task.priority : maxPriority;
            }
            if(SumTimeTaskToDay > new TimeSpan(8, 0, 0))
            {
                var exit = false;
                if(_appDB.DBProject.FirstOrDefault(p => p.code == projectCode).priority < maxPriority || liteTask != "Задача")
                {
                    Tasks RedTask = new Tasks();
                    foreach(var task in tasksSuper)
                    {
                        if (SumTimeTaskToDay - task.plannedTime <= new TimeSpan(8, 0, 0) && (task.priority < maxPriority || liteTask != "Задача") )
                        {
                            exit = true;
                            RedTask = task;
                            comment += $" + Задача {task.desc} перенесена на {task.date.Date.ToString(@"dd\.MM\.yyyy")}, в связи с более низким приоритетом.\n";
                            break;
                        }
                    }
                    if (RedTask != new Tasks())
                    {
                        _task.redactToDB(RedTask.id, RedTask.date.AddDays(1), RedTask.status, RedTask.comment != null ? $"{roleSession.SessionName}: {comment}\n" : null, RedTask.supervisor, RedTask.recipient, RedTask.priority, RedTask.plannedTime, RedTask.start, RedTask.finish);
                    }

                }

                if (!exit)
                {
                    return RedirectToAction("TaskTable", new
                    {
                        activTable = activTable,
                        meesage = "Сумма времени задач на этот день превышает 8 часов!\nПриоритет текущей задачи не позволяет сместить остальные.\nВыберите другой день.",
                        staffTableFilter = staffTableFilter,
                        recipientProjectFilter = recipientProjectFilter,
                        supervisorProjectFilter = supervisorProjectFilter,
                        porjectFiltr = porjectFiltr
                    });
                }
            }
            var item = new Tasks
            {
                actualTime = TimeSpan.Zero,
                desc = desc,
                projectCode = projectCode,
                supervisor = supervisor,
                recipient = recipient,
                priority = liteTask == "Задача" ? _appDB.DBProject.FirstOrDefault(p => p.code == projectCode).priority : 0,
                comment = comment != null ? $"{roleSession.SessionName}: {comment}\n" : null,
                plannedTime = plannedTime,
                date = date,
                Stage = Stage,
                status = "Создана",
                liteTask = liteTask == "Задача" ? false : true,
                creator = roleSession.SessionName

            };
            _task.addToDB(item);

            var iid = _appDB.DBTask.FirstOrDefault(p => p.desc == desc).id;
            LogistickTask log = new LogistickTask()
            {
                ProjectCode = _appDB.DBTask.FirstOrDefault(p => p.id == iid).projectCode,
                TaskId = iid,
                descTask = _appDB.DBTask.FirstOrDefault(p => p.id == iid).desc,
                supervisorId = _appDB.DBStaff.FirstOrDefault(p => p.name == supervisor).id,
                resipienId = supervisor != null ? _appDB.DBStaff.FirstOrDefault(p => p.name == supervisor).id : -1,
                dateRedaction = DateTime.Now,
                planedTime = plannedTime,
                actualTime = new TimeSpan(),
                CommitorId = _appDB.DBStaff.FirstOrDefault(p => p.name == roleSession.SessionName).id,
                taskStatusId = _appDB.DBTaskStatus.FirstOrDefault(p => p.name == "Создана").id,
                comment = "Задача создана. Комментарий: " + comment
            };
            _logistickTask.addToDB(log);


            return RedirectToAction("TaskTable", new { activTable = activTable, staffTableFilter = staffTableFilter, 
                recipientProjectFilter= recipientProjectFilter,
                supervisorProjectFilter = supervisorProjectFilter,
                porjectFiltr = porjectFiltr});

        }



        
        public ViewResult TaskTable(int Taskid = -1, int Projid = -1, string meesage = "",
            bool TaskRed = false, bool ProjectRed = false, bool ProjectCreate = false,
             string porjectFiltr = "", string supervisorProjectFilter ="",
             string recipientProjectFilter ="", string staffTableFilter ="", int activTable = 0 )
        {
            var roleSession = new SessionRoles();
            var sessionCod = "";
            try
            {
                roleSession = JsonConvert.DeserializeObject<SessionRoles>(HttpContext.Session.GetString("Session"));
                sessionCod = _appDB.DBStaff.FirstOrDefault(p => p.name == roleSession.SessionName).code;
            }
            catch (Exception)
            {
                return View(new HomeViewModel());
            }



            

            List<Staff> StaffTable = new List<Staff>();
            if (roleSession.SessionRole == "Директор")
            {
                foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R02"))
                {
                    StaffTable.Add(staffs);
                }
                foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R04"))
                {
                    StaffTable.Add(staffs);
                }
            }
            else if (roleSession.SessionRole == "ГИП")
            {
                foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R03"))
                {
                    StaffTable.Add(staffs);
                }
                foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R04"))
                {
                    StaffTable.Add(staffs);
                }
            }
            else if (roleSession.SessionRole == "Помощник ГИПа")
            {
                foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R04"))
                {
                    StaffTable.Add(staffs);
                }
            }
            else if (roleSession.SessionRole == "НО")
            {
                foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R02"))
                {
                    StaffTable.Add(staffs);
                }

                foreach (var staffs in _appDB.DBStaff.Where(p => p.supervisorCod == sessionCod && p.roleCod == "R05"))
                {
                    StaffTable.Add(staffs);
                }
                foreach (var staff1 in _appDB.DBStaff.Where(p => p.supervisorCod == sessionCod && p.roleCod == "R06"))
                {
                    StaffTable.Add(staff1);
                }
            }
            else if (roleSession.SessionRole == "РГ")
            {
                foreach (var staffs in _appDB.DBStaff.Where(p => p.supervisorCod == sessionCod && p.roleCod == "R06"))
                {
                    StaffTable.Add(staffs);
                }
            }

            var projects = _project.AllProjects;
            var tasks = _task.AllTasks;
            var staff = StaffTable;

            List<string> staffNames = new List<string>();
            foreach (var task in staff)
            {
                if (!staffNames.Contains(task.name)) staffNames.Add(task.name);
            }
            List<Tasks> taskStaffTable = tasks.Where(p => staffNames.Contains(p.supervisor) || staffNames.Contains(p.recipient)).ToList();



            List<string> ollGip = new List<string>();
            foreach(Project proj in projects.OrderBy(p => p.supervisor))
            {
                if(!ollGip.Contains(proj.supervisor)) ollGip.Add(proj.supervisor);
            }

            //////  настроить прием фильтров
            foreach(var filter in staffTableFilter.Split(','))
            {
                if (filter != "" && filter != "Все должности")
                {
                    staff = StaffTable.Where(p => p.post == filter).ToList();
                }
            }
            

            foreach(var filter in porjectFiltr.Split(','))
            {
                if (filter == "Проекты в архиве")
                {
                    projects = projects.Where(p => p.archive == "Да");
                }
                if (filter == "Текущие проекты")
                {
                    projects = projects.Where(p => p.archive == "Нет");
                }
            }


            foreach (var filter in supervisorProjectFilter.Split(','))
            {
                if (filter != "Все ГИПы" && filter != "")
                {
                    projects = projects.Where(p => p.supervisor == filter);
                }
            }


            foreach (var filter in recipientProjectFilter.Split(','))
            {
                if (filter != "Все ответственные" && filter != "")
                {
                    tasks = tasks.Where(p => p.supervisor == filter);
                }
            }

            //////////

            List<string> projCod = new List<string>();
            foreach (Project proj in projects)
            {
                if (!projCod.Contains(proj.code)) projCod.Add(proj.code);
            }
            tasks = tasks.Where(p => projCod.Contains(p.projectCode));
            

            string table1 = "";
            string table2 = "";
            string table3 = "";
            string table4 = "";
            switch (activTable)
            {
                case 0:
                    table1 = "block";
                    table2 = "none";
                    table3 = "none";
                    table4 = "none";
                    break;
                case 1:
                    table1 = "none";
                    table2 = "block";
                    table3 = "none";
                    table4 = "none";
                    break;
                case 2:
                    table1 = "none";
                    table2 = "none";
                    table3 = "block";
                    table4 = "none";
                    break;
                case 3:
                    table1 = "none";
                    table2 = "none";
                    table3 = "none";
                    table4 = "block";
                    break;
            }

            HomeViewModel homeTasks = new HomeViewModel
            {
                session = roleSession,
                roles = _role.AllRoles,
                posts = _post.AllPosts,
                filterStaff = staffTableFilter == "" ? "Все должности" : staffTableFilter,
                filterProj = porjectFiltr == "" ? "Все проекты" : porjectFiltr,
                filterSupProj = supervisorProjectFilter == "" ? "Все ГИПы" : supervisorProjectFilter,
                filterResProj = recipientProjectFilter == "" ? "Все ответственные" : recipientProjectFilter,

                projectTasks = tasks,
                projects = projects,
                projectId = Projid,
                redactedProject = _project.GetProject(Projid),

                tasks = _task.AllTasks,
                taskId = Taskid,
                redactedTask = _task.GetTask(Taskid),

                staffSess = new Staff(),
                staffs = _staff.AllStaffs,
                staffsTable = staff,
                staffTasks = taskStaffTable,

                ProjectCreate = ProjectCreate,
                TaskRed = TaskRed,
                ProjectRed = ProjectRed,

                nullProject = new Project(),
                nullTask = new Tasks(),
                nullStaff = new Staff(),

                
                activeTable1 = table1,
                activeTable2 = table2,
                activeTable3 = table3,
                activeTable4 = table4,

                activeTableIndex = activTable,

                ollGip = ollGip

            };
            ViewBag.RoleCod = _appDB.DBStaff.FirstOrDefault(p => p.name == roleSession.SessionName).code;
            ViewBag.Role = roleSession.SessionRole;
            ViewBag.RoleName = roleSession.SessionName;
            ViewBag.ErrorMassage = meesage;
            return View(homeTasks);
        }
    }
    
}
