using AutoMapper;
using MayaAstro.Models;
using Microsoft.EntityFrameworkCore;
using MayaAstro.DatabaseEntities;
using MayaAstro.Services.Configuration;
namespace MayaAstro.Services.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly MayaAstroContext _Context;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        public ProductRepository(MayaAstroContext context, IClock clock, IMapper mapper)
        {
            _Context = context;
            _clock = clock;
            _mapper = mapper;
        }
        #region Product


        public async Task<ApiResponseModel> GetProductList()
        {
            try
            {
                var data = await _Context.ProductCategory.OrderBy(p => p.Id).ToListAsync();
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
        public async Task<ApiResponseModel> GetProductsByCategory(int categoryId)
        {
            try
            {
                // Fetch the products that belong to the specified category
                var products = await _Context.ProductDetail
     .Where(p => p.ProductCategoryId == categoryId)
     .OrderByDescending(p => p.Id)
     .ToListAsync();


                if (products.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = products,
                        Message = "Data fetched successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                    };
                }

                // If no products found, return a success response with an empty list
                return new ApiResponseModel
                {
                    Data = new List<Product>(), // return an empty list
                    Message = "No products found for the selected category",
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
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> AddProduct(ProductVM obj)
        {
            try
            {
                var objmodel = await SaveProduct(obj);

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
        private async Task<Product> SaveProduct(ProductVM objmodel)
        {
            try
            {
                var blog = new Product();
                if (objmodel.Id > 0)
                {
                    blog = await _Context.ProductCategory.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (blog != null)
                    {
                        blog.Id = objmodel.Id;
                        _mapper.Map(objmodel, blog);
                    }
                    else
                    {
                        blog = _mapper.Map<Product>(objmodel);
                    }
                }
                if (blog.Id == 0)
                {
                    blog.CreatedDate = _clock.CurrentDateTime();
                    blog.CreatedBy = objmodel.CreatedBy;
                    blog = _mapper.Map<Product>(objmodel);
                    await _Context.ProductCategory.AddAsync(blog);

                }
                await _Context.SaveChangesAsync();
                return blog;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ApiResponseModel> GetProductById(int Id)
        {
            try
            {
                var data = await _Context.ProductCategory.Where(x => x.Id == Id).FirstOrDefaultAsync();
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
        public async Task<ApiResponseModel> DeleteProduct(int Id)
        {
            try
            {
                var data = await _Context.ProductCategory.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _Context.ProductCategory.Remove(data);
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

        #endregion Products
        #region ProductDetails

        public async Task<ApiResponseModel> GetProductDetailsList()
        {
            try
            {
                var data = await _Context.ProductDetail
                                   .GroupJoin(
                                       _Context.ProductCategory, // Join with ProductCategory
                                       product => product.ProductCategoryId, // Foreign key in ProductDetail
                                       category => category.Id, // Primary key in ProductCategory
                                       (product, categories) => new { product, categories }) // GroupJoin result
                                   .SelectMany(
                                       pc => pc.categories.DefaultIfEmpty(), // Left join, including products without categories
                                       (pc, category) => new
                                       {
                                           Id = pc.product.Id,
                                           Title = pc.product.Title,
                                           Description = pc.product.Description,
                                           BannerImageUrl = pc.product.BannerImageUrl,
                                           ProductCategoryId = pc.product.ProductCategoryId,
                                           ProductCategoryName = category != null ? category.ProductCategoryName : "Uncategorized", // Handle null category
                                           ProductMetaTitle = pc.product.ProductMetaTitle,
                                           ProductMetaDescription = pc.product.ProductMetaDescription,
                                           SeoTitle = pc.product.SeoTitle,
                                           ProductMetaContent = pc.product.ProductMetaContent,
                                           ProductDescription = pc.product.ProductDescription
                                       })
                                   .OrderByDescending(product => product.Id)
                                   .ToListAsync();

                if (data.Any()) // No need to check for null, just check if it has any elements
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
                    Message = "Something went wrong: " + ex.Message, // Including exception message for clarity
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest
                };
            }
        }


        public async Task<ApiResponseModel> AddProductDetails(ProductDetailVM obj)
        {
            try
            {
                var objmodel = await SaveProductDetails(obj);

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
        private async Task<ProductDetail> SaveProductDetails(ProductDetailVM objmodel)
        {
            try
            {
                var blog = new ProductDetail();
                if (objmodel.Id > 0)
                {
                    blog = await _Context.ProductDetail.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (blog != null)
                    {
                        blog.Id = objmodel.Id;
                        _mapper.Map(objmodel, blog);
                    }
                    else
                    {
                        blog = _mapper.Map<ProductDetail>(objmodel);
                    }
                }
                if (blog.Id == 0)
                {
                    blog.CreatedDate = _clock.CurrentDateTime();
                    blog.CreatedBy = objmodel.CreatedBy;
                    blog = _mapper.Map<ProductDetail>(objmodel);
                    await _Context.ProductDetail.AddAsync(blog);

                }
                await _Context.SaveChangesAsync();
                return blog;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<ApiResponseModel> GetProductDetailsById(int Id)
        {
            try
            {
                var data = await _Context.ProductDetail.Where(x => x.Id == Id).FirstOrDefaultAsync();
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
        public async Task<ApiResponseModel> DeleteProductDetails(int Id)
        {
            try
            {
                var data = await _Context.ProductDetail.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _Context.ProductDetail.Remove(data);
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

        public async Task<ApiResponseModel> ProductsExists(string Title)
        {
            try
            {
                var data = await _Context.ProductDetail.Where(x => x.Title == Title).FirstOrDefaultAsync();
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
        #endregion ProductDetails
        #region ProductSubCategory


        public async Task<ApiResponseModel> GetProductSubCategoryList()
        {
            try
            {
                var data = await _Context.ProductSubCategory
                                   .GroupJoin(
                                       _Context.ProductCategory, // Join with ProductCategory
                                       product => product.ProductCategoryId, // Foreign key in ProductDetail
                                       category => category.Id, // Primary key in ProductCategory
                                       (product, categories) => new { product, categories }) // GroupJoin result
                                   .SelectMany(
                                       pc => pc.categories.DefaultIfEmpty(), // Left join, including products without categories
                                       (pc, category) => new
                                       {
                                           Id = pc.product.Id,
                                           ProductSubCategoryName = pc.product.ProductSubCategoryName,
                                           ProductCategoryId = pc.product.ProductCategoryId,
                                           ProductCategoryName = category != null ? category.ProductCategoryName : "Uncategorized", // Handle null category

                                       })
                                   .OrderByDescending(product => product.Id)
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
        public async Task<ApiResponseModel> GetProductSubCategorysByCategory(int categoryId)
        {
            try
            {
                // Fetch the products that belong to the specified category
                var products = await _Context.ProductSubCategory
     //.Where(p => p.ProductCategoryId == categoryId)
     .OrderByDescending(p => p.Id)
     .ToListAsync();


                if (products.Any())
                {
                    return new ApiResponseModel()
                    {
                        Data = products,
                        Message = "Data fetched successfully",
                        StatusCode = Convert.ToInt32(System.Net.HttpStatusCode.OK),
                    };
                }

                // If no products found, return a success response with an empty list
                return new ApiResponseModel
                {
                    Data = new List<Product>(), // return an empty list
                    Message = "No products found for the selected category",
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
                    Status = false
                };
            }
        }

        public async Task<ApiResponseModel> AddProductSubCategory(ProductSubCategoryVM obj)
        {
            try
            {
                var objmodel = await SaveProductSubCategory(obj);

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
        private async Task<ProductSubCategory> SaveProductSubCategory(ProductSubCategoryVM objmodel)
        {
            try
            {
                var blog = new ProductSubCategory();
                if (objmodel.Id > 0)
                {
                    blog = await _Context.ProductSubCategory.FirstOrDefaultAsync(s => s.Id == objmodel.Id);
                    if (blog != null)
                    {
                        blog.Id = objmodel.Id;
                        _mapper.Map(objmodel, blog);
                    }
                    else
                    {
                        blog = _mapper.Map<ProductSubCategory>(objmodel);
                    }
                }
                if (blog.Id == 0)
                {
                    blog.CreatedDate = _clock.CurrentDateTime();
                    blog.CreatedBy = objmodel.CreatedBy;
                    blog = _mapper.Map<ProductSubCategory>(objmodel);
                    await _Context.ProductSubCategory.AddAsync(blog);

                }
                await _Context.SaveChangesAsync();
                return blog;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ApiResponseModel> GetProductSubCategoryById(int Id)
        {
            try
            {
                var data = await _Context.ProductSubCategory.Where(x => x.Id == Id).FirstOrDefaultAsync();
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
        public async Task<ApiResponseModel> DeleteProductSubCategory(int Id)
        {
            try
            {
                var data = await _Context.ProductSubCategory.FindAsync(Id);
                if (data == null)
                {
                    return new ApiResponseModel
                    {
                        Message = "No data found",
                        StatusCode = (int)System.Net.HttpStatusCode.OK,
                    };
                }
                _Context.ProductSubCategory.Remove(data);
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

        #endregion ProductSubCategory
    }
}