using AutoMapper;
using IgloReservation.Api.DTOs.Auth;
using IgloReservation.Api.DTOs.IgloUnits;
using IgloReservation.Api.DTOs.Reservations;
using IgloReservation.Api.Models.Entities;

namespace IgloReservation.Api.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserResponse>()
            .ForMember(d => d.Roles, opt => opt.Ignore());

        CreateMap<Reservation, ReservationResponse>();
        CreateMap<CreateReservationRequest, Reservation>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.User, opt => opt.Ignore())
            .ForMember(d => d.IgloUnit, opt => opt.Ignore())
            .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.DeletedAtUtc, opt => opt.Ignore());
        CreateMap<UpdateReservationRequest, Reservation>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.UserId, opt => opt.Ignore())
            .ForMember(d => d.User, opt => opt.Ignore())
            .ForMember(d => d.IgloUnit, opt => opt.Ignore())
            .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.DeletedAtUtc, opt => opt.Ignore());

        CreateMap<IgloUnit, IgloUnitResponse>();
        CreateMap<CreateIgloUnitRequest, IgloUnit>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.DeletedAtUtc, opt => opt.Ignore())
            .ForMember(d => d.Reservations, opt => opt.Ignore());
        CreateMap<UpdateIgloUnitRequest, IgloUnit>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.DeletedAtUtc, opt => opt.Ignore())
            .ForMember(d => d.Reservations, opt => opt.Ignore());
    }
}
