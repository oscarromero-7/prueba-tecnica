using System.Linq.Expressions;
using PruebaTecnica.Domain;

namespace PruebaTecnica.Application.Addresses;

public record AddressResponse(
    int Id,
    int UserId,
    string Street,
    string City,
    string Country,
    string? ZipCode,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static readonly Expression<Func<Address, AddressResponse>> Projection = a =>
        new AddressResponse(a.Id, a.UserId, a.Street, a.City, a.Country, a.ZipCode, a.CreatedAt, a.UpdatedAt);

    private static readonly Func<Address, AddressResponse> Map = Projection.Compile();

    public static AddressResponse FromEntity(Address address) => Map(address);
}
