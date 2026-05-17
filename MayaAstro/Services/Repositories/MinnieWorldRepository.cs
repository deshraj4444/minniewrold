using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MayaAstro.Services.Repositories
{
    public class MinnieWorldRepository : IMinnieWorldRepository
    {
        private readonly MayaAstroContext _AstroContext;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        public MinnieWorldRepository(MayaAstroContext context, IClock clock, IMapper mapper)
        {
            _AstroContext = context;
            _clock = clock;
            _mapper = mapper;
        }

        #region Contact
        public async Task<ApiResponseModel> GetContactList()
        {
            try
            {
                var data = await _AstroContext.Contacts.ToListAsync();
                if (data != null && data.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = data,
                        Message = "Data fetch successfully",
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
                    Message = "Something went wrong",
                    StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
                };
            }
        }
        public async Task<ApiResponseModel> AddContact(ContactsVM obj)
        {
            try
            {
                var objmodel = await SaveContact(obj);

                return new ApiResponseModel()
                {
                    Data = obj,
                    Message = "Data save successfully",
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
        private async Task<Contacts> SaveContact(ContactsVM objmodel)
        {
            try
            {
                var cont = new Contacts();
                if (objmodel.Id > 0)
                {
                    cont = await _AstroContext.Contacts.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (cont != null)
                    {
                        cont.Id = objmodel.Id;
                        _mapper.Map(objmodel, cont);
                    }
                    else
                    {
                        cont = _mapper.Map<Contacts>(objmodel);
                    }
                }
                if (cont.Id == 0)
                {
                    cont.CreatedDate = _clock.CurrentDateTime();
                    cont = _mapper.Map<Contacts>(objmodel);
                    await _AstroContext.Contacts.AddAsync(cont);

                }
                await _AstroContext.SaveChangesAsync();
                return cont;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ApiResponseModel> GetContactById(int Id)
        {
            try
            {
                var data = await _AstroContext.Contacts.Where(x => x.Id == Id).FirstOrDefaultAsync();
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
        public async Task<ApiResponseModel> DeleteContact(int Id)
        {
            try
            {
                var data = await _AstroContext.Contacts.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _AstroContext.Contacts.Remove(data);
                await _AstroContext.SaveChangesAsync();
                return new ApiResponseModel()
                {
                    Data = data,
                    Message = "Data Deleted successfully",
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

        public async Task<ApiResponseModel> ProductsExists(string Title)
        {
            try
            {
                var data = await _AstroContext.ProductDetail.Where(x => x.Title == Title).FirstOrDefaultAsync();
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
#endregion Contacts
    }
}
