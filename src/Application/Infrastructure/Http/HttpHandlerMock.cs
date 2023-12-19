using Application.Infrastructure.Dtos;
using Application.Infrastructure.Factories;
using Application.Interfaces;
using Application.ValueObjects;

namespace Application.Infrastructure.Http;

public class HttpHandlerMock : IHttpHandler
{
    public HttpHandlerMock()
    {
        System.Console.WriteLine("HttpHandlerMock ready to rock");
    }

    public Task<T> GetAsync<T>(Uri uri) where T : class
    {
        if (typeof(T) == typeof(string))
        {
            var response = "started and stopped";
            return Task.FromResult((T)(object)response);
        }
        else if (typeof(T) == typeof(HomeApplianceStatus))
        {
            var status = new HomeApplianceStatus
            {
                Name = "Home Appliance Mock",
                OnState = true
            };
            return Task.FromResult((T)(object)status);
        }
        else if (typeof(T) == typeof(HomeApplianceDto))
        {
            var dto = new HomeApplianceDto
            {
                Name = "Home Appliance Mock",
                OnState = "true"
            };
            return Task.FromResult((T)(object)dto);
        }
        else if (typeof(T) == typeof(List<PowerPriceDto>))
        {
            var powerPrices = new List<PowerPriceDto>();
            powerPrices.AddRange(Enumerable.Range(0, 24).Select(i => new PowerPriceDto
            {
                TimeStamp = DateTime.Today.AddHours(i),
                Price = 1
            }));
            return Task.FromResult((T)(object)powerPrices);
        }
        else
        {
            System.Console.WriteLine(typeof(T));
            throw new NotImplementedException();
        }
    }

    public async Task<T> GetAsync<T>(Url url) where T : class
    {
        return await GetAsync<T>(new Uri(url.ToString())).ConfigureAwait(false);
    }
}