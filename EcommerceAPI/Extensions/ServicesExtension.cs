using EcommerceAPI.Services.IServices;
using EcommerceAPI.Services;

namespace EcommerceAPI.Extensions
{
    public static class ServicesExtension
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IShoppingCardService, ShoppingCardService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IReviewService, ReviewService>();
            services.AddTransient<IPromotionService, PromotionService>();
        }
    }
}
