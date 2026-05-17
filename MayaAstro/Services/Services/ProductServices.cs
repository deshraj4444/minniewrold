using MayaAstro.Models;
using MayaAstro.Services.Repositories;

namespace MayaAstro.Services.Services
{
    public class ProductServices : IProductServices
    {
        private readonly IProductRepository _iproductRepository;
        private readonly ICryptoServices _cryptoServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductServices(IProductRepository iproductRepository, ICryptoServices cryptoServices, IHttpContextAccessor httpContextAccessor)
        {
            _iproductRepository = iproductRepository;
            _httpContextAccessor = httpContextAccessor;
            _cryptoServices = cryptoServices;
        }
        #region Products

        public async Task<ApiResponseModel> GetProductList()
        {
            return await _iproductRepository.GetProductList();
        }
        public async Task<ApiResponseModel> GetProductsByCategory(int categoryId)
        {
            return await _iproductRepository.GetProductsByCategory(categoryId);
        }
        public async Task<ApiResponseModel> AddProduct(ProductVM obj)
        {
            return await _iproductRepository.AddProduct(obj);
        }
        public async Task<ApiResponseModel> GetProductById(int Id)
        {
            return await _iproductRepository.GetProductById(Id);
        }
        public async Task<ApiResponseModel> DeleteProduct(int Id)
        {
            return await _iproductRepository.DeleteProduct(Id);
        }

        #endregion products

        #region ProductDetails

        public async Task<ApiResponseModel> GetProductDetailsList()
        {
            return await _iproductRepository.GetProductDetailsList();
        }
        public async Task<ApiResponseModel> AddProductDetails(ProductDetailVM obj)
        {
            return await _iproductRepository.AddProductDetails(obj);
        }
        public async Task<ApiResponseModel> GetProductDetailsById(int Id)
        {
            return await _iproductRepository.GetProductDetailsById(Id);
        }

        public async Task<ApiResponseModel> ProductsExists(string Title)
        {
            return await _iproductRepository.ProductsExists(Title);
        }
        public async Task<ApiResponseModel> DeleteProductDetails(int Id)
        {
            return await _iproductRepository.DeleteProductDetails(Id);

        }
        #endregion ProductDetails


        #region ProductSubCategory

        public async Task<ApiResponseModel> GetProductSubCategoryList()
        {
            return await _iproductRepository.GetProductSubCategoryList();
        }
        public async Task<ApiResponseModel> GetProductSubCategorysByCategory(int categoryId)
        {
            return await _iproductRepository.GetProductSubCategorysByCategory(categoryId);
        }
        public async Task<ApiResponseModel> AddProductSubCategory(ProductSubCategoryVM obj)
        {
            return await _iproductRepository.AddProductSubCategory(obj);
        }
        public async Task<ApiResponseModel> GetProductSubCategoryById(int Id)
        {
            return await _iproductRepository.GetProductSubCategoryById(Id);
        }
        public async Task<ApiResponseModel> DeleteProductSubCategory(int Id)
        {
            return await _iproductRepository.DeleteProductSubCategory(Id);
        }

        #endregion ProductSubCategory

    }
}
