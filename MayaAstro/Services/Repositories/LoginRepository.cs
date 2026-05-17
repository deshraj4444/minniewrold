using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
namespace MayaAstro.Services.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly MayaAstroContext _Context;
        private readonly IMapper _mapper;
        public LoginRepository(MayaAstroContext Context, IMapper mapper)
        {
            _Context = Context;
            _mapper = mapper;
        }
        #region Users
        public async Task<ApiResponseModel> GetUserList()
        {
            try
            {
                var data = await _Context.Users.ToListAsync();
                if (data != null && data.Any())
                {
                    return new ApiResponseModel
                    {
                        Data = data,
                        Message = "Data fetched successfully",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                        Status = true
                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                    Status = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Data = null,
                    Message = "Something went wrong while fetching data",
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Status = false
                };
            }
        }
        public async Task<ApiResponseModel> RegisterUser(UserVM UserProfileModel)
        {
            try
            {
                var User = await AddEditUsers(UserProfileModel);
                if (User != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = User.Id,
                        Message = "Data saved successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                    };
                }
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = ex.Message,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<Users> AddEditUsers(UserVM UserProfileModel)
        {
            try
            {
                var user = new Users();
                if (UserProfileModel.Id != Guid.Empty)
                {
                    user = await _Context.Users.FirstOrDefaultAsync(s => s.Id == UserProfileModel.Id);
                    if (user != null)
                    {
                        UserProfileModel.Password = UserProfileModel.Password == null ? user.Password : UserProfileModel.Password;
                    }
                    if (user != null)
                    {
                        // Map the properties from ViewModel to the existing user entity
                        _mapper.Map(UserProfileModel, user);
                    }
                    else
                    {
                        user = _mapper.Map<Users>(UserProfileModel);
                    }
                }
                if (UserProfileModel.Id == Guid.Empty)
                {
                    user = _mapper.Map<Users>(UserProfileModel);
                   

                    await _Context.Users.AddAsync(user);
                    await _Context.SaveChangesAsync();
                    var userWebsite = new UserWebsite
                    {
                        UserId = user.Id,
                        WebsiteId = 1,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _Context.UserWebsite.AddAsync(userWebsite);
                }
                await _Context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<ApiResponseModel> GetUserById(Guid Id)
        {
            try
            {
                var data = await _Context.Users.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                        Status = true
                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                    Status = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> DeleteUser(Guid Id)
        {
            try
            {
                var data = await _Context.Users.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                   
                    await _Context.SaveChangesAsync();
                    return new ApiResponseModel()
                    {
                        Data = data.Id,
                        Message = "User delete successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                        Status = true
                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                    Status = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }
        #endregion Users

        public async Task<ApiResponseModel> EmailExists(string email)
        {
            try
            {
                var userExists = await _Context.Users
                                               .AnyAsync(x => x.Email == email); 
                if (userExists)
                {
                    return new ApiResponseModel
                    {
                        Data = userExists,
                        Message = "Email already exists",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                        Status = true
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Status = false
                };
            }
            catch (Exception ex)
            {
               
                return new ApiResponseModel
                {
                    Message = "Something went wrong",
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError,
                    Status = false
                };
            }
        }

        public async Task<LoginVM> GetAdminUser(LoginVM loginViewModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginViewModel.Email))
                    throw new ArgumentException("Email cannot be null or empty.");

                var user = await _Context.Users
                    .Where(u => u.Email.ToLower().Trim() == loginViewModel.Email.ToLower().Trim())
                    .Select(u => new LoginVM
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Salt = u.Salt,
                        Password = u.Password,
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new InvalidOperationException("User not found.");

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return null;
            }
        }
        public async Task<Users> UpdateAuthToken(LoginVM userprofilemodel)
        {
            try
            {
                var user = new Users();
                //if (userprofilemodel.Id != Guid.Empty)
                //{
                //    user = await _Context.User.FirstOrDefaultAsync(s => s.Id == userprofilemodel.Id);


                //}
                //await _CMSContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<ApiResponseModel> GetUserCurrentWebsiteId(Guid userId)
        {
            try
            {
                var websiteId = await _Context.UserWebsite
                    .Where(uw => uw.UserId == userId)
                    .Select(uw => uw.WebsiteId)
                    .FirstOrDefaultAsync();

                if (websiteId > 0)
                {
                    return new ApiResponseModel
                    {
                        Data = websiteId,
                        Message = "Website ID retrieved successfully",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                        Status = true
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No website ID found for the given user",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                    Status = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Data = null,
                    Message = "Something went wrong while fetching website ID",
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Status = false
                };
            }
        }
        public async Task<ApiResponseModel> UpdateUserWebsite(UserWebsiteVM model)
        {
            try
            {
                var userWebsite = await _Context.UserWebsite
                    .FirstOrDefaultAsync(uw => uw.UserId == model.UserId);
                if (userWebsite != null)
                {
                    userWebsite.WebsiteId = model.WebsiteId;
                    await _Context.SaveChangesAsync();

                    return new ApiResponseModel
                    {
                        Data = userWebsite.WebsiteId,
                        Message = "Website ID updated successfully",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                        Status = true
                    };
                }
                return new ApiResponseModel
                {
                    Message = "User not found or does not have a website assigned",
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Status = false
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Data = null,
                    Message = "Something went wrong while updating website ID",
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Status = false
                };
            }
        }
        #region Websites
        public async Task<ApiResponseModel> GetWebsiteList()
        {
            try
            {
                var data = await _Context.Website.ToListAsync();
                if (data != null && data.Any())
                {
                    return new ApiResponseModel
                    {
                        Data = data,
                        Message = "Data fetched successfully",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                        Status = true
                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                    Status = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Data = null,
                    Message = "Something went wrong while fetching data",
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Status = false
                };
            }
        }
        #endregion Websites
    }
}
