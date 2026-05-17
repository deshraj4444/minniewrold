using MayaAstro.Models;

namespace MayaAstro.Services.Services
{
    public interface IProductServices
    {
        #region Products

        Task<ApiResponseModel> GetProductList();
        Task<ApiResponseModel> GetProductById(int Id);
        Task<ApiResponseModel> AddProduct(ProductVM obj);
        Task<ApiResponseModel> GetProductsByCategory(int categoryId);
        Task<ApiResponseModel> DeleteProduct(int Id);

        #endregion Products

        #region ProductDetails

        Task<ApiResponseModel> GetProductDetailsList();
        Task<ApiResponseModel> AddProductDetails(ProductDetailVM obj);
        Task<ApiResponseModel> GetProductDetailsById(int Id);
        Task<ApiResponseModel> ProductsExists(string Title);

        Task<ApiResponseModel> DeleteProductDetails(int Id);

        #endregion ProductDetails



        #region ProductSubCategory

        Task<ApiResponseModel> GetProductSubCategoryList();
        Task<ApiResponseModel> GetProductSubCategoryById(int Id);
        Task<ApiResponseModel> AddProductSubCategory(ProductSubCategoryVM obj);
        Task<ApiResponseModel> GetProductSubCategorysByCategory(int categoryId);
        Task<ApiResponseModel> DeleteProductSubCategory(int Id);

        #endregion ProductSubCategory
    }
}
