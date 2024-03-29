﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using LaboratoryActivityAPI.Models;
using LaboratoryActivityAPI.IRepositories;
using LaboratoryActivityAPI.Repositories;

namespace LaboratoryActivityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationSettings _appSettings;

        IStudentRepository _studentRepository;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSettings> appSettings, LabActivityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;

            _studentRepository = new StudentRepository(context, userManager);
        }

        [HttpPost]
        [Route("Register")]
        //POST : /api/ApplicationUser/Register
        public async Task<Object> PostApplicationUser(ApplicationUserModel model)
        {
            model.Role = "Teacher";
            var applicationUser = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };

            try
            {
                var result = await _userManager.CreateAsync(applicationUser, model.Password);
                await _userManager.AddToRoleAsync(applicationUser, model.Role);
                return Ok(result);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        [Route("RegisterStudent")]
        //POST : /api/ApplicationUser/RegisterStudent
        public async Task<Object> PostStudent(ApplicationUserModel model)
        {
            var result = await _studentRepository.Add(model);
            
            if(result.Equals("conflict"))
            {
                return Conflict();
            } else if(result.Equals("bad request"))
            {
                return BadRequest();
            } else
            {
                return result;
            }
        }

        [HttpDelete("{id}")]
        //DELETE : /api/ApplicationUser/id
        public async Task<Object> DeleteStudent(string id)
        {
            var result = await _studentRepository.Delete(id);

            if (result.Equals("no content"))
            {
                return NoContent();
            }
            else if (result.Equals("not found"))
            {
                return NotFound();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        //GET : /api/ApplicationUser/
        public async Task<List<ApplicationUserModel>> GetStudents()
        {
            return await _studentRepository.GetAll();
        }

        [HttpGet]
        [Route("Student{id}")]
        //GET : /api/ApplicationUser/
        public async Task<ApplicationUserModel> GetStudentById(string id)
        {
            return await _studentRepository.GetByIdAsModel(id);
        }

        [HttpGet]
        [Route("StudentName/{username}")]
        //GET : /api/ApplicationUser/
        public async Task<ApplicationUserModel> GetStudentByName(string username)
        {
            return await _studentRepository.GetByUsernameAsModel(username);
        }

        [HttpPut]
        //PUT : /api/ApplicationUser
        public async Task<Object> PutStudent(ApplicationUserModel model)
        {
            var result = await _studentRepository.Update(model);

            if (result.Equals("no content"))
            {
                return NoContent();
            }
            else if (result.Equals("not found"))
            {
                return NotFound();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("SetUserRegistered")]
        //PUT : /api/ApplicationUser/SetUserRegistered
        public async Task<Object> SetUserRegistered(ApplicationUserModel model)
        {
            var result = await _studentRepository.SetStudentRegistered(model);

            if (result.Equals("no content"))
            {
                return NoContent();
            }
            else if (result.Equals("not found"))
            {
                return NotFound();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("Login")]
        //POST : /api/ApplicationUser/Login
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                //Get role assigned to the user
                var role = await _userManager.GetRolesAsync(user);
                if (role.FirstOrDefault().Equals("Student")) {
                    var isStudentRegistered = await _studentRepository.IsStudentRegistered(user.Id);
                    if (!isStudentRegistered)
                    {
                        return BadRequest();
                    }
                }
                IdentityOptions _options = new IdentityOptions();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID",user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType,role.FirstOrDefault())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
                return BadRequest(new { message = "Username or password is incorrect." });
        }
    }
}
