using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

	    private readonly DataContext _context;
		public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
		{
            _mapper = mapper;
            _tokenService = tokenService;
		    _context = context;
		}


        [HttpPost("register")]
		public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
		{
			//user created
			if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
			//Get Properties from registerDto, mapped into AppUser Object
			var user = _mapper.Map<AppUser>(registerDto);

		    using var hmac = new HMACSHA512();
			//hover over method and press F12 to go to implementation
			//by including using statment garbage collection is garanteed
			
			//put user in lowercase
			user.UserName = registerDto.Username.ToLower();
			//work out password hash and salt
			user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
			user.PasswordSalt = hmac.Key;
			
			_context.Users.Add(user);  //tracks the user
			await _context.SaveChangesAsync();  //adds the user to the database
			
			return new UserDto
			{
				Username = user.UserName,
				Token = _tokenService.CreateToken(user),
				KnownAs = user.knownAs
			};
		}

		[HttpPost("Login")]
		public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
		{
			//var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);
			var user = await _context.Users
			    .Include(p => p.Photos)
			    .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

			if (user == null) return Unauthorized("Invalid username");

			using var hmac = new HMACSHA512(user.PasswordSalt);

			var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

			for (int i = 0; i < computedHash.Length; i++)
			{
				if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
			}

			return new UserDto
			{
				Username = user.UserName,
				Token = _tokenService.CreateToken(user),
				PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
				KnownAs = user.knownAs
			};
		}



		private async Task<bool> UserExists(string username) 
		{
			return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
		}
    }
}