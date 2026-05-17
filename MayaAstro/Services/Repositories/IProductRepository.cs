using MayaAstro.Models;

namespace MayaAstro.Services.Repositories
{
    public interface IProductRepository
    {
        #region Product

        Task<ApiResponseModel> GetProductList();
        Task<ApiResponseModel> GetProductsByCategory(int categoryId);
        Task<ApiResponseModel> AddProduct(ProductVM obj);
        Task<ApiResponseModel> GetProductById(int Id);
        Task<ApiResponseModel> DeleteProduct(int Id);

        #endregion Product

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
