using AutoMapper;
using IgloReservation.Api.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace IgloReservation.Api.Tests;

internal static class TestMapper
{
    public static IMapper Create() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance)
            .CreateMapper();
}
