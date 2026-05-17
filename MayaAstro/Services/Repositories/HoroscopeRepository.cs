using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace MayaAstro.Services.Repositories
{
    public class HoroscopeRepository : IHoroscopeRepository
    {
        private readonly MayaAstroContext _Context;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        public HoroscopeRepository(MayaAstroContext context, IClock clock, IMapper mapper)
        {
            _Context = context;
            _clock = clock;
            _mapper = mapper;
        }
        #region Rashi
        public async Task<ApiResponseModel> GetRashiList()
        {
            try
            {
                var data = await _Context.Rashi.OrderBy(item => item.Id).ToListAsync();
                if (data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Status = true,
                        Data = data,
                        Message = "Data fetched successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Status = false,
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> AddRashi(RashiVM obj)
        {
            try
            {
                var objmodel = await SaveRashi(obj);
                return new ApiResponseModel()
                {
                    Status = true,
                    Data = objmodel,
                    Message = "Data save successfully",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<Rashi> SaveRashi(RashiVM objmodel)
        {
            try
            {
                var rashi = await _Context.Rashi.FirstOrDefaultAsync(s => s.Id == objmodel.Id);

                if (rashi != null)
                {
                    // Preserve existing image
                    var oldImage = rashi.ImageUrl;

                    // map model to entity
                    _mapper.Map(objmodel, rashi);

                    // if no new image provided → don't overwrite!
                    if (string.IsNullOrEmpty(objmodel.ImageUrl))
                        rashi.ImageUrl = oldImage;
                }
                else
                {
                    rashi = _mapper.Map<Rashi>(objmodel);
                    rashi.CreatedAt = DateTime.UtcNow;
                    await _Context.Rashi.AddAsync(rashi);
                }

                await _Context.SaveChangesAsync();
                return rashi;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<ApiResponseModel> GetRashiById(int Id)
        {
            try
            {
                var data = await _Context.Rashi.FirstOrDefaultAsync(x => x.Id == Id);
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Status = true,
                        Data = data,
                        Message = "Data fetched successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Status = false,
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),

                };
            }
        }


        public async Task<ApiResponseModel> DeleteRashi(int Id)
        {
            try
            {
                var data = await _Context.Rashi.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Status = false,
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _Context.Rashi.Remove(data);
                await _Context.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Status = true,
                    Data = data,
                    Message = "Data Deleted successfully",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        #endregion Rashi

        #region Horoscope Detail
        public async Task<ApiResponseModel> GetHoroscopeList()
        {
            try
            {
                var data = await _Context.Horoscope.OrderBy(item => item.Id).ToListAsync();
                if (data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Status = true,
                        Data = data,
                        Message = "Data fetched successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Status = false,
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> AddHoroscope(HoroscopeVM obj)
        {
            try
            {
                var objmodel = await SaveHoroscope(obj);
                return new ApiResponseModel()
                {
                    Status = true,
                    Data = objmodel,
                    Message = "Data save successfully",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        private async Task<Horoscope> SaveHoroscope(HoroscopeVM objmodel)
        {
            try
            {
                var horoscope = await _Context.Horoscope.FirstOrDefaultAsync(s => s.Id == objmodel.Id);

                if (horoscope != null)
                {
                    _mapper.Map(objmodel, horoscope);

                }
                else
                {
                    horoscope = _mapper.Map<Horoscope>(objmodel);

                    await _Context.Horoscope.AddAsync(horoscope);
                }
                await _Context.SaveChangesAsync();
                return horoscope;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        public async Task<ApiResponseModel> GetHoroscopeById(int Id)
        {
            try
            {
                var data = await _Context.Horoscope.FirstOrDefaultAsync(x => x.Id == Id);
                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Status = true,
                        Data = data,
                        Message = "Data fetched successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                    };
                }
                return new ApiResponseModel
                {
                    Status = false,
                    Message = "No data found",
                    StatusCode = (int)System.Net.HttpStatusCode.OK,

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),

                };
            }
        }

        public async Task<ApiResponseModel> GetHoroscopeDetailByUrl(string slug)
        {
            try
            {


                var data = await _Context.Horoscope
                    .Where(x => x.PageUrl.ToLower() == slug.ToLower())
                    .FirstOrDefaultAsync();

                if (data != null)
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
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
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> DeleteHoroscope(int Id)
        {
            try
            {
                var data = await _Context.Horoscope.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Status = false,
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _Context.Horoscope.Remove(data);
                await _Context.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Status = true,
                    Data = data,
                    Message = "Data Deleted successfully",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel()
                {
                    Status = false,
                    Data = null,
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }

        }

        public async Task<ApiResponseModel> TrueFalse()
        {
            // Simulate an asynchronous operation
            bool result = DateTime.UtcNow.Second % 2 == 0;

            return new ApiResponseModel
            {
                Status = true,
                Message = "Data fetched successfully",
                Data = result
            };

        }


        #endregion Horoscope Detail

    }
}
