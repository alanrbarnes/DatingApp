using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize] 

    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        //Endpoint to get to all users in the database
        //Endpoint to get to a specific user in the database
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDtos>>> GetUsers() //public ActionResult<IEnumerable<AppUser>> GetUsers() //public ActionResult<List<AppUser>> GetUsers() 
        {
            var users = await _userRepository.GetMembersAsync();
            
            return Ok(users);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<MemberDtos>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }
    }
}