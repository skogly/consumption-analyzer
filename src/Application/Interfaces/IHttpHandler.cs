using Application.ValueObjects;

namespace Application.Interfaces;

public interface IHttpHandler
{
    Task<T> GetAsync<T>(Uri uri) where T : class;
    Task<T> GetAsync<T>(Url url) where T : class;
}