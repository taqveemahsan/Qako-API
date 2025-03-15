using AutoMapper;
using AuditPilot.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AuthPilot.Models.Enums;


namespace AuthPilot.Models.AutoMapper
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Configure the mapping from RegisterClientRequest to Client

            CreateMap<ClientDto, Client>()
           .ForMember(dest => dest.Id, opt => opt.Ignore()) // Auto-generated ID
           .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
           .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
           .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
           .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
           .ForMember(dest => dest.IsActive, opt => opt.Ignore())
           .ForMember(dest => dest.CompanyType, opt => opt.MapFrom(src => (int)src.CompanyType));

            CreateMap<Client, ClientDto>()
                .ForMember(dest => dest.CompanyType, opt => opt.MapFrom(src => (CompanyType)src.CompanyType));

            CreateMap<ClientProject, ClientProjectdto>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

            CreateMap<ClientProjectdto, ClientProject>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

            // New mappings for UserProjectPermission and UserProjectPermissionDto
            CreateMap<UserProjectPermission, UserProjectPermissionDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.HasAccess, opt => opt.MapFrom(src => src.HasAccess));

            CreateMap<UserProjectPermissionDto, UserProjectPermission>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Auto-generated ID
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.HasAccess, opt => opt.MapFrom(src => src.HasAccess))
                .ForMember(dest => dest.AssignedOn, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore()) 
                .ForMember(dest => dest.Project, opt => opt.Ignore());
        }
    }
}
