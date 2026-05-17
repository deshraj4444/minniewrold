using System;
using System.Net;
using System.Text.Json;
using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MayaAstro.Services.Repositories
{
    public class JewellerRepositiory : IJewellerRepository
    {
        private readonly MayaAstroContext _Context;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly HttpClient _client = new HttpClient();

        public JewellerRepositiory(MayaAstroContext context, IClock clock, IMapper mapper)
        {
            _Context = context;
            _clock = clock;
            _mapper = mapper;
        }

        #region MessageBannerDetail
        public async Task<ApiResponseModel> AddMessageBannerDetail(MessageBannerDetailVM model)
        {
            try
            {
                var entity = await SaveMessageBannerDetail(model);

                return new ApiResponseModel
                {
                    Data = _mapper.Map<MessageBannerDetailVM>(entity),
                    Message = "MessageBanner saved successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        private async Task<MessageBannerDetail> SaveMessageBannerDetail(MessageBannerDetailVM model)
        {
            try
            {
                MessageBannerDetail entity;

                if (model.Id > 0)
                {
                    entity = await _Context.MessageBannerDetail
                        .FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (entity == null)
                        throw new Exception("MessageBanner not found");

                    _mapper.Map(model, entity);
                    entity.ModifiedDate = _clock.CurrentDateTime();
                    entity.ModifiedBy = model.ModifiedBy;

                    _Context.MessageBannerDetail.Update(entity);
                }
                else
                {
                    entity = _mapper.Map<MessageBannerDetail>(model);
                    entity.CreatedDate = _clock.CurrentDateTime();
                    entity.CreatedBy = model.CreatedBy;

                    await _Context.MessageBannerDetail.AddAsync(entity);
                }

                await _Context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving MessageBanner: " + ex.Message);
            }
        }

        public async Task<ApiResponseModel> GetMessageBannerDetailById(int Id)
        {
            try
            {
                var data = await _Context.MessageBannerDetail.FirstOrDefaultAsync(x => x.Id == Id);

                if (data != null)
                {
                    return new ApiResponseModel
                    {
                        Data = data,
                        Message = "Data fetched successfully",
                        Status = true,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No data found",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        #endregion

        #region BankDetail
        public async Task<ApiResponseModel> AddBankDetail(BankDetailVM model)
        {
            try
            {
                var entity = await SaveBankDetail(model);

                return new ApiResponseModel
                {
                    Data = _mapper.Map<BankDetailVM>(entity),
                    Message = "Bank detail saved successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        private async Task<BankDetail> SaveBankDetail(BankDetailVM model)
        {
            try
            {
                BankDetail entity;

                if (model.Id > 0)
                {
                    entity = await _Context.BankDetail.FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (entity == null)
                        throw new Exception("Bank detail not found");

                    _mapper.Map(model, entity);
                    _Context.BankDetail.Update(entity);
                }
                else
                {
                    entity = _mapper.Map<BankDetail>(model);
                    await _Context.BankDetail.AddAsync(entity);
                }

                await _Context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving BankDetail: " + ex.Message);
            }
        }

        public async Task<ApiResponseModel> GetBankDetailById(int Id)
        {
            try
            {
                var data = await _Context.BankDetail.FirstOrDefaultAsync(x => x.Id == Id);

                if (data != null)
                {
                    return new ApiResponseModel
                    {
                        Data = data,
                        Message = "Data fetched successfully",
                        Status = true,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }

                return new ApiResponseModel
                {
                    Message = "No data found",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        public async Task<ApiResponseModel> GetBankDetailList()
        {
            try
            {
                var data = await _Context.BankDetail
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                var result = _mapper.Map<List<BankDetailVM>>(data);

                return new ApiResponseModel
                {
                    Data = result,
                    Message = data.Any() ? "Data fetched successfully" : "No data found",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        public async Task<ApiResponseModel> DeleteBankDetail(int id)
        {
            try
            {
                var entity = await _Context.BankDetail.FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "Bank detail not found",
                        Status = false,
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

                _Context.BankDetail.Remove(entity);

                await _Context.SaveChangesAsync();

                return new ApiResponseModel
                {
                    Message = "Bank detail deleted successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        #endregion

        #region CommonEnquiry
        public async Task<ApiResponseModel> VerifyPhone(string phone)
        {
            try
            {
                var user = await _Context.CommonEnquiry
                    .FirstOrDefaultAsync(x => x.Phone == phone && x.Type == "2");

                if (user != null)
                {
                    return new ApiResponseModel
                    {
                        Data = _mapper.Map<CommonEnquiryVM>(user),
                        Message = "User already exists",
                        Status = true,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }

                return new ApiResponseModel
                {
                    Message = "User not found",
                    Status = false,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        public async Task<ApiResponseModel> AddEnquiry(CommonEnquiryVM model)
        {
            try
            {
                int.TryParse(model.Type, out int type);

                // ----------------------------
                // Registration Duplicate Check
                // ----------------------------
                //if (type == 2)
                //{
                //    var existingUser = await _Context.CommonEnquiry
                //        .FirstOrDefaultAsync(x => x.Phone == model.Phone && x.Type == "2");

                //    if (existingUser != null)
                //    {
                //        return new ApiResponseModel
                //        {
                //            Message = "This mobile number is already registered.",
                //            Status = false,
                //            StatusCode = 400
                //        };
                //    }
                //}

                // ----------------------------
                // Enquiry Spam Protection
                // ----------------------------
                // ----------------------------
                // Enquiry Spam Protection
                // ----------------------------
                if (type == 1)
                {
                    var lastEnquiry = await _Context.CommonEnquiry
                        .Where(x => x.Phone == model.Phone && x.Type == "1")
                        .OrderByDescending(x => x.CreatedDate)
                        .FirstOrDefaultAsync();

                    if (lastEnquiry != null && lastEnquiry.CreatedDate.HasValue)
                    {
                        var minutes = (_clock.CurrentDateTime() - lastEnquiry.CreatedDate.Value).TotalMinutes;

                        if (minutes < 2)
                        {
                            return new ApiResponseModel
                            {
                                Message = "You recently submitted an enquiry. Please wait.",
                                Status = false,
                                StatusCode = 400
                            };
                        }
                    }
                }
                var entity = await SaveEnquiry(model);

                return new ApiResponseModel
                {
                    Data = _mapper.Map<CommonEnquiryVM>(entity),
                    Message = "Enquiry saved successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        private async Task<CommonEnquiry> SaveEnquiry(CommonEnquiryVM model)
        {
            try
            {
                CommonEnquiry entity;

                if (model.Id > 0)
                {
                    entity = await _Context.CommonEnquiry.FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (entity == null)
                        throw new Exception("Enquiry not found");

                    _mapper.Map(model, entity);
                    entity.ModifiedDate = _clock.CurrentDateTime();
                    entity.ModifiedBy = model.ModifiedBy;
                    _Context.CommonEnquiry.Update(entity);
                }
                else
                {
                    entity = _mapper.Map<CommonEnquiry>(model);
                    entity.CreatedDate = _clock.CurrentDateTime();
                    entity.CreatedBy = model.CreatedBy;
                    entity.IsActive = true;
                    await _Context.CommonEnquiry.AddAsync(entity);
                }

                await _Context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving enquiry: " + ex.Message);
            }
        }

        public async Task<ApiResponseModel> GetEnquiryList(string type)
        {
            try
            {
                var data = await _Context.CommonEnquiry
                    .Where(x => x.Type == type)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                var result = _mapper.Map<List<CommonEnquiryVM>>(data);

                return new ApiResponseModel
                {
                    Data = result,
                    Message = data.Any() ? "Data fetched successfully" : "No data found",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<ApiResponseModel> GetEnquiryById(int id)
        {
            try
            {
                var data = await _Context.CommonEnquiry.FirstOrDefaultAsync(x => x.Id == id);

                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        Status = false,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }

                var result = _mapper.Map<CommonEnquiryVM>(data);

                return new ApiResponseModel
                {
                    Data = result,
                    Message = "Data fetched successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<ApiResponseModel> DeleteEnquiry(int id)
        {
            try
            {
                var entity = await _Context.CommonEnquiry.FindAsync(id);

                if (entity == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        Status = false,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }

                // Permanently remove from database
                _Context.CommonEnquiry.Remove(entity);
                await _Context.SaveChangesAsync();

                return new ApiResponseModel
                {
                    Message = "Enquiry deleted successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        #endregion

        #region GoldPrice
        public async Task<GoldPriceData> GetLatestGoldPriceAsync()
        {
            try
            {
                var data = await _Context.GoldPriceData
                            .OrderByDescending(x => x.UpdatedAt)
                            .FirstOrDefaultAsync();

                return data;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching GoldPrice: " + ex.Message);
            }
        }
        public async Task SaveGoldPriceAsync(GoldPriceData data)
        {
            try
            {
                var existing = await _Context.GoldPriceData.FirstOrDefaultAsync(x => x.Id == 1);

                if (existing != null)
                {
                    // Update existing record
                    existing.GoldSpot = data.GoldSpot;
                    existing.SilverSpot = data.SilverSpot;
                    existing.InrSpot = data.InrSpot;

                    existing.Gold995InclGstBuy = data.Gold995InclGstBuy;
                    existing.Gold995InclGstSell = data.Gold995InclGstSell;

                    existing.Gold995Below50Buy = data.Gold995Below50Buy;
                    existing.Gold995Below50Sell = data.Gold995Below50Sell;

                    existing.Gold995Below10Buy = data.Gold995Below10Buy;
                    existing.Gold995Below10Sell = data.Gold995Below10Sell;

                    existing.Silver30KgImportedBuy = data.Silver30KgImportedBuy;
                    existing.Silver30KgImportedSell = data.Silver30KgImportedSell;

                    existing.SilverBelow30KgBuy = data.SilverBelow30KgBuy;
                    existing.SilverBelow30KgSell = data.SilverBelow30KgSell;

                    existing.GoldFutureBuy = data.GoldFutureBuy;
                    existing.GoldFutureSell = data.GoldFutureSell;

                    existing.SilverFutureBuy = data.SilverFutureBuy;
                    existing.SilverFutureSell = data.SilverFutureSell;

                    existing.GoldNextBuy = data.GoldNextBuy;
                    existing.GoldNextSell = data.GoldNextSell;

                    existing.SilverNextBuy = data.SilverNextBuy;
                    existing.SilverNextSell = data.SilverNextSell;

                    existing.UpdatedAt = DateTime.UtcNow;

                    _Context.GoldPriceData.Update(existing);
                }
                else
                {
                    // First time insert
                    await _Context.GoldPriceData.AddAsync(data);
                }

                await _Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving GoldPrice: " + ex.Message);
            }
        }
        #endregion



        public async Task<ApiResponseModel> SaveAnnouncement(AnnouncementsVM model)
        {
            try
            {
                var entity = await SaveAnnouncementEntity(model);

                return new ApiResponseModel
                {
                    Data = _mapper.Map<AnnouncementsVM>(entity),
                    Message = "Announcement saved successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        private async Task<Announcements> SaveAnnouncementEntity(AnnouncementsVM model)
        {
            try
            {
                Announcements entity;

                if (model.Id > 0)
                {
                    entity = await _Context.Announcements
                        .FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (entity == null)
                        throw new Exception("Announcement not found");

                    _mapper.Map(model, entity);


                    _Context.Announcements.Update(entity);
                }
                else
                {
                    entity = _mapper.Map<Announcements>(model);

                    entity.CreatedDate = _clock.CurrentDateTime();

                    await _Context.Announcements.AddAsync(entity);
                }

                await _Context.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving announcement: " + ex.Message);
            }
        }
        public async Task<ApiResponseModel> GetAnnouncementList()
        {
            try
            {
                var data = await _Context.Announcements
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                var result = _mapper.Map<List<AnnouncementsVM>>(data);

                return new ApiResponseModel
                {
                    Data = result,
                    Message = data.Any() ? "Data fetched successfully" : "No data found",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        public async Task<ApiResponseModel> GetAnnouncementById(int id)
        {
            try
            {
                var data = await _Context.Announcements
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        Status = false,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }

                var result = _mapper.Map<AnnouncementsVM>(data);

                return new ApiResponseModel
                {
                    Data = result,
                    Message = "Data fetched successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        public async Task<ApiResponseModel> GetAnnouncementByDate(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _Context.Announcements.AsQueryable();

                if (startDate.HasValue && endDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    var end = endDate.Value.Date;

                    query = query.Where(x => x.StartDate.Date >= start && x.StartDate.Date <= end);
                }
                else if (startDate.HasValue)
                {
                    var start = startDate.Value.Date;

                    query = query.Where(x => x.StartDate.Date == start);
                }
                else if (endDate.HasValue)
                {
                    var end = endDate.Value.Date;

                    query = query.Where(x => x.StartDate.Date == end);
                }
                else
                {
                    // ⭐ If no filter → show latest 10 updates
                    query = query
                        .OrderByDescending(x => x.StartDate)
                        .Take(10);
                }

                var data = await query
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync();

                var result = _mapper.Map<List<AnnouncementsVM>>(data);

                return new ApiResponseModel
                {
                    Data = result,
                    Message = data.Any() ? "Data fetched successfully" : "No data found",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        public async Task<ApiResponseModel> DeleteAnnouncement(int id)
        {
            try
            {
                var entity = await _Context.Announcements
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "Announcement not found",
                        Status = false,
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

                _Context.Announcements.Remove(entity);

                await _Context.SaveChangesAsync();

                return new ApiResponseModel
                {
                    Message = "Announcement deleted successfully",
                    Status = true,
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
        #region Premium


        public async Task<ApiResponseModel> GetGoldSettingsById(int id)
        {
            try
            {
                var data = await _Context.GoldSettings
                    .FirstOrDefaultAsync(x => x.Id == id);

                return new ApiResponseModel
                {
                    Data = data,
                    Status = true,
                    StatusCode = 200,
                    Message = "Gold settings fetched successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Status = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }
        public async Task<ApiResponseModel> GetPremiumAsyncById(int id)
        {
            try
            {
                var data = await _Context.GoldSettings
                             .FirstOrDefaultAsync(x => x.Id == id);

                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Status = false,
                        StatusCode = 404,
                        Message = "Gold settings not found"
                    };
                }

                // Build a premium object for both Gold and Silver
                var premiumData = new
                {
                    // Gold
                    GoldPremium = (data.GoldPremiumPerGram ?? 0) + (data.GoldGstAdjustment ?? 0) + (data.GoldDealerMargin ?? 0),

                    // Silver
                    SilverPremium = (data.SilverPremiumPerKg ?? 0) + (data.SilverGstAdjustment ?? 0) + (data.SilverDealerMargin ?? 0)
                };

                return new ApiResponseModel
                {
                    Data = premiumData,
                    Status = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Status = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }
        public async Task<ApiResponseModel> SaveGoldSettings(GoldSettingsVM model)
        {
            try
            {
                var entity = await AddOrUpdateGoldSettings(model);

                return new ApiResponseModel
                {
                    Data = _mapper.Map<GoldSettingsVM>(entity),
                    Message = "Gold settings saved successfully",
                    Status = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Message = "Something went wrong: " + ex.Message,
                    Status = false,
                    StatusCode = 400
                };
            }
        }
        public async Task<GoldSettings> AddOrUpdateGoldSettings(GoldSettingsVM model)
        {
            GoldSettings entity = await _Context.GoldSettings
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entity != null)
            {
                // Update
                entity.GoldPremiumPerGram = model.GoldPremiumPerGram;
                entity.GoldGstAdjustment = model.GoldGstAdjustment;
                entity.GoldDealerMargin = model.GoldDealerMargin;

                entity.SilverPremiumPerKg = model.SilverPremiumPerKg;
                entity.SilverGstAdjustment = model.SilverGstAdjustment;
                entity.SilverDealerMargin = model.SilverDealerMargin;
                entity.UpdatedAt = DateTime.Now;

                _Context.GoldSettings.Update(entity);
            }
            else
            {
                // Insert
                entity = new GoldSettings
                {
                    GoldPremiumPerGram = model.GoldPremiumPerGram,
                GoldGstAdjustment = model.GoldGstAdjustment,
                GoldDealerMargin = model.GoldDealerMargin,

                SilverPremiumPerKg = model.SilverPremiumPerKg,
                SilverGstAdjustment = model.SilverGstAdjustment,
                SilverDealerMargin = model.SilverDealerMargin,
                UpdatedAt = DateTime.Now
                };

                await _Context.GoldSettings.AddAsync(entity);
            }

            await _Context.SaveChangesAsync();

            return entity;
        }
        #endregion
    }
}