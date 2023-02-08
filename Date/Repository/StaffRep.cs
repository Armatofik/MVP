﻿using Microsoft.EntityFrameworkCore;
using MVP.Date.Interfaces;
using MVP.Date.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Date.Repository
{
    public class StaffRep : IStaff
    {
        private readonly AppDB _appDB;

        public StaffRep(AppDB appDB)
        {
            _appDB = appDB;
        }

        public IEnumerable<Staff> AllStaffs => _appDB.DBStaff;

        public IEnumerable<Staff> DivisoinStaff(int divisionId) => _appDB.DBStaff.Where(p => p.divisionId == divisionId);// incl

        public Staff GetStaff(int staffId) => _appDB.DBStaff.FirstOrDefault(p => p.id == staffId);

        public List<Staff> StaffTable(string SessionRole, string sessionCod)
        {
            List<Staff> StaffTable = new List<Staff>();
            switch (SessionRole)
            {
                case "Директор":
                    foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R02"))
                    {
                        StaffTable.Add(staffs);
                    }
                    foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R04"))
                    {
                        StaffTable.Add(staffs);
                    }
                    break;
                case "ГИП":
                    foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R03"))
                    {
                        StaffTable.Add(staffs);
                    }
                    foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R04"))
                    {
                        StaffTable.Add(staffs);
                    }
                    break;
                case "Помощник ГИПа":
                    foreach (var staffs in _appDB.DBStaff.Where(p => p.roleCod == "R04"))
                    {
                        StaffTable.Add(staffs);
                    }
                    break;
                case "НО":
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
                    break;
                case "РГ":
                    foreach (var staffs in _appDB.DBStaff.Where(p => p.supervisorCod == sessionCod && p.roleCod == "R06"))
                    {
                        StaffTable.Add(staffs);
                    }
                    break;
            }
            return StaffTable;
        }
    }
}
