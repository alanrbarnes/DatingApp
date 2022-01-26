using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using API.Extensions;

namespace API.Controllers
{
    [Authorize] 

    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
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
        
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDtos>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();
            var user = await _userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            //Get the user
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            //Variable for results
            var result = await _photoService.AddPhotoAsync(file);

            //Check the error
            if (result.Error != null) return BadRequest(result.Error.Message);

            //Create a new photo
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            //Check to see if the user has any photos at the moment
            if (user.Photos.Count == 0) //is this the first photo
            {
                photo.IsMain = true;
            }

            //add the photo 
            user.Photos.Add(photo);

            //return the photo
            if (await _userRepository.SaveAllAsync())
            {
                //return 201
                return CreatedAtRoute("GetUser", new {username = user.UserName} , _mapper.Map<PhotoDto>(photo));
            }
               

            //if failed return a bad request
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain != null) currentMain.IsMain = false; //turn one photo off
            photo.IsMain = true; //turn this photo on

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            //get user object
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            //get photo interested in deleting
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            //check to see if photo is null
            if (photo == null) return NotFound();

            //check to see if photo is main
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            //Check to see if we have a public id for this image
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            //remove photo from database
            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
        }

    }
}