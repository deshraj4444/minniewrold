using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MayaAstro.Services.Repositories
{
	public class BlogRepository : IBlogRepository
	{
		private readonly MayaAstroContext _Context;
		private readonly IClock _clock;
		private readonly IMapper _mapper;

		public BlogRepository(MayaAstroContext context, IClock clock, IMapper mapper)
		{
			_Context = context;
			_clock = clock;
			_mapper = mapper;
		}



		#region Add Blog Details


		public async Task<ApiResponseModel> BlogListImage(string? categoryName, int pageSize, int pageNo, string search, int domainId, int? typeId = null)
		{
			try
			{
				// Calculate the number of records to skip
				int skip = (pageNo - 1) * pageSize;
				string sanitizedSearch = string.IsNullOrEmpty(search) ? search : search.Trim();
				var data = await (from s in _Context.BlogDetail
								  join c in _Context.BlogCategory on s.BlogCategoryId equals c.Id
								  into categoryJoin
								  from c in categoryJoin.DefaultIfEmpty()
								  where s.IsPublished == 1
								  && s.WebsiteId == domainId
								  && (!typeId.HasValue || s.TypeId == typeId.Value)
										&& (string.IsNullOrEmpty(categoryName) || c.BlogCategoryName == categoryName)
								   && (string.IsNullOrEmpty(sanitizedSearch) || (s.Title ?? string.Empty).ToLower().Contains(sanitizedSearch.ToLower()))

								  select new BlogListImageVM
								  {
									  Id = s.Id,
									  Title = s.Title,
									  ThumbnailImageUrl = s.ThumbnailImageUrl,
									  PublishDate = s.PublishDate,
									  PageUrl = s.PageUrl,
									  CategoryName = c.BlogCategoryName,
									  Author = s.Author,
									  AuthorImage = s.ButtonUrl,
								  })
								  .OrderByDescending(x => x.Id)
								  .Skip(skip)   
								  .Take(pageSize) 
								  .ToListAsync();

				if (data.Any())
				{
					return new ApiResponseModel()
					{
						Data = data,
						Message = "Data fetched successfully",
						StatusCode = (int)System.Net.HttpStatusCode.OK
					};
				}

				return new ApiResponseModel
				{
					Message = "No data found",
					StatusCode = (int)System.Net.HttpStatusCode.OK
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel()
				{
					Data = null,
					Message = "Something went wrong: " + ex.Message,
					StatusCode = (int)System.Net.HttpStatusCode.BadRequest
				};
			}
		}

		public async Task<ApiResponseModel> HomeBlogList(int domainId, string? categoryName  )
		{
			try
			{
				var data = await (from s in _Context.BlogDetail
								  join c in _Context.BlogCategory on s.BlogCategoryId equals c.Id
								  into categoryJoin
								  from c in categoryJoin.DefaultIfEmpty()
								  where s.IsPublished == 1 && s.WebsiteId== domainId && (string.IsNullOrEmpty(categoryName) || c.BlogCategoryName == categoryName) // 1 define to publish blogs
								  select new BlogListImageVM
								  {
									  Id = s.Id,
									  Title = s.Title,
									  ThumbnailImageUrl = s.ThumbnailImageUrl,
									  PublishDate = s.PublishDate,
									  PageUrl = s.PageUrl,
									  CategoryName = c.BlogCategoryName,
								  }).OrderByDescending(x => x.Id).Take(7).ToListAsync();

				if (data.Any())
				{
					return new ApiResponseModel()
					{
						Data = data,
						Message = "Data fetched successfully",
						StatusCode = (int)System.Net.HttpStatusCode.OK
					};
				}

				return new ApiResponseModel
				{
					Message = "No data found",
					StatusCode = (int)System.Net.HttpStatusCode.OK
				};
			}
			catch (Exception ex)
			{
				return new ApiResponseModel()
				{
					Data = null,
					Message = "Something went wrong: " + ex.Message,
					StatusCode = (int)System.Net.HttpStatusCode.BadRequest
				};
			}
		}

        public async Task<ApiResponseModel> GetBlogDetailList(int typeId, int domainId)
        {
            try
            {
                var data = await _Context.BlogDetail
                    .Where(blog => blog.TypeId == typeId  && blog.IsDeleted == false)
                    .GroupJoin(
                        _Context.BlogCategory,
                        blog => blog.BlogCategoryId,
                        category => category.Id,
                        (blog, categories) => new { blog, categories }
                    )
                    .SelectMany(
                        bc => bc.categories.DefaultIfEmpty(),
                        (bc, category) => new BlogDetailVM
                        {
                            Id = bc.blog.Id,
                            Title = bc.blog.Title,
                            Description = bc.blog.Description,
                            BannerImageUrl = bc.blog.BannerImageUrl,
                            ThumbnailImageUrl = bc.blog.ThumbnailImageUrl,
                            BlogCategoryId = bc.blog.BlogCategoryId,
                            Author = bc.blog.Author,
                            ButtonUrl = bc.blog.ButtonUrl,
                            PublishDate = bc.blog.PublishDate,
                            PageUrl = bc.blog.PageUrl,
                            WebsiteId = bc.blog.WebsiteId,
                            BlogMetaTitle = bc.blog.BlogMetaTitle,
                            BlogMetaDescription = bc.blog.BlogMetaDescription,
                            BlogMetaKeyword = bc.blog.BlogMetaKeyword,
                            BlogMetaContent = bc.blog.BlogMetaContent,
                            SeoTitle = bc.blog.SeoTitle,
                            BannerAltText = bc.blog.BannerAltText,
                            IsPublished = bc.blog.IsPublished,
                            Tag = bc.blog.Tag,
                            SortOrder = bc.blog.SortOrder,
                            CreatedBy = bc.blog.CreatedBy,
                            CreatedDate = bc.blog.CreatedDate,
                            ModifiedBy = bc.blog.ModifiedBy,
                            ModifiedDate = bc.blog.ModifiedDate,
                            IsActive = bc.blog.IsActive,
                            TypeId = bc.blog.TypeId,
                            IsDeleted = bc.blog.IsDeleted,
                            OgiImage = bc.blog.OgiImage,
                            ButtonText = bc.blog.ButtonText,
                            IsShowButton = bc.blog.IsShowButton,
                            BlogCategoryName = category != null ? category.BlogCategoryName : "Uncategorized"
                        }
                    )
                    .OrderByDescending(blog => blog.Id)
                    .ToListAsync();

                return new ApiResponseModel
                {
                    Data = data,
                    Message = data.Any() ? "Data fetched successfully" : "No data found",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    StatusCode = 400
                };
            }
        }



        public async Task<ApiResponseModel> AddBlogDetail(BlogDetailVM obj)
		{
			try
			{
				var objmodel = await SaveBlogDetail(obj);

				return new ApiResponseModel()
				{
					Data = obj,
					Message = "Data save successfully",
					StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
				};
			}
			catch
			{
				return new ApiResponseModel()
				{
					Data = null,
					Message = "Something went wrong",
					StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
				};
			}
		}
		private async Task<BlogDetail> SaveBlogDetail(BlogDetailVM objmodel)
		{
			try
			{
				BlogDetail blog;
				if (objmodel.Id > 0)
				{
					// Find the existing blog entry
					blog = await _Context.BlogDetail.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
					if (blog != null)
					{
						// Map the changes from objmodel to the existing blog entry
						_mapper.Map(objmodel, blog);
						// Ensure Id is set correctly
						blog.Id = objmodel.Id;
						blog.ModifiedDate = _clock.CurrentDateTime();
						blog.PublishDate = DateTime.Now.ToUniversalTime();
						blog.CreatedDate = _clock.CurrentDateTime();
					}
					else
					{
						// Map to a new BlogDetail instance if not found
						blog = _mapper.Map<BlogDetail>(objmodel);
						blog.Id = objmodel.Id;
					}
				}
				else
				{
					// Create a new BlogDetail instance
					blog = _mapper.Map<BlogDetail>(objmodel);
					blog.CreatedDate = _clock.CurrentDateTime();
					blog.PublishDate = DateTime.Now.ToUniversalTime();
					blog.CreatedBy = objmodel.CreatedBy;

					try
					{
						await _Context.BlogDetail.AddAsync(blog);
						await _Context.SaveChangesAsync();
						Console.WriteLine("Data saved successfully.");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error saving blog: {ex.Message}");
						throw;
					}
				}

				// Save changes to the database
				await _Context.SaveChangesAsync();
				return blog;
			}
			catch
			{
				// Handle the exception as needed, such as logging
				return null;
			}
		}
		public async Task<ApiResponseModel> GetBlogDetailById(int Id)
		{
			try
			{
				var data = await _Context.BlogDetail.Where(x => x.Id == Id).FirstOrDefaultAsync();
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
		public async Task<ApiResponseModel> BlogExists(string title)
		{
			try
			{
				var data = await _Context.BlogDetail.Where(x => x.Title == title).FirstOrDefaultAsync();
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
			catch
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
		public async Task<ApiResponseModel> DeleteBlogDetail(int Id)
		{
			try
			{
				var data = await _Context.BlogDetail.FindAsync(Id);
				if (data == null)
				{
					return new ApiResponseModel
					{
						Message = "No data found",
						StatusCode = (int)System.Net.HttpStatusCode.OK,
					};
				}
				_Context.BlogDetail.Remove(data);
				await _Context.SaveChangesAsync();
				return new ApiResponseModel()
				{
					Data = data,
					Message = "Data Deleted successfully",
					StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),

				};
			}
			catch
			{
				return new ApiResponseModel()
				{
					Data = null,
					Message = "Something went wrong",
					StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.BadRequest),
				};
			}
		}

		public async Task<ApiResponseModel> GetBlogDetailByUrl(string slug)
		{
			try
			{


				var data = await _Context.BlogDetail
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

		#endregion Add Blog Details

		#region BlogCategory

		public async Task<ApiResponseModel> GetBlogCategoryList(int typeId)
		{
			try
			{
				var data = await _Context.BlogCategory
					.Where(item=>item.TypeId== typeId)
					.OrderByDescending(item => item.Id).ToListAsync();
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


		public async Task<ApiResponseModel> GetBlogCategoryUserList(int domainId)
		{
			try
			{
				var data = await _Context.BlogCategory
					.Where(item => item.WebsiteId == domainId)
					.OrderByDescending(item => item.Id).ToListAsync();
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
		public async Task<ApiResponseModel> AddBlogCategory(BlogCategoryVM obj)
		{
			try
			{
				var objmodel = await SaveBlogCategory(obj);

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
		private async Task<BlogCategory> SaveBlogCategory(BlogCategoryVM objmodel)
		{
			try
			{
				var blogCategory = new BlogCategory();
				if (objmodel.Id > 0)
				{
					blogCategory = await _Context.BlogCategory.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
					if (blogCategory != null)
					{
						blogCategory.Id = objmodel.Id;
						_mapper.Map(objmodel, blogCategory);
					}
					else
					{
						blogCategory = _mapper.Map<BlogCategory>(objmodel);
					}
				}
				if (blogCategory.Id == 0)
				{
					blogCategory.CreatedDate = _clock.CurrentDateTime();
					blogCategory.CreatedBy = objmodel.CreatedBy;
					blogCategory = _mapper.Map<BlogCategory>(objmodel);
					await _Context.BlogCategory.AddAsync(blogCategory);

				}
				await _Context.SaveChangesAsync();
				return blogCategory;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public async Task<ApiResponseModel> GetBlogCategoryById(int Id)
		{
			try
			{
				var data = await _Context.BlogCategory.Where(x => x.Id == Id).FirstOrDefaultAsync();
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
		public async Task<ApiResponseModel> DeleteBlogCategory(int Id)
		{
			try
			{
				var data = await _Context.BlogCategory.FindAsync(Id);
				if (data == null)
				{
					return new ApiResponseModel
					{
						Message = "No data found",
						StatusCode = (int)System.Net.HttpStatusCode.OK,
					};
				}
				_Context.BlogCategory.Remove(data);
				await _Context.SaveChangesAsync();
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

		#endregion BlogCategory
	}
}
