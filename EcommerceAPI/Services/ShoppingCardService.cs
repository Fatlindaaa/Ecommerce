using Amazon.Runtime.Internal.Util;
using AutoMapper;
using EcommerceAPI.Data;
using EcommerceAPI.Data.UnitOfWork;
using EcommerceAPI.Helpers;
using EcommerceAPI.Models.DTOs.Order;
using EcommerceAPI.Models.DTOs.ShoppingCard;
using EcommerceAPI.Models.Entities;
using EcommerceAPI.Services.IServices;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Product = EcommerceAPI.Models.Entities.Product;
using System.Text;
using EcommerceAPI.Models.DTOs.Promotion;
using EcommerceAPI.Models.DTOs.Product;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace EcommerceAPI.Services
{
    public class ShoppingCardService : IShoppingCardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ShoppingCardService> _logger;
        private readonly ICacheService _cacheService;
        private readonly IProductService _productService;
        private List<string> _keys;

        public ShoppingCardService(IUnitOfWork unitOfWork, IMapper mapper, IEmailSender emailSender, ILogger<ShoppingCardService> logger, ICacheService cacheService, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailSender = emailSender;
            _logger = logger;
            _cacheService = cacheService;
            _keys = new List<string>();
            _productService = productService;
        }

        private async Task<CartItem> GetCardItem(int itemId)
        {

            var cartItem = await _unitOfWork.Repository<CartItem>()
                .GetById(x => x.CartItemId == itemId)
                .AsNoTracking()
                .Include("Product")
                .FirstOrDefaultAsync();

            return cartItem;
        }

        public async Task AddProductToCard(string userId, int productId, int count)
        {
            
                var product = await _productService.GetProduct(productId);

                 if (product.Stock < count)
                 {
                     throw new Exception("Stock is not sufficient.");
                 }

                var shoppingCardItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Count = count
                };

                _unitOfWork.Repository<CartItem>().Create(shoppingCardItem);
                _unitOfWork.Complete();


                
                var cartItem = await GetCardItem(shoppingCardItem.CartItemId);

                //Check if the data is already in the cache
                var key = $"CartItems_{userId}";
                
                //Store the data in the cache
                _cacheService.SetDataMember(key, cartItem);
            
            
        }

        public async Task<ShoppingCardDetails> GetShoppingCardContentForUser(string userId)
        {
            try
            { 
                // Log the key
                var key = $"CartItems_{userId}";

                // Check if the data is already in the cache
                var usersShoppingCard = _cacheService.GetDataSet<CartItem>(key);

                // If not, then get the data from the database
                if (usersShoppingCard == null)
                {
                    usersShoppingCard = await _unitOfWork.Repository<CartItem>()
                                                                        .GetByCondition(x => x.UserId == userId)
                                                                        .Include(x => x.Product)
                                                                        .ToListAsync();
                }

                var shoppingCardList = new List<ShoppingCardViewDto>();
                foreach (CartItem item in usersShoppingCard)
                {
                    var currentProduct = item.Product;
                    var model = new ShoppingCardViewDto
                    {
                        ShoppingCardItemId = item.CartItemId,
                        ProductId = item.ProductId,

                       
                        ProductImage = currentProduct.ImageUrl,
                        ProductDescription = currentProduct.Description,
                        ProductName = currentProduct.Name,
                        ProductPrice = currentProduct.Price,
                        ShopingCardProductCount = item.Count,
                        Total = currentProduct.Price * item.Count
                    };

                    shoppingCardList.Add(model);
                }
                
                var shoppingCardDetails = new ShoppingCardDetails()
                {
                    ShoppingCardItems = shoppingCardList,
                    CardTotal = shoppingCardList.Select(x => x.Total).Sum()
                };
                return shoppingCardDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error while trying to get the shopping card content!");
                return new ShoppingCardDetails();
            }
        }
        public async Task RemoveProductFromCard(int shoppingCardItemId, string userId)
        {
            try
            {
                var cacheKey = $"CartItems_{userId}";
                var shoppingCardItem = await CheckRedisAndDatabaseForData(shoppingCardItemId, cacheKey);

                _unitOfWork.Repository<CartItem>().Delete(shoppingCardItem);
                _unitOfWork.Complete();
                       
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while trying to remove a product to card");
                throw new Exception("An error occurred while removing the item from the cart");

            }
        }

        public async Task RemoveAllProductsFromCard(string userId)
        {
            var key = $"CartItems_{userId}";

            var usersShoppingCard = _cacheService.GetDataSet<CartItem>(key);
            _cacheService.RemoveData(key);
            _unitOfWork.Repository<CartItem>().DeleteRange(usersShoppingCard);
            _unitOfWork.Complete();

        }
        public async Task IncreaseProductQuantityInShoppingCard(int shoppingCardItemId, string userId, int? newQuantity)
        {

            try
            {
                var cacheKey = $"CartItems_{userId}";
                var shoppingCardItem = await CheckRedisAndDatabaseForData(shoppingCardItemId, cacheKey);

                if (newQuantity == null)
                    shoppingCardItem.Count++;
                else
                    shoppingCardItem.Count = (int)newQuantity;
                
                _unitOfWork.Repository<CartItem>().Update(shoppingCardItem);
                _unitOfWork.Complete();
                var cartItem = await GetCardItem(shoppingCardItem.CartItemId);
                _cacheService.SetDataMember(cacheKey, cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while trying to add a product to card");
                throw new Exception("An error occurred while adding the item to the cart");

            }
        }
        public async Task DecreaseProductQuantityInShoppingCard(int shoppingCardItemId,string userId, int? newQuantity)
        {
            try
            {
                var cacheKey = $"CartItems_{userId}";
                var shoppingCardItem = await CheckRedisAndDatabaseForData(shoppingCardItemId, cacheKey);

                if (shoppingCardItem == null)
                {
                    throw new Exception("Cart item not found in the database.");
                }
                if (newQuantity == null)
                    shoppingCardItem.Count--;
                else
                    shoppingCardItem.Count = (int)newQuantity;

                _unitOfWork.Repository<CartItem>().Update(shoppingCardItem);
                _unitOfWork.Complete();
                var cartItem = await GetCardItem(shoppingCardItem.CartItemId);
                _cacheService.SetDataMember(cacheKey, cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while trying to remove one product to card");
                throw new Exception("An error occurred while removing the item to the cart");

            }
        }
        private async Task<CartItem> CheckRedisAndDatabaseForData(int shoppingCardItemId, string cachekey)
        {
            CartItem? shoppingCardItem = null;
            var usersShoppingCard = _cacheService.GetDataSet<CartItem>(cachekey);
            if (usersShoppingCard != null)
            {
                foreach (var item in usersShoppingCard)
                {
                    if (item.CartItemId == shoppingCardItemId)
                    {
                        shoppingCardItem = item;
                        _cacheService.RemoveDataFromSet(cachekey, item);
                    }
                }
            }

            if (shoppingCardItem == null)
            {
                shoppingCardItem = await _unitOfWork.Repository<CartItem>()
                                                    .GetById(x => x.CartItemId == shoppingCardItemId)
                                                    .FirstOrDefaultAsync();
            }
            return shoppingCardItem;

        }       

    }
}
