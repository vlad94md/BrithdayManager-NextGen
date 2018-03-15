using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using BirthdayManager.Core.Constants;
using BirthdayManager.Core.Models;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers.Api
{
    public class UsersController : ApiController
    {
        private ApplicationDbContext _context;

        public UsersController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpGet]
        //GET /api/users
        public IEnumerable<UserDto> GetUsers(string query = null)
        {
            var  usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                usersQuery = usersQuery.Where(c => (c.FirstName + " " + c.LastName).Contains(query));
            }

            var customersDto = usersQuery
                .ToList()
                .Select(Mapper.Map<ApplicationUser, UserDto>);
            
            return customersDto;
        }
    }
}
