using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MayaAstro.Services.Mappers
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Contacts, ContactsVM>();
            CreateMap<BlogDetail, BlogDetailVM>();
            CreateMap<BlogDetailVM, BlogDetail>();
            CreateMap<BlogCategory, BlogCategoryVM>();
            CreateMap<BlogCategoryVM, BlogCategory>();
            CreateMap<RashiVM, Rashi>();
            CreateMap<Rashi, RashiVM>();
            CreateMap<HoroscopeVM, Horoscope>();
            CreateMap<Horoscope, HoroscopeVM>();
            CreateMap<ProductDetail, ProductDetailVM>();
            CreateMap<ProductDetailVM, ProductDetail>();    
            CreateMap<Product, ProductVM>();
            CreateMap<ProductVM, Product>();
            CreateMap<ProductSubCategory, ProductSubCategoryVM>();
            CreateMap<ProductSubCategoryVM, ProductSubCategory>();
            CreateMap<ProductSubCategoryVM, ProductSubCategory>();
            CreateMap<ProductSubCategoryVM, ProductSubCategory>();
            CreateMap<UserVM, Users>().ReverseMap();
            CreateMap<LectureCategory, LectureCategoryVM>();
            CreateMap<LectureCategoryVM, LectureCategory>();
            CreateMap<LectureDetail, LectureDetailVM>();
            CreateMap<LectureDetailVM, LectureDetail>();
            CreateMap<Question, QuestionVM>();
            CreateMap<QuestionVM, Question>();
            CreateMap<QuestionDetail, QuestionDetailVM>();
            CreateMap<QuestionDetailVM, QuestionDetail>();
            CreateMap<QuickLearn, QuickLearnVM>();
            CreateMap<QuickLearnVM, QuickLearn>();
            CreateMap<ContentMaster, ContentMasterVM>();
            CreateMap<ContentMasterVM, ContentMaster>();
            CreateMap<BankDetail, BankDetailVM>();
            CreateMap<BankDetailVM, BankDetail>();
            CreateMap<CommonEnquiry, CommonEnquiryVM>();
            CreateMap<CommonEnquiryVM, CommonEnquiry>();
            CreateMap<MessageBannerDetail, MessageBannerDetailVM>();
            CreateMap<MessageBannerDetailVM, MessageBannerDetail>();
            CreateMap<GoldPriceData, GoldPriceDataVM>();
            CreateMap<GoldPriceDataVM, GoldPriceData>();
            CreateMap<Announcements, AnnouncementsVM>();
            CreateMap<AnnouncementsVM, Announcements>();
            CreateMap<GoldSettings, GoldSettingsVM>();
            CreateMap<GoldSettingsVM, GoldSettings>();
        }
    }
}
