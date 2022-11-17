using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redis;

        public BasketRepository(IDistributedCache redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        public async Task<ShoppingCart> GetBasket(string username)
        {
            var cart = await _redis.GetStringAsync(username);

            if (string.IsNullOrEmpty(cart)) return null;

            return JsonConvert.DeserializeObject<ShoppingCart>(cart);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart cart)
        {
            await _redis.SetStringAsync(cart.Username, JsonConvert.SerializeObject(cart));

            return await GetBasket(cart.Username);
        }

        public async Task DeleteBasket(string username)
        {
            await _redis.RemoveAsync(username);
        }
    }
}
