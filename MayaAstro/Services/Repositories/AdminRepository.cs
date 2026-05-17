using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MayaAstro.Services.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly MayaAstroContext _CMSContext;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public AdminRepository(MayaAstroContext context, IClock clock, IMapper mapper)
        {
            _CMSContext = context;
            _clock = clock;
            _mapper = mapper;
        }

        #region Lecture for App
        public async Task<ApiResponseModel> GetLectureList()
        {
            try
            {
                var data = await (from lecture in _CMSContext.LectureCategory
                                  join category in _CMSContext.ContentMaster
                                  on lecture.ContentId equals category.Id
                                  select new
                                  {
                                      lecture.Id,
                                      lecture.LectureName,
                                      lecture.ContentId,
                                      LectureCategoryName = category.Name
                                  }).OrderByDescending(lecture => lecture.Id).ToListAsync();
                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> AddLecture(LectureCategoryVM obj)
        {
            try
            {
                var objmodel = await SaveLectureCategory(obj);

                return new ApiResponseModel()
                {
                    Data = obj,
                    Message = "Data save successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<LectureCategory> SaveLectureCategory(LectureCategoryVM objmodel)
        {
            try
            {
                var LectureCategory = new LectureCategory();
                if (objmodel.Id > 0)
                {
                    LectureCategory = await _CMSContext.LectureCategory.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (LectureCategory != null)
                    {
                        LectureCategory.Id = objmodel.Id;
                        LectureCategory.ModifiedDate = _clock.CurrentDateTime();
                        LectureCategory.ModifiedBy = objmodel.ModifiedBy;
                        _mapper.Map(objmodel, LectureCategory);
                    }
                    else
                    {
                        LectureCategory = _mapper.Map<LectureCategory>(objmodel);
                    }
                }
                if (LectureCategory.Id == 0)
                {

                    LectureCategory.CreatedDate = _clock.CurrentDateTime();
                    LectureCategory.CreatedBy = objmodel.CreatedBy;
                    LectureCategory = _mapper.Map<LectureCategory>(objmodel);
                    await _CMSContext.LectureCategory.AddAsync(LectureCategory);

                }
                await _CMSContext.SaveChangesAsync();
                return LectureCategory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        public async Task<ApiResponseModel> GetLectureById(int Id)
        {
            try
            {
                var data = await _CMSContext.LectureCategory.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                // If data is empty, return a success response with an empty list
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }
        public async Task<ApiResponseModel> DeleteLecture(int Id)
        {
            try
            {
                var data = await _CMSContext.LectureCategory.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _CMSContext.LectureCategory.Remove(data);
                await _CMSContext.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Data = data,
                    Message = "Data Deleted successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }



        public async Task<ApiResponseModel> GetLectureDetailsList()
        {
            try
            {
                var data = await (from lecture in _CMSContext.LectureDetail
                                  join category in _CMSContext.LectureCategory
                                  on lecture.LectureId equals category.Id
                                  select new
                                  {
                                      lecture.Id,
                                      lecture.Name,
                                      lecture.EnglishName,
                                      lecture.LectureId,
                                      LectureCategoryName = category.LectureName
                                  }).OrderByDescending(lecture => lecture.Id).ToListAsync();

                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetched successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                // Log the exception for debugging (if logging is implemented)
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }

        public async Task<ApiResponseModel> GetLectureListByCategory(int categoryId)
        {
            try
            {
                // Perform the join between LectureDetail and LectureCategory
                var data = await (from ld in _CMSContext.LectureDetail
                                  join lc in _CMSContext.LectureCategory
                                  on ld.LectureId equals lc.Id
                                  where ld.LectureId == categoryId // Filter by categoryId
                                  select new
                                  {
                                      LectureDetail = ld,
                                      CategoryName = lc.LectureName // Assuming 'Name' is the category name field
                                  })
                                  .ToListAsync();

                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data.Select(d => new
                        {
                            d.LectureDetail.Id,
                            d.LectureDetail.LectureId,
                            d.LectureDetail.Name,           // Include Name field from LectureDetail
                            d.LectureDetail.EnglishName,
                            d.LectureDetail.CreatedBy,
                            d.LectureDetail.CreatedDate,
                            d.LectureDetail.ModifiedBy,
                            d.LectureDetail.ModifiedDate,
                            CategoryName = d.CategoryName // Include the CategoryName in the result
                        }).ToList(),
                        Message = "Data fetched successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }


        public async Task<ApiResponseModel> AddLectureDetails(LectureDetailVM obj)
        {
            try
            {
                var objmodel = await SaveLectureDetail(obj);

                return new ApiResponseModel()
                {
                    Data = obj,
                    Message = "Data save successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<LectureDetail> SaveLectureDetail(LectureDetailVM objmodel)
        {
            try
            {
                var Lecture = new LectureDetail();
                if (objmodel.Id > 0)
                {
                    // Find the existing Lecture entry
                    Lecture = await _CMSContext.LectureDetail.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (Lecture != null)
                    {
                        Lecture.Id = objmodel.Id;
                        Lecture.ModifiedDate = _clock.CurrentDateTime();
                        Lecture.ModifiedBy = objmodel.ModifiedBy;
                        _mapper.Map(objmodel, Lecture);

                    }
                    else
                    {
                        // Map to a new LectureDetail instance if not found
                        Lecture = _mapper.Map<LectureDetail>(objmodel);
                    }
                }
                else
                {
                    // Create a new LectureDetail instance

                    Lecture.CreatedDate = _clock.CurrentDateTime();
                    Lecture.CreatedBy = objmodel.CreatedBy;
                    Lecture = _mapper.Map<LectureDetail>(objmodel);
                    await _CMSContext.LectureDetail.AddAsync(Lecture);
                }

                // Save changes to the database
                await _CMSContext.SaveChangesAsync();
                return Lecture;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        public async Task<ApiResponseModel> GetLectureDetailsById(int Id)
        {
            try
            {
                var data = await _CMSContext.LectureDetail.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                // If data is empty, return a success response with an empty list
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> DeleteLectureDetails(int Id)
        {
            try
            {
                var data = await _CMSContext.LectureDetail.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _CMSContext.LectureDetail.Remove(data);
                await _CMSContext.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Data = data,
                    Message = "Data Deleted successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        #endregion
        private static readonly Dictionary<int, string> DeviceTokenStorage = new Dictionary<int, string>();

        public async Task<bool> SaveDeviceToken(DeviceTokenVM model)
        {
            try
            {

                // Add a new token
                var newToken = new DeviceToken
                {
                    UserId = model.UserId,
                    Token = model.Token,
                    Idiom = model.Idiom,
                    Manufacturer = model.Manufacturer,
                    Model = model.Model,
                    Name = model.Name,
                    Platform = model.Platform,
                    Version = model.Version
                };

                await _CMSContext.DeviceToken.AddAsync(newToken);


                // Save changes to the database
                await _CMSContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error saving device token: {ex.Message}");
                return false;
            }
        }



        public async Task<List<string>> GetDeviceToken()
        {
            // Fetch all device tokens from the database
            var tokens = await _CMSContext.DeviceToken
                .Select(t => t.Token)
                .ToListAsync();

            return tokens;
        }
        public async Task<LoginVM> GetAdminUser(LoginVM loginViewModel)
        {
            try
            {
                return await (from u in _CMSContext.Users
                              where u.Email.ToLower().Trim() == loginViewModel.Email.ToLower().Trim()
                              select new LoginVM
                              {
                                  Id = u.Id,
                                  Email = u.Email,
                                  FirstName = u.FirstName,
                                  LastName = u.LastName,
                                  Salt = u.Salt,
                                  Password = u.Password,
                              }).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<Users> UpdateAuthToken(LoginVM userprofilemodel)
        {
            try
            {
                var user = new Users();
                if (userprofilemodel.Id != null)
                {
                    user = await _CMSContext.Users.FirstOrDefaultAsync(s => s.Id == userprofilemodel.Id);
                    user.AuthToken = userprofilemodel.AuthToken;

                }
                await _CMSContext.SaveChangesAsync();
                return user;


            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #region Question
        public async Task<ApiResponseModel> GetQuestionList()
        {
            try
            {
                var data = await _CMSContext.Question.OrderByDescending(question => question.Id).ToListAsync();
                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> AddQuestion(QuestionVM obj)
        {
            try
            {
                var objmodel = await SaveQuestion(obj);

                return new ApiResponseModel()
                {
                    Data = obj,
                    Message = "Data save successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<Question> SaveQuestion(QuestionVM objmodel)
        {
            try
            {
                var Question = new Question();
                if (objmodel.Id > 0)
                {
                    Question = await _CMSContext.Question.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (Question != null)
                    {

                        // Ensure Id is set correctly
                        Question.Id = objmodel.Id;
                        Question.ModifiedDate = _clock.CurrentDateTime();
                        Question.ModifiedBy = objmodel.ModifiedBy;
                        _mapper.Map(objmodel, Question);
                    }
                    else
                    {
                        Question = _mapper.Map<Question>(objmodel);
                    }
                }
                if (Question.Id == 0)
                {

                    Question.CreatedDate = _clock.CurrentDateTime();
                    Question.CreatedBy = objmodel.CreatedBy;
                    Question = _mapper.Map<Question>(objmodel);
                    await _CMSContext.Question.AddAsync(Question);

                }
                await _CMSContext.SaveChangesAsync();
                return Question;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        public async Task<ApiResponseModel> GetQuestionById(int Id)
        {
            try
            {
                var data = await _CMSContext.Question.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                // If data is empty, return a success response with an empty list
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }
        public async Task<ApiResponseModel> DeleteQuestion(int Id)
        {
            try
            {
                var data = await _CMSContext.Question.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _CMSContext.Question.Remove(data);
                await _CMSContext.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Data = data,
                    Message = "Data Deleted successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> GetQuestionDetailList()
        {
            try
            {
                var data = await (from lecture in _CMSContext.QuestionDetail
                                  join category in _CMSContext.Question
                                  on lecture.QuestionId equals category.Id
                                  select new
                                  {
                                      lecture.Id,
                                      lecture.Answer,
                                      lecture.IsTrue,
                                      lecture.UserId,
                                      lecture.QuestionId,
                                      QuestionName = category.Title
                                  }).OrderByDescending(lecture => lecture.Id).ToListAsync();

                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetched successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                // Log the exception for debugging (if logging is implemented)
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }

        public async Task<ApiResponseModel> GetQuestionListByCategory()
        {
            try
            {
                // Fetch the Question list with the associated QuestionDetail records
                var data = await (from ld in _CMSContext.Question
                                  join lc in _CMSContext.QuestionDetail
                                  on ld.Id equals lc.QuestionId
                                  select new
                                  {
                                      LectureDetail = ld,
                                      QuestionDetail = lc,
                                      CategoryName = ld.Title
                                  })
                                  .ToListAsync();

                if (data != null && data.Any())
                {
                    // Group by the Question Id and project the result in the desired format
                    var groupedData = data
                        .GroupBy(d => d.LectureDetail.Id)
                        .Select(g => new
                        {
                            Question = new
                            {
                                g.FirstOrDefault().LectureDetail.Id,
                                g.FirstOrDefault().LectureDetail.Title,

                            },
                            QuestionDetails = g.Select(d => new
                            {
                                d.QuestionDetail.Id,
                                d.QuestionDetail.Answer,
                                d.QuestionDetail.QuestionId,
                                d.QuestionDetail.IsTrue
                            }).ToList()
                        }).ToList();

                    return new ApiResponseModel()
                    {
                        Data = groupedData,
                        Message = "Data fetched successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }





        public async Task<ApiResponseModel> AddQuestionDetail(QuestionDetailVM obj)
        {
            try
            {
                var objmodel = await SaveQuestionDetail(obj);

                return new ApiResponseModel()
                {
                    Data = obj,
                    Message = "Data save successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<QuestionDetail> SaveQuestionDetail(QuestionDetailVM objmodel)
        {
            try
            {
                var Question = new QuestionDetail();
                if (objmodel.Id > 0)
                {
                    // Find the existing Question entry
                    Question = await _CMSContext.QuestionDetail.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (Question != null)
                    {
                        Question.Id = objmodel.Id;
                        Question.ModifiedDate = _clock.CurrentDateTime();
                        Question.ModifiedBy = objmodel.ModifiedBy;
                        _mapper.Map(objmodel, Question);

                    }
                    else
                    {
                        // Map to a new QuestionDetail instance if not found
                        Question = _mapper.Map<QuestionDetail>(objmodel);

                    }
                }
                else
                {
                    // Create a new QuestionDetail instance

                    Question.CreatedDate = _clock.CurrentDateTime();
                    Question.CreatedBy = objmodel.CreatedBy;
                    Question = _mapper.Map<QuestionDetail>(objmodel);
                    await _CMSContext.QuestionDetail.AddAsync(Question);
                }

                // Save changes to the database
                await _CMSContext.SaveChangesAsync();
                return Question;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Handle the exception as needed, such as logging
                return null;
            }
        }
        public async Task<ApiResponseModel> GetQuestionDetailById(int Id)
        {
            try
            {
                var data = await _CMSContext.QuestionDetail.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                // If data is empty, return a success response with an empty list
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> DeleteQuestionDetail(int Id)
        {
            try
            {
                var data = await _CMSContext.QuestionDetail.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _CMSContext.QuestionDetail.Remove(data);
                await _CMSContext.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Data = data,
                    Message = "Data Deleted successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        #endregion
        public async Task<ApiResponseModel> GetQuickLearnList()
        {
            try
            {
                var data = await _CMSContext.QuickLearn.OrderByDescending(question => question.Id).ToListAsync();
                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> AddQuickLearn(QuickLearnVM obj)
        {
            try
            {
                var objmodel = await SaveQuickLearn(obj);

                return new ApiResponseModel()
                {
                    Data = obj,
                    Message = "Data save successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<QuickLearn> SaveQuickLearn(QuickLearnVM objmodel)
        {
            try
            {
                var Question = new QuickLearn();
                if (objmodel.Id > 0)
                {
                    // Find the existing Question entry
                    Question = await _CMSContext.QuickLearn.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (Question != null)
                    {
                        Question.Id = objmodel.Id;
                        Question.ModifiedDate = _clock.CurrentDateTime();
                        Question.ModifiedBy = objmodel.ModifiedBy;
                        _mapper.Map(objmodel, Question);

                    }
                    else
                    {
                        // Map to a new QuestionDetail instance if not found
                        Question = _mapper.Map<QuickLearn>(objmodel);

                    }
                }
                else
                {
                    // Create a new QuestionDetail instance

                    Question.CreatedDate = _clock.CurrentDateTime();
                    Question.CreatedBy = objmodel.CreatedBy;
                    Question = _mapper.Map<QuickLearn>(objmodel);
                    await _CMSContext.QuickLearn.AddAsync(Question);
                }

                // Save changes to the database
                await _CMSContext.SaveChangesAsync();
                return Question;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Handle the exception as needed, such as logging
                return null;
            }
        }
        public async Task<ApiResponseModel> GetQuickLearnById(int Id)
        {
            try
            {
                var data = await _CMSContext.QuickLearn.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK)

                    };
                }
                // If data is empty, return a success response with an empty list
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> DeleteQuickLearn(int Id)
        {
            try
            {
                var data = await _CMSContext.QuickLearn.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _CMSContext.QuickLearn.Remove(data);
                await _CMSContext.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Data = data,
                    Message = "Data Deleted successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }

        #region ContantMaster
        public async Task<ApiResponseModel> GetContantMasterList()
        {
            try
            {
                var data = await _CMSContext.ContentMaster.OrderByDescending(question => question.Id).ToListAsync();
                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> AddContantMaster(ContentMasterVM obj)
        {
            try
            {
                var objmodel = await SaveContantMaster(obj);

                return new ApiResponseModel()
                {
                    Data = obj,
                    Message = "Data save successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK)

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<ContentMaster> SaveContantMaster(ContentMasterVM objmodel)
        {
            try
            {
                var ContentMaster = new ContentMaster();
                if (objmodel.Id > 0)
                {
                    // Find the existing ContentMaster entry
                    ContentMaster = await _CMSContext.ContentMaster.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (ContentMaster != null)
                    {

                        ContentMaster.Id = objmodel.Id;
                        ContentMaster.ModifiedDate = _clock.CurrentDateTime();
                        ContentMaster.ModifiedBy = objmodel.ModifiedBy;
                        _mapper.Map(objmodel, ContentMaster);

                    }
                    else
                    {
                        // Map to a new ContentMasterDetail instance if not found
                        ContentMaster = _mapper.Map<ContentMaster>(objmodel);

                    }
                }
                else
                {
                    // Create a new ContentMasterDetail instance

                    ContentMaster.CreatedDate = _clock.CurrentDateTime();
                    ContentMaster.CreatedBy = objmodel.CreatedBy;
                    ContentMaster = _mapper.Map<ContentMaster>(objmodel);
                    await _CMSContext.ContentMaster.AddAsync(ContentMaster);
                }

                // Save changes to the database
                await _CMSContext.SaveChangesAsync();
                return ContentMaster;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        public async Task<ApiResponseModel> GetContantMasterById(int Id)
        {
            try
            {
                var data = await _CMSContext.ContentMaster.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
                        Status = true,
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                // If data is empty, return a success response with an empty list
                return new ApiResponseModel
                {
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> DeleteContantMaster(int Id)
        {
            try
            {
                var data = await _CMSContext.ContentMaster.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK
                    };
                }
                _CMSContext.ContentMaster.Remove(data);
                await _CMSContext.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Data = data,
                    Message = "Data Deleted successfully",
                    Status = true,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK)

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong" + ex.Message,
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }
        #endregion ContantMaster
    }
}
